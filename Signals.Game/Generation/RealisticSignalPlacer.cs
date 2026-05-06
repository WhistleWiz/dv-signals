using Signals.Common;
using Signals.Game.Controllers;
using Signals.Game.Railway;
using System.Collections.Generic;

namespace Signals.Game.Generation
{
    public sealed class RealisticSignalPlacer : SignalPlacer
    {
        private struct PlacementHelper
        {
            public SignalControllerDefinition Definition;
            public PrefabType PrefabType;
            public SignalType SignalType;
            public bool Old;

            public PlacementHelper(SignalControllerDefinition definition, PrefabType prefabType, bool old)
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

        public override void CreateSignals(SignalPack pack, Dictionary<Junction, JunctionSignalGroup> registry)
        {
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

            // Check branches.
            foreach (var branch in junction.outBranches)
            {
                var track = branch.track.outBranch.track;
                var deadEnd = false;
                var deadCheck = false;

                // If it's any of the logic yard tracks, it needs a signal for exit.
                if (IsPaxTrack(track) && !GoesToDeadEnd())
                {
                    branchTrackKey.Add(track, GetPlacement(PrefabType.ExitPax, old));
                    anyYard = true;
                }
                else if (IsInOrOutTrack(track) && !GoesToDeadEnd())
                {
                    branchTrackKey.Add(track, GetPlacement(PrefabType.Exit, old));
                    anyYard = true;
                }
                else if (IsLogicYardTrack(track))
                {
                    branchTrackKey.Add(track, GetPlacement(PrefabType.Shunting, old));
                    anyYard = true;
                }
                // If it's not part of a station, then it requires a normal signal.
                else if (!track.IsPartOfStation())
                {
                    if (IsShortDeadEnd(track) || ComesFromStationAndIsShort(track))
                    {
                        branchTrackKey.Add(track, GetPlacement(PrefabType.Shunting, old));
                    }
                    else
                    {
                        branchTrackKey.Add(track, GetPlacement(PrefabType.Mainline, old));
                    }
                }
                // If it's a station track between 2 mains, it needs a normal signal.
                else if (IsBetweenMains(track))
                {
                    branchTrackKey.Add(track, GetPlacement(PrefabType.Mainline, old));
                }
                else
                {
                    if (track.GetLength() < LongTrackThreshold)
                    {
                        anyYard = true;
                    }

                    // Generic yard tracks required a certain length to have a signal.
                    if (track.GetLength() > SmallTrackThreshold)
                    {
                        // If this track can be considered a mainline through the yard, then give it proper signals.
                        if (IsYardTrackMainline(branch))
                        {
                            // Manually change the type.
                            var placement = GetPlacement(PrefabType.Exit, old);
                            placement.SignalType = SignalType.Mainline;
                            branchTrackKey.Add(track, placement);
                        }
                        else
                        {
                            branchTrackKey.Add(track, GetPlacement(PrefabType.Shunting, old));
                        }
                    }
                }

                // Quick function to prevent running this loop multiple times.
                bool GoesToDeadEnd()
                {
                    if (!deadCheck)
                    {
                        deadEnd = TrackWalker.GoesToDeadEnd(branch.track, TrackDirection.In);
                    }

                    return deadEnd;
                }
            }

            // If the in track is a generic yard track it doesn't get a signal, unless it's big.
            if (inTrack.IsPartOfYard() && !IsLogicYardTrack(inTrack) && inTrack.GetLength() < SmallTrackThreshold)
            {
                // If there's also no branch signals, then don't create a group.
                if (branchTrackKey.Count == 0) return null;

                // Create signals at the branches from the keys.
                return new JunctionSignalGroup(junction, null, CreateBranchSignals(junction, branchTrackKey, branchDistance));
            }

            PlacementHelper junctionSignal;

            // For the junction side, do the same checks for inputs and outputs.
            if (IsPaxTrack(inTrack))
            {
                junctionSignal = GetPlacement(PrefabType.ExitPax, old);
            }
            else if (IsInOrOutTrack(inTrack))
            {
                junctionSignal = GetPlacement(PrefabType.Exit, old);
            }
            else if (IsLogicYardTrack(inTrack) || inTrack.IsNonSign() || IsShortDeadEnd(inTrack))
            {
                // Track is too small for a signal.
                if (inTrack.GetLength() < VerySmallTrackThreshold)
                {
                    return new JunctionSignalGroup(junction, null, CreateBranchSignals(junction, branchTrackKey, branchDistance));
                }

                junctionSignal = GetPlacement(PrefabType.Shunting, old);
            }
            // If the track goes into any yard track...
            else if (anyYard)
            {
                junctionSignal = GetPlacement(PrefabType.Entry, old);
            }
            else
            {
                junctionSignal = junction.IsLeft() ? GetPlacement(PrefabType.JunctionLeft, old) : GetPlacement(PrefabType.JunctionRight, old);
            }

            var junctionController = CreateSignalAtJunction(junction, junctionSignal.Definition);
            junctionSignal.Apply(junctionController);

            return new JunctionSignalGroup(junction, junctionController,
                CreateBranchSignals(junction, branchTrackKey, branchDistance));

            PlacementHelper GetPlacement(PrefabType prefabType, bool old)
            {
                return new PlacementHelper(GetForType(pack, prefabType, old), prefabType, old);
            }
        }

