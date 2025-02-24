using DV.Logic.Job;
using DV.Utils;
using Signals.Common;
using Signals.Game.Controllers;
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

        public new static string AllowAutoCreate()
        {
            return "[SignalManager]";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _junctionMap.Clear();
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

        // Prevents triggering the Instance call too soon and creating the singleton.
        internal static void RegisterCreation() => Instance.CreateSignals();

        private void CreateSignals()
        {
            SignalsMod.Log("Started creating signals...");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            int created = 0;
            var pack = GetCurrentPack();

            foreach (var track in RailTrackRegistry.Instance.AllTracks)
            {
                switch (ShouldMakeSignal(track))
                {
                    case SignalCreationMode.Mainline:
                        var junction = track.inJunction;
                        _junctionMap.Add(junction, CreateJunctionSignals(pack.Signal, junction));
                        break;
                    default:
                        continue;
                }

                created++;
            }

            sw.Stop();
            SignalsMod.Log($"Finished creating {created} signal(s) ({sw.Elapsed.TotalSeconds:F4}s)");
            sw.Restart();

            // Wish this could be done within the same loop but alas.
            int mergeCount = MergeCloseSignals();

            sw.Stop();
            SignalsMod.Log($"Merged {mergeCount} signal(s) ({sw.Elapsed.TotalSeconds:F4}s)");

            TrackChecker.StartBuildingMap();
        }

        private SignalCreationMode ShouldMakeSignal(RailTrack track)
        {
            // Signals only at juntions, don't duplicate junctions, don't make them at short dead ends.
            if (!track.isJunctionTrack || _junctionMap.ContainsKey(track.inJunction) || InIsShortDeadEnd(track.inJunction))
            {
                return SignalCreationMode.None;
            }

            var inName = track.inJunction.inBranch.track.name;
            SignalsMod.LogVerbose($"Testing track '{inName}' for signals...");

            // If the in track belongs to a yard...
            if (inName.StartsWith(YardNameStart))
            {
                // Check all out branches.
                foreach (var branch in track.inJunction.outBranches)
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
                    if (!outName.StartsWith(YardNameStart) || PassengerTest(branch.track.outBranch.track))
                    {
                        return SignalCreationMode.Mainline;
                    }
                }

                // Switch branches stay in the same yard, no signal needed.
                return SignalCreationMode.Shunting;
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

        private static bool PassengerTest(RailTrack track)
        {
            return SignalsMod.Settings.CreateSignalsOnPax && track.logicTrack.ID.TrackPartOnly.EndsWith(TrackID.LOADING_PASSENGER_TYPE);
        }

        private static JunctionSignalPair CreateJunctionSignals(SignalControllerDefinition signal, Junction junction)
        {
            SignalsMod.LogVerbose($"Making junction signals for track '{junction.inBranch.track.name}'");

            var outTrack = junction.outBranches[0].track;

            // Use the anchor point as the placement root. Signal is offset to the side by a custom value specified
            // in the pack, and then backwards by a few metres to look better. Also keeps it out of the way from
            // switch stands this way. It'll block the stand from the branches side but that is fine. This does
            // however mean that if the track length is very short, it can look odd.
            var point = outTrack.curve.GetAnchorPoints()[0];
            var backward = -point.handle2.normalized;

            var to = Instantiate(signal, point.transform, false);
            var from = Instantiate(signal, point.transform, false);

            to.transform.localPosition = backward;
            from.transform.localPosition = -4.0f * backward;
            to.transform.localRotation = Quaternion.LookRotation(-point.handle2);
            from.transform.localRotation = Quaternion.LookRotation(point.handle2);

            return new JunctionSignalPair(new JunctionSignalController(to, junction, true), new JunctionSignalController(from, junction, false));
        }

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

        private static void Merge(JunctionSignalPair p1, JunctionSignalPair p2, bool direction = true)
        {
            if (direction)
            {
                Destroy(p1.OutBranchesSignal?.Definition.gameObject);
                Destroy(p2.OutBranchesSignal?.Definition.gameObject);
                p1.OutBranchesSignal = null;
                p2.OutBranchesSignal = null;
            }
            else
            {
                Destroy(p1.InBranchesSignal?.Definition.gameObject);
                Destroy(p2.InBranchesSignal?.Definition.gameObject);
                p1.InBranchesSignal = null;
                p2.InBranchesSignal = null;
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
        public bool TryGetSignal(Junction junction, bool direction, out JunctionSignalController? signalController)
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
    }
}
