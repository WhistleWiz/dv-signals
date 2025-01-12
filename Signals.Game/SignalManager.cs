﻿using DV.Logic.Job;
using DV.Utils;
using Signals.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace Signals.Game
{
    public class SignalManager : SingletonBehaviour<SignalManager>
    {
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

        private Dictionary<Junction, SignalPair> _junctionMap =
            new Dictionary<Junction, SignalPair>();

        public new static string AllowAutoCreate()
        {
            return "[SignalManager]";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _junctionMap.Clear();
        }

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
                if (!ShouldMakeSignal(track)) continue;

                SignalsMod.LogVerbose($"Making signals for track '{track.inJunction.inBranch.track.name}'");

                var junction = track.inJunction;
                var inTrack = junction.inBranch.track;

                // Use the anchor point as the placement root. Signal is offset to the side by a custom value specified
                // in the pack, and then backwards by a few metres to look better. Also keeps it out of the way from
                // switch stands this way. It'll block the stand from the branches side but that is fine. This does
                // however mean that if the track length is very short, it can look odd.
                var point = track.curve.GetAnchorPoints()[0];
                var backward = -point.handle2.normalized;
                Vector3 offset = Vector3.Cross(Vector3.up, point.handle2).normalized * pack.OffsetFromTrackCentre;

                var to = Instantiate(pack.Signal, point.transform, false);
                var from = Instantiate(pack.Signal, point.transform, false);

                to.transform.localPosition = offset + backward;
                from.transform.localPosition = -offset - 4.0f * backward;
                to.transform.localRotation = Quaternion.LookRotation(-point.handle2);
                from.transform.localRotation = Quaternion.LookRotation(point.handle2);

                _junctionMap.Add(junction,
                    new SignalPair(new SignalController(to, junction, true), new SignalController(from, junction, false)));

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

        private bool ShouldMakeSignal(RailTrack track)
        {
            // Signals only at juntions, don't duplicate junctions, don't make them at short dead ends.
            if (!track.isJunctionTrack || _junctionMap.ContainsKey(track.inJunction) || InIsShortDeadEnd(track.inJunction))
            {
                return false;
            }

            var inName = track.inJunction.inBranch.track.name;
            SignalsMod.LogVerbose($"Testing track '{inName}' for signals...");

            // If the in track belongs to a yard...
            if (inName.StartsWith(YardNameStart))
            {
                if (PassengerTest(track.inJunction.inBranch.track))
                {
                    return true;
                }

                // Check all out branches.
                foreach (var branch in track.inJunction.outBranches)
                {
                    // Track ends after a switch for some reason, plop signal.
                    if (!branch.track.outIsConnected)
                    {
                        return true;
                    }

                    // Get the track after the switch track.
                    var outName = branch.track.outBranch.track.name;

                    SignalsMod.LogVerbose($"Testing branch '{outName}' for signals...");

                    // If this yard track goes to a non yard track...
                    if (!outName.StartsWith(YardNameStart) || PassengerTest(branch.track.outBranch.track))
                    {
                        return true;
                    }
                }

                // Switch branches stay in the same yard, no signal needed.
                return false;
            }

            // No conditions met, plop signal.
            return true;
        }

        private bool InIsShortDeadEnd(Junction junction)
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

        private static int GetJunctionId(RailTrack track)
        {
            return track.inJunction.junctionData.junctionId;
        }

        private static bool PassengerTest(RailTrack track)
        {
            return SignalsMod.Settings.CreateSignalsOnPax && track.logicTrack.ID.TrackPartOnly.EndsWith(TrackID.LOADING_PASSENGER_TYPE);
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

                var result = Merge(item.Value, _junctionMap[other]);
                _junctionMap[item.Key] = result.Flip();
                _junctionMap[other] = result;

                merged.Add(item.Key);
            }

            return merged.Count;
        }

        private static SignalPair Merge(SignalPair p1, SignalPair p2, bool direction = true)
        {
            if (direction)
            {
                Destroy(p1.To.Definition.gameObject);
                Destroy(p2.To.Definition.gameObject);
                return new SignalPair(p1.From, p2.From);
            }
            else
            {
                Destroy(p1.From.Definition.gameObject);
                Destroy(p2.From.Definition.gameObject);
                return new SignalPair(p1.To, p2.To);
            }
        }

        #endregion

        internal bool TryGetSignals(Junction junction, out SignalPair pair)
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
        public bool TryGetSignal(Junction junction, bool direction, out SignalController signalController)
        {
            if (TryGetSignals(junction, out var pair))
            {
                signalController = pair.GetSignal(direction);
                return true;
            }

            signalController = null!;
            return false;
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
    }
}
