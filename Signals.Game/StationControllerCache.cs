using Signals.Game.Controllers;
using Signals.Game.Railway;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game
{
    public class StationControllerCache
    {
        public class StationCache
        {
            public class SignalsToEntryTracksCollection
            {
                public HashSet<RailTrack> EntryTracks;
                public List<BasicSignalController> ExitSignals;

                public SignalsToEntryTracksCollection(HashSet<RailTrack> tracks)
                {
                    EntryTracks = tracks;
                    ExitSignals = new List<BasicSignalController>();
                }

                public bool MatchesEntryTracks(HashSet<RailTrack> tracks)
                {
                    return EntryTracks.SetEquals(tracks);
                }
            }

            private bool _sorted = false;

            public string Station;
            public List<BasicSignalController> EntrySignals;
            public List<BasicSignalController> ExitSignals;
            public List<BasicSignalController> MiscSignals;
            public List<BasicSignalController> ExitNoGroupSignals;
            public List<SignalsToEntryTracksCollection> ExitGroups;

            public StationCache(string station)
            {
                Station = station;
                EntrySignals = new List<BasicSignalController>();
                ExitSignals = new List<BasicSignalController>();
                MiscSignals = new List<BasicSignalController>();
                ExitNoGroupSignals = new List<BasicSignalController>();
                ExitGroups = new List<SignalsToEntryTracksCollection>();
            }

            public void Sort()
            {
                if (_sorted) return;

                _sorted = true;

                var entryTracks = EntrySignals.Where(x => x.PlacementInfo.HasValue).Select(x => x.PlacementInfo!.Value.Track);

                foreach (var signal in ExitSignals)
                {
                    if (!signal.PlacementInfo.HasValue)
                    {
                        ExitNoGroupSignals.Add(signal);
                        continue;
                    }

                    var info = signal.PlacementInfo.Value;
                    var results = TrackWalker.GetReachableTracks(info.Track, info.Direction.Flipped(), entryTracks);

                    if (results.Count == 0)
                    {
                        ExitNoGroupSignals.Add(signal);
                        continue;
                    }

                    var match = ExitGroups.FirstOrDefault(x => x.MatchesEntryTracks(results));

                    if (match == null)
                    {
                        match = new SignalsToEntryTracksCollection(results);
                        ExitGroups.Add(match);
                    }

                    match.ExitSignals.Add(signal);
                }

                ExitGroups = ExitGroups.OrderBy(x => x.EntryTracks.Count).ToList();

                foreach (var group in ExitGroups)
                {
                    // Small trick to avoid using a controller that is slightly turned.
                    var t = group.ExitSignals.Count < 3 ?
                        group.ExitSignals[0].Definition.transform :
                        group.ExitSignals[(group.ExitSignals.Count - 1) / 2].Definition.transform;
                    group.ExitSignals = group.ExitSignals.OrderBy(x => t.InverseTransformPoint(x.Definition.transform.position).x).ToList();
                }
            }

            public SignalsToEntryTracksCollection? GetExitGroupFor(BasicSignalController controller, out int groupIndex)
            {
                var group = ExitGroups.FirstOrDefault(x => x.ExitSignals.Contains(controller));

                if (group == null)
                {
                    groupIndex = -1;
                }
                else
                {
                    groupIndex = ExitGroups.IndexOf(group);
                }

                return group;
            }
        }

        private static Dictionary<string, StationCache> s_stations =
            new Dictionary<string, StationCache>();
        private static Dictionary<string, List<BasicSignalController>> s_trackToControllers =
            new Dictionary<string, List<BasicSignalController>>();
        private static Dictionary<BasicSignalController, StationCache> s_controllerToStation =
            new Dictionary<BasicSignalController, StationCache>();

        public static void ClearCache()
        {
            s_stations.Clear();
            s_trackToControllers.Clear();
            s_controllerToStation.Clear();
        }

        public static void Generate(List<BasicSignalController> registry)
        {
            foreach (var item in registry)
            {
                if (item.Type == SignalType.Shunting) continue;

                if (item.Group == null)
                {
                    TryAssignToTrack(item);
                    continue;
                }

                var station = item.Group.Station;

                if (string.IsNullOrEmpty(station))
                {
                    // Because the entry signals can be merged and moved out of the station tracks,
                    // so the station must be detected in some way...
                    if (item.Type.IsEntry() && item.PlacementInfo.HasValue)
                    {
                        SignalsMod.LogVerbose($"Entry signal ({item.Id}) with no station check started...");

                        var info = item.PlacementInfo.Value;
                        var junction = TrackWalker.GetNextJunctionDivergingOnly(info.Track, info.Direction.Flipped());

                        if (junction == null) goto AssignToTrackEnd;

                        station = junction.GetStation();

                        SignalsMod.LogVerbose($"    First station: {station}");

                        if (string.IsNullOrEmpty(station))
                        {
                            foreach (var branch in junction.outBranches)
                            {
                                var junction2 = TrackWalker.GetNextJunctionDivergingOnly(branch.track, TrackDirection.Out);

                                if (junction2 == null) continue;

                                station = junction2.GetStation();

                                SignalsMod.LogVerbose($"    Second station: {station}");

                                if (!string.IsNullOrEmpty(station)) break;
                            }

                            if (string.IsNullOrEmpty(station)) goto AssignToTrackEnd;
                        }
                    }
                    else
                    {
                        goto AssignToTrackEnd;
                    }
                }

                var signals = CheckStation(station);
                var flag = false;

                if (item.Type.IsEntry())
                {
                    signals.EntrySignals.Add(item);
                    flag = true;
                }
                else if (item.Type.IsAnyExit())
                {
                    signals.ExitSignals.Add(item);
                    flag = true;
                }
                else
                {
                    if (!TryAssignToTrack(item))
                    {
                        signals.MiscSignals.Add(item);
                        flag = true;
                    }
                }

                if (flag)
                {
                    s_controllerToStation.Add(item, signals);
                }

                continue;

            AssignToTrackEnd:
                TryAssignToTrack(item);
                continue;
            }

            static bool TryAssignToTrack(BasicSignalController controller)
            {
                return controller.PlacementInfo.HasValue && AssignToTrack(controller.PlacementInfo.Value.Track, controller);
            }

            static StationCache CheckStation(string station)
            {
                if (!s_stations.TryGetValue(station, out var signals))
                {
                    signals = new StationCache(station);
                    s_stations.Add(station, signals);
                }

                return signals;
            }
        }

        private static bool AssignToTrack(RailTrack track, BasicSignalController controller)
        {
            if (track.IsPartOfYard()) return false;

            if (s_trackToControllers.TryGetValue(track.name, out var list))
            {
                list.Add(controller);
            }
            else
            {
                list = new List<BasicSignalController> { controller };
                s_trackToControllers.Add(track.name, list);
            }

            return true;
        }

        public static bool TryGetStationInfo(BasicSignalController controller,
            out StationCache station)
        {
            return s_controllerToStation.TryGetValue(controller, out station);
        }

        public static int GetIndex(BasicSignalController controller)
        {
            if (!controller.PlacementInfo.HasValue) return -1;

            if (s_trackToControllers.TryGetValue(controller.PlacementInfo.Value.Track.name, out var list))
            {
                return list.IndexOf(controller);
            }

            return -1;
        }
    }
}
