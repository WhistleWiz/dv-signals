using DV.Localization;
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
            Shunting
        }

        private const string YardNameStart = "[Y]";
        private const int LaserPointerTargetLayer = 15;
        private const float DeadEndThreshold = 100.0f;
        private const float ClosenessThreshold = 25.0f;

        private static Transform? _holder;

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

        private Dictionary<Junction, JunctionSignalPair> _junctionMap =
            new Dictionary<Junction, JunctionSignalPair>();
        private List<DistantSignalController> _distantSignals =
            new List<DistantSignalController>();

        public new static string AllowAutoCreate()
        {
            return "[SignalManager]";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _junctionMap.Clear();
            _distantSignals.Clear();
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
            if (msg != LocalizationAPI.L(WorldStreamingInit.INFO_GAME_CONTENT)) return;

            DisplayLoadingThingy();
            Instance.CreateSignals();
        }

        private static void DisplayLoadingThingy()
        {
            // Thanks Skin Manager.
            var info = FindObjectOfType<DisplayLoadingInfo>();

            if (info != null)
            {
                info.loadProgressTMP.richText = true;
                info.loadProgressTMP.text += "\ncreating <color=#FF2200>si</color><color=#FFDD22>gna</color><color=#22FF44>ls</color>";
            }
        }

        private void CreateSignals()
        {
            SignalsMod.Log("Started creating signals...");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            int created = 0;
            var pack = GetCurrentPack();

            foreach (var track in RailTrackRegistry.Instance.AllTracks)
            {
                JunctionSignalPair signals;
                var junction = track.inJunction;

                switch (ShouldMakeSignal(track))
                {
                    case SignalCreationMode.Mainline:
                        signals = CreateMainlineSignals(pack, junction);
                        break;
                    case SignalCreationMode.IntoYard:
                        signals = CreateIntoYardSignals(pack, junction);
                        break;
                    default:
                        continue;
                }

                _junctionMap.Add(junction, signals);
                created++;
            }

            sw.Stop();
            SignalsMod.Log($"Finished creating signals for {created} junction(s) ({sw.Elapsed.TotalSeconds:F4}s)");
            sw.Restart();

            // Wish this could be done within the same loop but alas.
            int mergeCount = MergeCloseSignals();

            sw.Stop();
            SignalsMod.Log($"Merged {mergeCount} signal(s) ({sw.Elapsed.TotalSeconds:F4}s)");
            sw.Restart();

            created = CreateDistantSignals(pack);

            sw.Stop();
            SignalsMod.Log($"Finished creating {created} distant signal(s) ({sw.Elapsed.TotalSeconds:F4}s)");

            TrackChecker.StartBuildingMap();
        }

        private int CreateDistantSignals(SignalPack pack)
        {
            if (pack.DistantSignal == null) return 0;

            int count = 0;

            foreach (var junction in _junctionMap)
            {
                DistantSignalController? distant;

                // Create a distant signal for the in branch.
                var signal = junction.Value.OutBranchesSignal;

                if (signal != null && (signal.Type == SignalType.Mainline || signal.Type == SignalType.IntoYard))
                {
                    distant = CreateDistantSignalIn(signal, pack.DistantSignal, pack.DistantSignalDistance, pack.DistantSignalMinimumTrackLength);

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
                        distant = CreateDistantSignalOut(signal, pack.DistantSignal, i, pack.DistantSignalDistance, pack.DistantSignalMinimumTrackLength);

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

        // Creation testing.

        private SignalCreationMode ShouldMakeSignal(RailTrack track)
        {
            // Signals only at juntions, don't duplicate junctions, don't make them at short dead ends.
            if (!track.isJunctionTrack || _junctionMap.ContainsKey(track.inJunction) || InIsShortDeadEnd(track.inJunction))
            {
                return SignalCreationMode.None;
            }

            var junction = track.inJunction;
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
                        return SignalCreationMode.IntoYard;
                    }
                }

                // Switch branches stay in the same yard, no signal needed.
                return SignalCreationMode.Shunting;
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

        // Main creation methods.

        private static JunctionSignalPair CreateMainlineSignals(SignalPack pack, Junction junction)
        {
            var signals = CreateJunctionSignals(pack.Signal, junction);

            foreach (var signal in signals.AllSignals)
            {
                signal.Type = SignalType.Mainline;
            }

            return signals;
        }

        private static JunctionSignalPair CreateIntoYardSignals(SignalPack pack, Junction junction)
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

        private static DistantSignalController? CreateDistantSignalIn(JunctionSignalController junctionSignal, SignalControllerDefinition def,
            float distance, float minLength)
        {
            var track = junctionSignal.Junction.inBranch.track;

            if (track.logicTrack.length < minLength) return null;

            SignalsMod.LogVerbose($"Making distant signal [in] for signal '{junctionSignal.Name}'");

            bool dir = track.inJunction == junctionSignal.Junction;

            var (point, forward) = dir ?
                BezierHelper.GetAproxPointAtLength(track.curve, distance) :
                BezierHelper.GetAproxPointAtLengthReverse(track.curve, distance);

            var signal = Instantiate(def, track.curve.transform, false);

            signal.transform.position = point;
            signal.transform.localRotation = Quaternion.LookRotation(dir ? forward : -forward);

            return new DistantSignalController(junctionSignal, signal);
        }

        private static DistantSignalController? CreateDistantSignalOut(JunctionSignalController junctionSignal, SignalControllerDefinition def,
            int branch, float distance, float minLength)
        {
            var branchTrack = junctionSignal.Junction.outBranches[branch].track;
            var track = branchTrack.outBranch.track;

            if (track.logicTrack.length < minLength) return null;

            SignalsMod.LogVerbose($"Making distant signal [b:{branch}] for signal '{junctionSignal.Name}'");

            bool dir = track.inBranch.track == branchTrack;

            var (point, forward) = dir ?
                BezierHelper.GetAproxPointAtLength(track.curve, distance) :
                BezierHelper.GetAproxPointAtLengthReverse(track.curve, distance);

            var signal = Instantiate(def, track.curve.transform, false);

            signal.transform.position = point;
            signal.transform.localRotation = Quaternion.LookRotation(dir ? forward : -forward);

            return new DistantSignalController(junctionSignal, signal);
        }

        // Internal creation methods.

        private static JunctionSignalPair CreateJunctionSignals(SignalControllerDefinition signal, Junction junction)
        {
            return CreateDoubleJunctionSignals(signal, signal, junction);
        }

        private static JunctionSignalPair CreateJunctionSignals(SignalControllerDefinition? outSignal, SignalControllerDefinition inSignal, Junction junction)
        {
            if (outSignal != null)
            {
                return CreateDoubleJunctionSignals(outSignal, inSignal, junction);
            }

            return CreateSingleJunctionSignal(inSignal, junction);
        }

        private static JunctionSignalPair CreateDoubleJunctionSignals(SignalControllerDefinition outSignal, SignalControllerDefinition inSignal, Junction junction)
        {
            SignalsMod.LogVerbose($"Making junction signals for track '{junction.inBranch.track.name}'");

            var outTrack = junction.outBranches[0].track;

            // Use the anchor point as the placement root. Signal is offset to the side by a custom value specified
            // in the pack, and then backwards by a few metres to look better. Also keeps it out of the way from
            // switch stands this way. It'll block the stand from the branches side but that is fine. This does
            // however mean that if the track length is very short, it can look odd.
            var point = outTrack.curve.GetAnchorPoints()[0];
            var backward = -point.handle2.normalized;

            var to = Instantiate(outSignal, point.transform, false);
            var from = Instantiate(inSignal, point.transform, false);

            to.transform.localPosition = backward;
            from.transform.localPosition = -4.0f * backward;
            to.transform.localRotation = Quaternion.LookRotation(-point.handle2);
            from.transform.localRotation = Quaternion.LookRotation(point.handle2);

            return new JunctionSignalPair(
                new JunctionSignalController(to, junction, TrackDirection.Out),
                new JunctionSignalController(from, junction, TrackDirection.In));
        }

        private static JunctionSignalPair CreateSingleJunctionSignal(SignalControllerDefinition inSignal, Junction junction)
        {
            SignalsMod.LogVerbose($"Making junction signals for track '{junction.inBranch.track.name}'");

            var outTrack = junction.outBranches[0].track;

            // Similar to the previous method, but only creates a single signal (the one facing the in branch, so "out" of the yard).
            var point = outTrack.curve.GetAnchorPoints()[0];
            var backward = -point.handle2.normalized;

            var from = Instantiate(inSignal, point.transform, false);

            from.transform.localPosition = -4.0f * backward;
            from.transform.localRotation = Quaternion.LookRotation(point.handle2);

            return new JunctionSignalPair(null, new JunctionSignalController(from, junction, TrackDirection.In));
        }

        // Post processing.

        private int MergeCloseSignals()
        {
            HashSet<Junction> merged = new HashSet<Junction>();
            var pairs = _junctionMap.ToList();

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

                Merge(item.Value, _junctionMap[other]);

                merged.Add(item.Key);
            }

            return merged.Count;
        }

        private static void Merge(JunctionSignalPair p1, JunctionSignalPair p2, TrackDirection direction = TrackDirection.Out)
        {
            if (direction.IsOut())
            {
                Destroy(p1.OutBranchesSignal?.Definition.gameObject);
                Destroy(p2.OutBranchesSignal?.Definition.gameObject);
                p1.OutBranchesSignal = null;
                p2.OutBranchesSignal = null;
            }
            else
            {
                Destroy(p1.InBranchSignal?.Definition.gameObject);
                Destroy(p2.InBranchSignal?.Definition.gameObject);
                p1.InBranchSignal = null;
                p2.InBranchSignal = null;
            }
        }

        #endregion

        internal bool TryGetSignals(Junction junction, out JunctionSignalPair pair)
        {
            return _junctionMap.TryGetValue(junction, out pair);
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
