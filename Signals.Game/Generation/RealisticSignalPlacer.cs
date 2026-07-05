using Signals.Common;
using Signals.Game.Controllers;
using Signals.Game.Railway;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game.Generation
{
    public sealed class RealisticSignalPlacer : SignalPlacer
    {
        private struct PlacementHelper
        {
            public SignalControllerDefinition? Definition;
            public PrefabType PrefabType;
            public SignalType SignalType;
            public bool Old;

            public PlacementHelper(SignalControllerDefinition? definition, PrefabType prefabType, bool old)
            {
                Definition = definition;
                PrefabType = prefabType;
                SignalType = GetSignalType(prefabType);
                Old = old;
            }

            public readonly void Apply(BasicSignalController? controller)
            {
                if (controller == null) return;

                controller.IsOld = Old;
                controller.PrefabType = PrefabType;
                controller.Type = SignalType;
            }
        }

        private static HashSet<RailTrack> s_stationTracks = new HashSet<RailTrack>();

        public override void CreateSignals(SignalPack pack, Dictionary<Junction, JunctionSignalGroup> registry)
        {
            s_stationTracks.Clear();

            foreach (var junction in RailTrackRegistry.Junctions)
            {
                if (registry.ContainsKey(junction)) continue;

                var group = CreateGroup(pack, junction);

                if (group != null)
                {
                    registry.Add(junction, group);
                }
            }
        }

        private JunctionSignalGroup? CreateGroup(SignalPack pack, Junction junction)
        {
            var branchDistance = BranchSignalDistance(junction, BranchPlacementDistance);
            var branchTrackKey = new Dictionary<RailTrack, PlacementHelper>();
            var inTrack = junction.inBranch.track;
            var anyYard = false;
            var old = IsOld(junction);
            var deadEnd = false;
            var deadCheck = false;
            var stationEnd = false;
            var stationCheck = false;

            if (junction.IsFromDoubleTrackModStation())
            {
                foreach (var branch in junction.outBranches)
                {
                    var track = branch.track.outBranch.track;
                    branchTrackKey.Add(track, ShuntingOrSpacing(track, false));
                }

                var jplc = ShuntingOrSpacing(inTrack, IsLogicYardTrack(inTrack));

                if (jplc.Definition == null) return WithoutJunction();

                var ctrl = CreateSignalAtJunction(junction, jplc.Definition, JunctionPlacementDistance);
                jplc.Apply(ctrl);
                return new JunctionSignalGroup(junction, ctrl, CreateBranchSignals(junction, branchTrackKey, branchDistance));
            }

            if (junction.IsFromDoubleTrackMod())
            {
                foreach (var branch in junction.outBranches)
                {
                    branchTrackKey.Add(branch.track.outBranch.track, GetPlacement(branch.IsThroughTrack() ? PrefabType.Mainline : PrefabType.Diverging));
                }

                return new JunctionSignalGroup(junction, null, CreateBranchSignalsOppositeDouble(junction, branchTrackKey, branchDistance));
            }

            // Check branches.
            foreach (var branch in junction.outBranches)
            {
                var track = branch.track.outBranch.track;
                deadCheck = false;
                stationCheck = false;

                // If it's any of the logic yard tracks, it needs a signal for exit.
                if (IsPaxTrack(track) && !GoesToDeadEndIn() && LeavesStationIn())
                {
                    branchTrackKey.Add(track, GetPlacement(PrefabType.ExitPax));
                    anyYard = true;
                }
                else if (IsExitTrack(track) && !GoesToDeadEndIn() && LeavesStationIn())
                {
                    branchTrackKey.Add(track, GetPlacement(PrefabType.Exit));
                    anyYard = true;
                }
                else if (IsLogicYardTrack(track))
                {
                    if (IsLoadingTrack(track) && !inTrack.IsPartOfStation())
                    {
                        branchTrackKey.Add(track, ShuntingOrSpacing(track, true));
                    }
                    else if (!GoesToDeadEndIn())
                    {
                        branchTrackKey.Add(track, GetPlacement(PrefabType.ShuntingMajor));
                    }
                    else
                    {
                        branchTrackKey.Add(track, GetPlacement(PrefabType.Shunting));
                    }

                    anyYard = true;
                }
                // If it's not part of a station, then it requires a normal signal.
                else if (!track.IsPartOfStation())
                {
                    // If the branch is a short dead end or comes from a station track and is short,
                    // use a shunting signal.
                    if (IsShortDeadEnd(track) || ComesFromStationAndIsShort(track))
                    {
                        branchTrackKey.Add(track, GetPlacement(PrefabType.Shunting));
                        anyYard = true;
                    }
                    else if (!IsMainlineCountedAsYard(track) && (IsMainlineCountedAsYard(inTrack) || inTrack.IsPartOfStation()))
                    {
                        branchTrackKey.Add(track, GetPlacement(PrefabType.Entry));
                    }
                    else if (IsMainlineCountedAsYard(track))
                    {
                        anyYard = true;
                        branchTrackKey.Add(track, ShuntingOrSpacing(track, false));
                    }
                    else
                    {
                        // Only place the mainline signal if the in track isn't a short dead end either.
                        if (SignalsMod.Settings.PlaceSignalsInBranches && !IsShortDeadEnd(inTrack))
                        {
                            branchTrackKey.Add(track, GetPlacement(branch.IsThroughTrack() ? PrefabType.Mainline : PrefabType.Diverging));
                        }
                    }
                }
                else
                {
                    anyYard = true;

                    // Generic yard tracks required a certain length to have a signal.
                    if (track.GetLength() <= SmallTrackThreshold) continue;

                    // If this track can be considered a mainline through the yard, then give it proper signals.
                    if (IsYardTrackMainline(branch) && !GoesToDeadEndIn() && LeavesStationIn())
                    {
                        branchTrackKey.Add(track, GetPlacement(PrefabType.ExitMainline));
                    }
                    else
                    {
                        // If it goes to a dead end just use a shunting signal as usual, else do the spacing thingy.
                        branchTrackKey.Add(track, GoesToDeadEndIn() ? GetPlacement(PrefabType.Shunting) : ShuntingOrSpacing(track, false));
                    }
                }

                // Quick function to prevent running this loop multiple times.
                bool GoesToDeadEndIn()
                {
                    if (!deadCheck)
                    {
                        deadEnd = TrackWalker.GoesToDeadEnd(branch.track, TrackDirection.In);
                        deadCheck = true;
                    }

                    return deadEnd;
                }

                // Same for station.
                bool LeavesStationIn()
                {
                    if (!stationCheck)
                    {
                        stationEnd = !TrackWalker.RemainsInTheSameStation(branch.track, TrackDirection.In, junction.GetStation());
                        stationCheck = true;
                    }

                    return stationEnd;
                }
            }

            deadCheck = false;
            stationCheck = false;

            // If the in track is a generic yard track it doesn't get a signal, unless it's big.
            if (inTrack.IsPartOfYard() && !IsLogicYardTrack(inTrack) && inTrack.GetLength() < SmallTrackThreshold)
            {
                // If there's also no branch signals, then don't create a group.
                if (branchTrackKey.Count == 0) return null;

                // Create signals at the branches from the keys.
                return WithoutJunction();
            }

            PlacementHelper junctionSignal;
            var isMain = false;

            // For the junction side, do the same checks for inputs and outputs.
            if (IsPaxTrack(inTrack) && !GoesToDeadEndOut() && LeavesStationOut())
            {
                junctionSignal = GetPlacement(PrefabType.ExitPax);
            }
            else if (IsExitTrack(inTrack) && !GoesToDeadEndOut() && LeavesStationOut())
            {
                junctionSignal = GetPlacement(PrefabType.Exit);
            }
            else if (IsLogicYardTrack(inTrack) || inTrack.IsNonSign() || IsShortDeadEnd(inTrack))
            {
                // Track is too small for a signal.
                if (inTrack.GetLength() < VerySmallTrackThreshold) return WithoutJunction();

                // Special handling for loading tracks as they may act as mainlines (IME, CME...).
                if (IsLoadingTrack(inTrack) || inTrack.IsNonSign())
                {
                    junctionSignal = ShuntingOrSpacing(inTrack, IsLogicYardTrack(inTrack));
                }
                else
                {
                    junctionSignal = GetPlacement(IsLogicYardTrack(inTrack) ? PrefabType.ShuntingMajor : PrefabType.Shunting);
                }
            }
            // If the track goes into any yard track...
            else if (anyYard)
            {
                // If the track is marked as mainline but is actually short and within station tracks,
                // then fall back to either shunting or spacing signals.
                // Same with yard tracks.
                if(IsMainlineCountedAsYard(inTrack) || inTrack.IsPartOfStation())
                {
                    junctionSignal = ShuntingOrSpacing(inTrack, false);
                }
                else
                {
                    // Otherwise it's an entrance into a station, so entry signal.
                    junctionSignal = GetPlacement(PrefabType.Entry);
                    isMain = true;
                }
            }
            else
            {
                if (!SignalsMod.Settings.PlaceSignalsOutsideStations) return WithoutJunction();

                junctionSignal = junction.IsLeft() ? GetPlacement(PrefabType.JunctionLeft) : GetPlacement(PrefabType.JunctionRight);
                isMain = true;
            }

            // Upgrade shunting signal.
            // Shunting major gets upgraded if it doesn't have a prefab.
            if (junctionSignal.PrefabType == PrefabType.Shunting ||
                (junctionSignal.PrefabType == PrefabType.ShuntingMajor && junctionSignal.Definition == null))
            {
                junctionSignal = GetPlacement(PrefabType.ShuntingJunction);
            }

            if (junctionSignal.Definition == null) return WithoutJunction();

            var junctionController = CreateSignalAtJunction(junction, junctionSignal.Definition,
                isMain ? LongJunctionPlacementDistance : JunctionPlacementDistance);
            junctionSignal.Apply(junctionController);

            return new JunctionSignalGroup(junction, junctionController,
                CreateBranchSignals(junction, branchTrackKey, branchDistance));

            PlacementHelper GetPlacement(PrefabType prefabType)
            {
                return new PlacementHelper(GetForType(pack, prefabType, old), prefabType, old);
            }

            PlacementHelper ShuntingOrSpacing(RailTrack track, bool logicTrack)
            {
                var spacing = pack.GetSpacingSignal(old);

                if (spacing != null && track.GetLength() > SpacingThreshold)
                {
                    return new PlacementHelper(spacing, PrefabType.Spacing, old);
                }
                else if (logicTrack)
                {
                    return GetPlacement(PrefabType.ShuntingMajor);
                }
                else
                {
                    return GetPlacement(PrefabType.Shunting);
                }
            }

            bool GoesToDeadEndOut()
            {
                if (!deadCheck)
                {
                    deadEnd = junction.outBranches.Any(x => TrackWalker.GoesToDeadEnd(x.track, TrackDirection.Out));
                    deadCheck = true;
                }

                return deadEnd;
            }

            bool LeavesStationOut()
            {
                if (!stationCheck)
                {
                    var station = junction.GetStation();
                    stationEnd = junction.outBranches.Any(x => !TrackWalker.RemainsInTheSameStation(x.track, TrackDirection.Out, station));
                    stationCheck = true;
                }

                return stationEnd;
            }

            JunctionSignalGroup WithoutJunction()
            {
                return new JunctionSignalGroup(junction, null, CreateBranchSignals(junction, branchTrackKey, branchDistance));
            }
        }

        private static SignalType GetSignalType(PrefabType prefab) => prefab switch
        {
            PrefabType.Mainline => SignalType.Mainline,
            PrefabType.Diverging => SignalType.Mainline,
            PrefabType.JunctionLeft => SignalType.Mainline,
            PrefabType.JunctionRight => SignalType.Mainline,
            PrefabType.Entry => SignalType.Entry,
            PrefabType.Exit => SignalType.Exit,
            PrefabType.ExitPax => SignalType.ExitPax,
            PrefabType.Shunting => SignalType.Shunting,
            PrefabType.ShuntingJunction => SignalType.Shunting,
            PrefabType.ExitMainline => SignalType.ExitMainline,
            PrefabType.Spacing => SignalType.Spacing,
            _ => SignalType.NotSet,
        };

        private static bool ComesFromStationAndIsShort(RailTrack track)
        {
            if (track.GetLength() > SmallTrackThreshold) return false;

            var branch = track.inJunction == null ? track.GetInBranch() : track.GetOutBranch();

            if (branch == null || !branch.track.isJunctionTrack) return false;

            branch = branch.track.GetInBranch();

            return branch != null && branch.track.IsPartOfStation();
        }

        private static bool IsYardTrackMainline(Junction.Branch branch)
        {
            if (!branch.track.outBranch.track.IsPartOfYard()) return false;

            if (!branch.IsThroughTrack()) return false;

            var tracks = TrackWalker.WalkTracks(branch.track, TrackDirection.In, 3);

            if (tracks.Count < 3 || !tracks[0].Track.IsPartOfStation()) return false;

            return !tracks[2].Track.IsPartOfYard();
        }

        private static bool IsMainlineCountedAsYard(RailTrack track)
        {
            if (s_stationTracks.Contains(track)) return true;

            var j1 = TrackWalker.GetNextJunction(track, TrackDirection.In);
            var j2 = TrackWalker.GetNextJunction(track, TrackDirection.Out);

            if (j1 != null)
            {
                var station = j1.GetStation();

                if (!string.IsNullOrEmpty(station) && (j2 == null || station == j2.GetStation()))
                {
                    s_stationTracks.Add(track);
                    return true;
                }

                return false;
            }

            if (j2 != null && !string.IsNullOrEmpty(j2.GetStation()))
            {
                s_stationTracks.Add(track);
                return true;
            }

            return false;

            //var j1 = track.inJunction ?? (track.GetInBranch()?.track?.inJunction);
            //var j2 = track.outJunction ?? (track.GetOutBranch()?.track?.inJunction);

            //if (j1 != null && j2 != null)
            //{
            //    var station = j1.GetStation();

            //    if (!string.IsNullOrEmpty(station) && station == j2.GetStation())
            //    {
            //        s_stationTracks.Add(track);
            //        return true;
            //    }
            //}

            //if (track.GetLength() > SmallTrackThreshold) return false;

            //var tracks = TrackWalker.WalkTracks(track, TrackDirection.In, 2);

            //if (tracks.Count < 2 || !tracks[1].Track.IsPartOfStation()) return false;

            //tracks = TrackWalker.WalkTracks(track, TrackDirection.Out, 2);

            //if (tracks.Count < 2 || !tracks[1].Track.IsPartOfStation()) return false;

            //s_stationTracks.Add(track);
            //return true;
        }

        private static List<TrackSignalController> CreateBranchSignals(Junction junction,
            Dictionary<RailTrack, PlacementHelper> branchTrackKey, float distance)
        {
            var signals = new List<TrackSignalController>();

            foreach (var branch in junction.outBranches)
            {
                var track = branch.track.outBranch.track;

                if (!branchTrackKey.TryGetValue(track, out var helper) || helper.Definition == null) continue;

                var kpSet = track.GetKinkedPointSet();
                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
                var tSpan = kpSet.GetSpan(distance, tDirT);
                var index = kpSet.GetPointIndexForSpan(tSpan);
                var point = kpSet.points[index];

                var placement = new SignalPlacementInfo(track, tDirT, index, tSpan);
                var signal = InstantiateFromDef(helper.Definition, point.position, tDirT.IsOut() ? point.forward : -point.forward, false, track);
                var controller = new TrackSignalController(signal, branch.track, TrackDirection.In, placement);

                helper.Apply(controller);
                signals.Add(controller);
            }

            return signals;
        }

        private static List<TrackSignalController> CreateBranchSignalsOppositeDouble(Junction junction,
            Dictionary<RailTrack, PlacementHelper> branchTrackKey, float distance)
        {
            var signals = new List<TrackSignalController>();

            if (junction.outBranches.Count != 2) return CreateBranchSignals(junction, branchTrackKey, distance);

            for (int i = 0; i < junction.outBranches.Count; i++)
            {
                Junction.Branch? branch = junction.outBranches[i];
                var track = branch.track.outBranch.track;

                if (!branchTrackKey.TryGetValue(track, out var helper) || helper.Definition == null) continue;

                var kpSet = track.GetKinkedPointSet();
                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
                var tSpan = kpSet.GetSpan(distance, tDirT);
                var index = kpSet.GetPointIndexForSpan(tSpan);
                var point = kpSet.points[index];

                var placement = new SignalPlacementInfo(track, tDirT, index, tSpan);
                var signal = InstantiateFromDef(helper.Definition, point.position, tDirT.IsOut() ? point.forward : -point.forward,
                    helper.Definition.Offset > 0 ? i == 0 : i != 0, track);
                var controller = new TrackSignalController(signal, branch.track, TrackDirection.In, placement);

                helper.Apply(controller);
                signals.Add(controller);
            }

            return signals;
        }

        public override int MergeSignals(SignalPack pack, Dictionary<Junction, JunctionSignalGroup> registry)
        {
            var toDelete = new Dictionary<TrackSignalController, HashSet<TrackSignalController>>();
            var toMerge = new Dictionary<TrackSignalController, HashSet<TrackSignalController>>();

            // Check for possible merge targets.
            foreach (var group in registry)
            {
                foreach (var controller in group.Value.AllControllers)
                {
                    if (controller.Type == SignalType.Shunting) continue;

                    controller.UpdateBlocks();

                    foreach (var potential in controller.GetPotentialNextControllers())
                    {
                        foreach (var next in potential.Controllers)
                        {
                            if (next.Key is TrackSignalController track)
                            {
                                // Close enough to merge and signal types allow merging.
                                if (next.Value <= ClosenessThreshold && CanBeMerged(controller, track))
                                {
                                    AddToMap(track, controller);
                                }
                            }
                        }
                    }

                    controller.FlagAllBlocksForUpdating();
                }
            }

            int count = toMerge.Count;

            foreach (var merge in toMerge)
            {
                var remain = merge.Key;
                var result = Merge(pack, remain, merge.Value);

                if (result != null)
                {
                    var group = remain.Group;
                    result.Group = group;

                    if (group != null)
                    {
                        // Check which signal the remaining one was...
                        if (group.JunctionSignal == remain && result is JunctionSignalController junction)
                        {
                            group.JunctionSignal = junction;
                        }
                        else if (group.ReverseJunctionSignal == remain)
                        {
                            group.ReverseJunctionSignal = result;
                        }
                        else
                        {
                            group.BranchSignals.Add(result);
                        }
                    }

                    remain.Destroy();

                    foreach (var item in merge.Value)
                    {
                        item.Destroy();
                    }
                }
            }

            return count;

            void AddToMap(TrackSignalController delete, TrackSignalController remain)
            {
                if (!toDelete.TryGetValue(delete, out var set) || set == null)
                {
                    set = new HashSet<TrackSignalController>();
                    toDelete[delete] = set;
                }

                set.Add(remain);

                if (!toMerge.TryGetValue(remain, out set) || set == null)
                {
                    set = new HashSet<TrackSignalController>();
                    toMerge[remain] = set;
                }

                set.Add(delete);
            }
        }
    }
}