        private static SignalType GetSignalType(PrefabType prefab) => prefab switch
        {
            PrefabType.Mainline => SignalType.Mainline,
            PrefabType.JunctionLeft => SignalType.Mainline,
            PrefabType.JunctionRight => SignalType.Mainline,
            PrefabType.Entry => SignalType.Entry,
            PrefabType.Exit => SignalType.Exit,
            PrefabType.ExitPax => SignalType.ExitPax,
            PrefabType.Shunting => SignalType.Shunting,
            _ => SignalType.NotSet,
        };

        private static bool IsBetweenMains(RailTrack track)
        {
            //var inBranch = track.GetInBranch();
            //var outBranch = track.GetOutBranch();

            //if (inBranch == null || outBranch == null || !inBranch.track.isJunctionTrack || !outBranch.track.isJunctionTrack)
            //{
            //    return false;
            //}

            //inBranch = inBranch.track.GetInBranch();
            //outBranch = outBranch.track.GetInBranch();

            //if (inBranch == null || outBranch == null)
            //{
            //    return false;
            //}

            //return !IsPartOfStation(inBranch.track) && !IsPartOfStation(outBranch.track);

            var inSide = TrackWalker.WalkTracks(track, TrackDirection.In, 2);
            // Not enough tracks.
            if (inSide.Count < 2) return false;

            var outSide = TrackWalker.WalkTracks(track, TrackDirection.Out, 2);
            if (outSide.Count < 2) return false;

            return !inSide[1].Track.IsPartOfStation() && !outSide[1].Track.IsPartOfStation();
        }

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

            if (branch.track.name != "[track through]") return false;

            var tracks = TrackWalker.WalkTracks(branch.track, TrackDirection.In, 3);

            if (tracks.Count < 3 || !tracks[0].Track.IsPartOfStation()) return false;

            return !tracks[2].Track.IsPartOfYard();
        }

        private static List<TrackSignalController> CreateBranchSignals(Junction junction,
            Dictionary<RailTrack, PlacementHelper> branchTrackKey, float distance)
        {
            var signals = new List<TrackSignalController>();

            foreach (var branch in junction.outBranches)
            {
                var track = branch.track.outBranch.track;

                if (!branchTrackKey.TryGetValue(track, out var helper)) continue;

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

                    foreach (var block in controller.GetPotentialBlocks())
                    {
                        var next = block.NextController;

                        if (next is TrackSignalController track)
                        {
                            // Close enough to merge and signal types allow merging.
                            if (block.Length <= ClosenessThreshold && CanBeMerged(controller, track))
                            {
                                AddToMap(track, controller);
                            }
                        }
                    }
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
