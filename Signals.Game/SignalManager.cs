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
    internal class SignalManager : SingletonBehaviour<SignalManager>
    {
        private const string YardNameStart = "[Y]";

        private Dictionary<Junction, SignalPair> _junctionMap =
            new Dictionary<Junction, SignalPair>();

        public static SignalPack DefaultPack = null!;
        public static Dictionary<string, SignalPack> InstalledPacks = new Dictionary<string, SignalPack>();

        public new static string AllowAutoCreate()
        {
            return "[SignalManager]";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _junctionMap.Clear();
        }

        public void CreateSignals()
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

                var point = track.curve.GetAnchorPoints()[0];
                Vector3 offset = Vector3.Cross(Vector3.up, point.handle2).normalized * pack.OffsetFromTrackCentre;

                var to = Instantiate(pack.Signal, point.transform, false);
                var from = Instantiate(pack.Signal, point.transform, false);

                to.transform.localPosition = offset;
                from.transform.localPosition = -offset;
                to.transform.localRotation = Quaternion.LookRotation(point.handle2);
                from.transform.localRotation = Quaternion.LookRotation(-point.handle2);

                _junctionMap.Add(junction,
                    new SignalPair(new SignalController(to, junction, true), new SignalController(from, junction, false)));

                created++;
            }

            sw.Stop();
            SignalsMod.Log($"Finished creating {created} signal(s) ({sw.Elapsed.TotalSeconds:F4}s)");
        }

        private bool ShouldMakeSignal(RailTrack track)
        {
            // Signals only at juntions, and don't duplicate junctions.
            if (!track.isJunctionTrack || _junctionMap.ContainsKey(track.inJunction))
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

        private static int GetJunctionId(RailTrack track)
        {
            return track.inJunction.junctionData.junctionId;
        }

        private static bool PassengerTest(RailTrack track)
        {
            return SignalsMod.Settings.CreateSignalsOnPax && track.logicTrack.ID.TrackPartOnly.EndsWith(TrackID.LOADING_PASSENGER_TYPE);
        }

        public bool TryGetSignals(Junction junction, out SignalPair pair)
        {
            return _junctionMap.TryGetValue(junction, out pair);
        }

        public SignalPack GetCurrentPack()
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
                    if (DefaultPack == null)
                    {
                        DefaultPack = pack;
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

        internal static void RegisterCreation() => Instance.CreateSignals();
    }
}