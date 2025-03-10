using DV;
using DV.Utils;
using Signals.Common;
using Signals.Game.Controllers;
using Signals.Game.Curves;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace Signals.Game
{
    public class SignalManager : SingletonBehaviour<SignalManager>
    {
        private enum SignalCreationMode
        {
            None,
            Mainline,
            IntoYard,
            IntoYardReverse,
            Shunting
        }

        private const string YardNameStart = "[Y]";
        private const int LaserPointerTargetLayer = 15;
        private const float DeadEndThreshold = 100.0f;
        private const float ClosenessThreshold = 50.0f;
        private const float UpdateTime = 1.0f;
        
        private static Transform? _holder;
        private static bool _loaded = false;

        internal static SignalPack DefaultPack = null!;
        internal static Dictionary<string, SignalPack> InstalledPacks = new Dictionary<string, SignalPack>();

        internal static Transform Holder
        {
            get
            {
                if (_holder == null)
                {
                    var go = new GameObject("[Signal Holder]");
                    go.SetActive(false);
                    DontDestroyOnLoad(go);
                    _holder = go.transform;
                }

                return _holder;
            }
        }

        private Dictionary<Junction, JunctionSignalGroup> _junctionSignals =
            new Dictionary<Junction, JunctionSignalGroup>();
        private List<DistantSignalController> _distantSignals =
            new List<DistantSignalController>();
        private List<BasicSignalController> _signalRegister =
            new List<BasicSignalController>();

        private Coroutine? _updateCoro;

        public List<BasicSignalController> AllSignals => _signalRegister;

        public new static string AllowAutoCreate()
        {
            return "[SignalManager]";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _junctionSignals.Clear();
            _distantSignals.Clear();

            StopCoroutine(_updateCoro);
        }

        #region Mod Loading

        internal static SignalPack GetCurrentPack()
        {
            if (InstalledPacks.TryGetValue(SignalsMod.Settings.CustomPack, out var pack))
            {
                return pack;
            }

            if (!string.IsNullOrEmpty(SignalsMod.Settings.CustomPack))
            {
                SignalsMod.Error($"Could not find pack '{SignalsMod.Settings.CustomPack}', using default signals.");
            }

            return DefaultPack;
        }

        internal static void LoadSignals(UnityModManager.ModEntry mod)
        {
            AssetBundle bundle;
            var files = Directory.EnumerateFiles(mod.Path, Constants.Bundle, SearchOption.AllDirectories);

            foreach (var file in files)
            {
                bundle = AssetBundle.LoadFromFile(file);

                // Somehow failed to load the bundle.
                if (bundle == null)
                {
                    SignalsMod.Error("Failed to load bundle!");
                    continue;
                }

                var pack = bundle.LoadAllAssets<SignalPack>().FirstOrDefault();

                if (pack != null)
                {
                    ProcessSignals(pack);

                    if (DefaultPack == null)
                    {
                        DefaultPack = pack;
                        SignalsMod.Log("Loaded default pack.");
                    }
                    else
                    {
                        InstalledPacks.Add(mod.Info.Id, pack);
                    }

                    bundle.Unload(false);
                    break;
                }

                bundle.Unload(false);
            }
        }

        internal static void UnloadSignals(UnityModManager.ModEntry mod)
        {
            if (InstalledPacks.ContainsKey(mod.Info.Id))
            {
                InstalledPacks.Remove(mod.Info.Id);
            }
        }

        private static void ProcessSignals(SignalPack pack)
        {
            foreach (var item in pack.AllSignals)
            {
                item.gameObject.layer = LaserPointerTargetLayer;
                item.gameObject.AddComponent<SignalHover>();
            }
        }

        #endregion

        #region Signal Creation

        internal static void CheckStartCreation(string msg, bool isError, float percent)
        {
            // Reset for reloads.
            if (percent < 60)
            {
                _loaded = false;
                return;
            }

            if (_loaded) return;

            DisplayLoadingThingy();
            Instance.CreateSignals();
            _loaded = true;
        }

        private static void DisplayLoadingThingy()
        {
            // Thanks Skin Manager.
            var info = FindObjectOfType<DisplayLoadingInfo>();

            if (info != null)
            {
                info.loadProgressTMP.richText = true;
                info.loadProgressTMP.text += "\ncreating <color=#FF3333>si</color><color=#FFCC00>gna</color><color=#33FF77>ls</color>";
            }
        }

        private void CreateSignals()
        {
            SignalsMod.Log("Started creating signals...");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            int count = 0;
            var pack = GetCurrentPack();            

            foreach (var junction in WorldData.Junctions)
            {
                switch (ShouldMakeSignal(junction))
                {
                    case SignalCreationMode.Mainline:
                        _junctionSignals.Add(junction, CreateMainlineSignals(pack, junction));
                        break;
                    case SignalCreationMode.IntoYard:
                        _junctionSignals.Add(junction, CreateIntoYardSignals(pack, junction));
                        break;
                    case SignalCreationMode.IntoYardReverse:
                        _junctionSignals.Add(junction, CreateIntoYardReverseSignals(pack, junction));
                        break;
                    case SignalCreationMode.Shunting:
                        count += CreateShuntingSignals(pack, junction);
                        break;
                    default:
                        continue;
                }
            }

            sw.Stop();
            SignalsMod.Log($"Finished creating signals for {_junctionSignals.Count} junction(s), and {count} shunting signal pair(s) ({sw.Elapsed.TotalSeconds:F4}s)");
            sw.Restart();

            // Wish this could be done within the same loop but alas.
            count = MergeCloseSignals(pack);

            sw.Stop();
            SignalsMod.Log($"Merged {count} signal(s) ({sw.Elapsed.TotalSeconds:F4}s)");
            sw.Restart();

            count = CreateDistantSignals(pack);

            sw.Stop();
            SignalsMod.Log($"Finished creating {count} distant signal(s) ({sw.Elapsed.TotalSeconds:F4}s)");
            SignalsMod.Log($"Total signal count: {_signalRegister.Count}");

            TrackChecker.StartBuildingMap();

            _updateCoro = StartCoroutine(UpdateRoutine());
        }

        private int CreateDistantSignals(SignalPack pack)
        {
            if (pack.DistantSignal == null) return 0;

            int count = 0;

            foreach (var junction in _junctionSignals)
            {
                DistantSignalController? distant;

                // Create a distant signal for the in branch.
                var signal = junction.Value.OutBranchesSignal;

                if (signal != null && (signal.Type == SignalType.Mainline || signal.Type == SignalType.IntoYard))
                {
                    distant = CreateDistantSignalIn(signal, pack);

                    if (distant != null)
                    {
                        _distantSignals.Add(distant);
                        count++;
                    }
                }

                // Create a distant signal for each out branch.
                signal = junction.Value.InBranchSignal;

                if (signal != null && (signal.Type == SignalType.Mainline || signal.Type == SignalType.IntoYard))
                {
                    for (int i = 0; i < junction.Key.outBranches.Count; i++)
                    {
                        distant = CreateDistantSignalOut(signal, pack, i);

                        if (distant != null)
                        {
                            _distantSignals.Add(distant);
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        #region Creation testing

        private SignalCreationMode ShouldMakeSignal(Junction junction)
        {
            // Signals only at juntions, don't duplicate junctions, don't make them at short dead ends.
            if (_junctionSignals.ContainsKey(junction) || InIsShortDeadEnd(junction))
            {
                return SignalCreationMode.None;
            }

            var inName = junction.inBranch.track.name;
            SignalsMod.LogVerbose($"Testing track '{inName}' for signals...");

            // If the in track belongs to a yard...
            if (inName.StartsWith(YardNameStart))
            {
                // Check all out branches.
                foreach (var branch in junction.outBranches)
                {
                    // Track ends after a switch for some reason, plop signal.
                    if (!branch.track.outIsConnected)
                    {
                        return SignalCreationMode.Mainline;
                    }

                    // Get the track after the switch track.
                    var outName = branch.track.outBranch.track.name;

                    SignalsMod.LogVerbose($"Testing branch '{outName}' for signals...");

                    // If this yard track goes to a non yard track...
                    if (!outName.StartsWith(YardNameStart))
                    {
                        return SignalCreationMode.IntoYardReverse;
                    }
                }

                // Switch branches stay in the same yard, no signal needed.
                return ShuntingSignalReturn();
            }

            // In case we are in a mainline, check all out branches for yards.
            foreach (var branch in junction.outBranches)
            {
                // Track ends after a switch for some reason, plop signal.
                if (!branch.track.outIsConnected)
                {
                    return SignalCreationMode.Mainline;
                }

                // Get the track after the switch track.
                var outName = branch.track.outBranch.track.name;

                SignalsMod.LogVerbose($"Testing branch '{outName}' for signals...");

                // If this non yard track goes to a yard track...
                if (outName.StartsWith(YardNameStart))
                {
                    return SignalCreationMode.IntoYard;
                }
            }

            // No conditions met, plop signal.
            return SignalCreationMode.Mainline;
        }

        private static bool InIsShortDeadEnd(Junction junction)
        {
            var track = junction.inBranch.track;

            if (track.logicTrack.length > DeadEndThreshold)
            {
                return false;
            }

            var branch = track.inJunction == junction ?
                track.GetOutBranch() :
                track.GetInBranch();

            return branch == null;
        }

        private static SignalCreationMode ShuntingSignalReturn() => SignalsMod.Settings.GenerateShuntingSignals ?
            SignalCreationMode.Shunting :
            SignalCreationMode.None;

        #endregion

        #region Main creation methods

        private static JunctionSignalGroup CreateMainlineSignals(SignalPack pack, Junction junction)
        {
            var signals = CreateJunctionSignals(pack.Signal, junction);

            foreach (var signal in signals.AllSignals)
            {
                signal.Type = SignalType.Mainline;
            }

            return signals;
        }

        private static JunctionSignalGroup CreateIntoYardSignals(SignalPack pack, Junction junction)
        {
            var signals = CreateJunctionSignals(pack.IntoYardSignal, pack.Signal, junction);

            foreach (var signal in signals.AllSignals)
            {
                signal.Type = SignalType.IntoYard;
            }

            // The signal facing the in branch does not point to the yard.
            if (signals.InBranchSignal != null)
            {
                signals.InBranchSignal.Type = SignalType.Mainline;
            }

            return signals;
        }

        private static JunctionSignalGroup CreateIntoYardReverseSignals(SignalPack pack, Junction junction)
        {
            var signals = CreateJunctionSignals(pack.Signal, pack.IntoYardSignal, junction);

            foreach (var signal in signals.AllSignals)
            {
                signal.Type = SignalType.Mainline;
            }

            // The signal facing the in branch does not point to the yard.
            if (signals.InBranchSignal != null)
            {
                signals.InBranchSignal.Type = SignalType.IntoYard;
            }

            return signals;
        }

        private static DistantSignalController? CreateDistantSignalIn(JunctionSignalController junctionSignal, SignalPack pack)
        {
            var track = junctionSignal.Junction.inBranch.track;

            if (track.logicTrack.length < pack.DistantSignalMinimumTrackLength) return null;

            SignalsMod.LogVerbose($"Making distant signal [in] for signal '{junctionSignal.Name}'");

            bool dir = track.inJunction == junctionSignal.Junction;

            var (point, forward, distance) = dir ?
                BezierHelper.GetAproxPointAtLength(track.curve, pack.DistantSignalDistance) :
                BezierHelper.GetAproxPointAtLengthReverse(track.curve, pack.DistantSignalDistance);

            var signal = Instantiate(pack.DistantSignal!, track.curve.transform, false);

            signal.transform.position = point;
            signal.transform.localRotation = Quaternion.LookRotation(dir ? forward : -forward);

            return new DistantSignalController(junctionSignal, signal, distance);
        }

        private static DistantSignalController? CreateDistantSignalOut(JunctionSignalController junctionSignal, SignalPack pack, int branch)
        {
            var branchTrack = junctionSignal.Junction.outBranches[branch].track;
            var track = branchTrack.outBranch.track;

            if (track.logicTrack.length < pack.DistantSignalMinimumTrackLength) return null;

            SignalsMod.LogVerbose($"Making distant signal [b:{branch}] for signal '{junctionSignal.Name}'");

            bool dir = track.inBranch.track == branchTrack;

            var (point, forward, distance) = dir ?
                BezierHelper.GetAproxPointAtLength(track.curve, pack.DistantSignalDistance) :
                BezierHelper.GetAproxPointAtLengthReverse(track.curve, pack.DistantSignalDistance);

            var signal = Instantiate(pack.DistantSignal!, track.curve.transform, false);

            signal.transform.position = point;
            signal.transform.localRotation = Quaternion.LookRotation(dir ? forward : -forward);

            return new DistantSignalController(junctionSignal, signal, distance);
        }

        private static int CreateShuntingSignals(SignalPack pack, Junction junction)
        {
            if (pack.ShuntingSignal == null) return 0;

            int count = 0;

            foreach (var branch in junction.outBranches)
            {
                if (branch.track == null ||
                    branch.track.outBranch == null ||
                    branch.track.outBranch.track == null ||
                    branch.track.outBranch.track.logicTrack.length < 25) continue;

                CreateSignalAtPoint(pack.ShuntingSignal, branch.track.curve.Last(), TrackDirection.In, -17);
                CreateSignalAtPoint(pack.ShuntingSignal, branch.track.curve.Last(), TrackDirection.Out, -16);
                count++;
            }

            return count;
        }

        #endregion

        #region Internal creation methods

        private static JunctionSignalGroup CreateJunctionSignals(SignalControllerDefinition signal, Junction junction)
        {
            return CreateDoubleJunctionSignals(signal, signal, junction);
        }

        private static JunctionSignalGroup CreateJunctionSignals(SignalControllerDefinition? outSignal, SignalControllerDefinition? inSignal, Junction junction)
        {
            if (outSignal != null)
            {
                if (inSignal != null)
                {
                    return CreateDoubleJunctionSignals(outSignal, inSignal, junction);
                }

                return CreateSingleJunctionSignalReverse(outSignal, junction);
            }

            if (inSignal != null)
            {
                return CreateSingleJunctionSignal(inSignal, junction);
            }

            throw new System.Exception("Tried creating JunctionSignalPair from 2 nulls, this cannot work!");
        }

        private static JunctionSignalGroup CreateDoubleJunctionSignals(SignalControllerDefinition outSignal, SignalControllerDefinition inSignal, Junction junction)
        {
            SignalsMod.LogVerbose($"Making junction signals for track '{junction.inBranch.track.name}'");

            var outTrack = junction.outBranches[0].track;

            // Use the anchor point as the placement root. Signal is offset to the side by a custom value specified
            // in the pack, and then backwards by a few metres to look better. Also keeps it out of the way from
            // switch stands this way. It'll block the stand from the branches side but that is fine. This does
            // however mean that if the track length is very short, it can look odd.
            var point = outTrack.curve[0];
            var backward = -point.handle2.normalized;

            var to = Instantiate(outSignal, point.transform, false);
            var from = Instantiate(inSignal, point.transform, false);

            to.transform.localPosition = backward;
            from.transform.localPosition = -4.0f * backward;
            to.transform.localRotation = Quaternion.LookRotation(-point.handle2);
            from.transform.localRotation = Quaternion.LookRotation(point.handle2);

            return new JunctionSignalGroup(junction,
                new JunctionSignalController(to, junction, TrackDirection.Out),
                new JunctionSignalController(from, junction, TrackDirection.In));
        }

        private static JunctionSignalGroup CreateSingleJunctionSignal(SignalControllerDefinition inSignal, Junction junction)
        {
            SignalsMod.LogVerbose($"Making junction signals for track '{junction.inBranch.track.name}'");

            var outTrack = junction.outBranches[0].track;

            // Similar to the previous method, but only creates a single signal (the one facing the in branch, so "out" of the yard).
            var point = outTrack.curve[0];
            var backward = -point.handle2.normalized;

            var from = Instantiate(inSignal, point.transform, false);

            from.transform.localPosition = -4.0f * backward;
            from.transform.localRotation = Quaternion.LookRotation(point.handle2);

            return new JunctionSignalGroup(junction, null, new JunctionSignalController(from, junction, TrackDirection.In));
        }

        private static JunctionSignalGroup CreateSingleJunctionSignalReverse(SignalControllerDefinition outSignal, Junction junction)
        {
            SignalsMod.LogVerbose($"Making junction signals for track '{junction.inBranch.track.name}'");

            var outTrack = junction.outBranches[0].track;

            // Similar to the previous method, but only creates a single signal (the one facing the out branch).
            var point = outTrack.curve[0];
            var backward = -point.handle2.normalized;

            var to = Instantiate(outSignal, point.transform, false);

            to.transform.localPosition = backward;
            to.transform.localRotation = Quaternion.LookRotation(-point.handle2);

            return new JunctionSignalGroup(junction, new JunctionSignalController(to, junction, TrackDirection.Out), null);
        }

        private static BasicSignalController CreateSignalAtPoint(SignalControllerDefinition def, BezierPoint point, TrackDirection dir, float offset)
        {
            var backward = point.handle1.normalized;
            var signal = Instantiate(def, point.transform, false);

            signal.transform.localPosition = backward * offset;
            signal.transform.localRotation = Quaternion.LookRotation(dir.IsOut() ? point.handle1 : -point.handle1);

            return new BasicSignalController(signal);
        }

        #endregion

        #region Post processing

        private int MergeCloseSignals(SignalPack pack)
        {
            HashSet<Junction> merged = new HashSet<Junction>();
            var pairs = _junctionSignals.ToList();

            foreach (var item in pairs)
            {
                var track = item.Key.inBranch.track;

                // If the track is large enough, don't merge. Only the in track (not on branching side)
                // matters, as you always want a signal for the single exit.
                if (track.logicTrack.length > ClosenessThreshold) continue;

                // Junction at the other end of the track.
                var other = track.outJunction == item.Key ?
                    track.inJunction :
                    track.outJunction;

                // No junction on the other side of the track, ignore merging.
                // Also ignore if the junctions aren't facing opposite directions.
                if (other == null || other.inBranch.track != track) continue;

                // Already merged.
                if (merged.Contains(other)) continue;

                Merge(item.Value, _junctionSignals[other], pack);

                merged.Add(item.Key);
            }

            return merged.Count;
        }

        private static void Merge(JunctionSignalGroup p1, JunctionSignalGroup p2, SignalPack pack, TrackDirection direction = TrackDirection.Out)
        {
            if (direction.IsOut())
            {
                // Replace the signal type if needed.
                if (p1.InBranchSignal != null && p2.OutBranchesSignal != null && p1.InBranchSignal.Type != p2.OutBranchesSignal.Type)
                {
                    p1.InBranchSignal = Replace(p1.InBranchSignal, pack.GetForType(p2.OutBranchesSignal.Type));
                }

                if (p2.InBranchSignal != null && p1.OutBranchesSignal != null && p2.InBranchSignal.Type != p1.OutBranchesSignal.Type)
                {
                    p2.InBranchSignal = Replace(p2.InBranchSignal, pack.GetForType(p1.OutBranchesSignal.Type));
                }

                p1.OutBranchesSignal?.Destroy();
                p2.OutBranchesSignal?.Destroy();
                p1.OutBranchesSignal = null;
                p2.OutBranchesSignal = null;
            }
            else
            {
                if (p1.OutBranchesSignal != null && p2.InBranchSignal != null && p1.OutBranchesSignal.Type != p2.InBranchSignal.Type)
                {
                    p1.OutBranchesSignal = Replace(p1.OutBranchesSignal, pack.GetForType(p2.InBranchSignal.Type));
                }

                if (p2.OutBranchesSignal != null && p1.InBranchSignal != null && p2.OutBranchesSignal.Type != p1.InBranchSignal.Type)
                {
                    p2.OutBranchesSignal = Replace(p2.OutBranchesSignal, pack.GetForType(p1.InBranchSignal.Type));
                }

                p1.InBranchSignal?.Destroy();
                p2.InBranchSignal?.Destroy();
                p1.InBranchSignal = null;
                p2.InBranchSignal = null;
            }
        }

        private static JunctionSignalController? Replace(JunctionSignalController? original, SignalControllerDefinition? replacement)
        {
            if (original == null || replacement == null) return null;

            var instanced = Instantiate(replacement, original.Definition.transform.parent, false);

            var instT = instanced.transform;
            var origT = original.Definition.transform;

            instT.localPosition = origT.localPosition;
            instT.localRotation = origT.localRotation;

            Destroy(original.Definition.gameObject);

            return new JunctionSignalController(instanced, original.Junction, original.Direction)
            {
                Type = original.Type
            };
        }

        #endregion

        #endregion

        // Update all signals from here.
        private System.Collections.IEnumerator UpdateRoutine()
        {
            while (PlayerManager.ActiveCamera == null) yield return null;

            yield return new WaitForSeconds(UpdateTime);

            while (true)
            {
                // Check how many signals to update per frame so they all update roughly once a second.
                int count = Mathf.CeilToInt(_signalRegister.Count * Time.fixedDeltaTime);

                // Loop through all registered signals.
                for (int start = 0; start < _signalRegister.Count; start += count)
                {
                    // Loop through a batch of signals. Updates are distributed so they don't all update
                    // at once, but batched so timing is consistent.
                    for (int current = start; current < start + count && current < _signalRegister.Count; current++)
                    {
                        // Stop updating if camera is gone.
                        while (PlayerManager.ActiveCamera == null) yield return new WaitForSeconds(UpdateTime);

                        var signal = _signalRegister[current];

                        // Check if it can be updated.
                        if (!signal.SafetyCheck())
                        {
                            current--;
                            continue;
                        }

                        // Distance optimisation.
                        signal.Optimise();

                        // Skip updating if not needed.
                        if (signal.ManualOperationOnly || signal.ShouldSkipUpdate()) continue;

                        // Actually update.
                        signal.UpdateAspect();
                    }

                    yield return new WaitForFixedUpdate();
                }
            }
        }

        public void RegisterSignal(BasicSignalController signal)
        {
            _signalRegister.Add(signal);
        }

        public bool UnregisterSignal(BasicSignalController signal)
        {
            return _signalRegister.Remove(signal);
        }

        internal bool TryGetSignals(Junction junction, out JunctionSignalGroup pair)
        {
            return _junctionSignals.TryGetValue(junction, out pair);
        }

        /// <summary>
        /// Tries to find a signal at a junction.
        /// </summary>
        /// <param name="junction">The <see cref="Junction"/> where a signal may be.</param>
        /// <param name="direction">The direction of the signal. <see langword="true"/> if pointing towards the branches, <see langword="false"/> otherwise.</param>
        /// <param name="signalController">The signal, if found.</param>
        /// <returns><see langword="true"/> if a signal was found, <see langword="false"/> otherwise.</returns>
        public bool TryGetSignal(Junction junction, TrackDirection direction, out JunctionSignalController? signalController)
        {
            if (TryGetSignals(junction, out var pair))
            {
                signalController = pair.GetSignal(direction);

                if (signalController != null)
                {
                    return true;
                }
            }

            signalController = null;
            return false;
        }

        /// <summary>
        /// Tries to find a pack from the mod ID.
        /// </summary>
        /// <param name="id">The mod ID that added the pack.</param>
        /// <param name="pack">The returned pack if it exists.</param>
        /// <returns></returns>
        public static bool TryGetPack(string id, out SignalPack pack)
        {
            return InstalledPacks.TryGetValue(id, out pack);
        }
    }
}
