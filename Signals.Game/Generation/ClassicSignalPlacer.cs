//using DV.PointSet;
//using Signals.Common;
//using Signals.Game.Controllers;
//using Signals.Game.Railway;
//using System.Collections.Generic;
//using System.Linq;

//namespace Signals.Game.Generation
//{
//    public sealed class ClassicSignalPlacer : SignalPlacer
//    {
//        private enum SignalCreationMode
//        {
//            None,
//            Mainline,
//            IntoYard,
//            IntoYardReverse,
//            IntoPax,
//            Shunting
//        }

//        private new const string YardNameStart = "[Y]";
//        private new const float BranchPlacementDistance = 17.5f;
//        private new const float BranchDistanceThreshold = 4.5f * 4.5f;
//        private new const float SmallTrackThreshold = 60.0f;

//        public override void CreateSignals(SignalPack pack, Dictionary<Junction, JunctionSignalGroup> registry)
//        {
//            foreach (var junction in RailTrackRegistry.Junctions)
//            {
//                switch (ShouldMakeSignal(registry, junction))
//                {
//                    case SignalCreationMode.Mainline:
//                        registry.Add(junction, CreateMainlineSignals(pack, junction));
//                        break;
//                    case SignalCreationMode.IntoYard:
//                        registry.Add(junction, CreateIntoYardSignals(pack, junction));
//                        break;
//                    case SignalCreationMode.IntoYardReverse:
//                        registry.Add(junction, CreateIntoYardReverseSignals(pack, junction));
//                        break;
//                    case SignalCreationMode.IntoPax:
//                        registry.Add(junction, CreateIntoPaxSignals(pack, junction));
//                        break;
//                    case SignalCreationMode.Shunting:
//                        registry.Add(junction, CreateShuntingSignals(pack, junction));
//                        break;
//                    default:
//                        continue;
//                }
//            }
//        }

//        private SignalCreationMode ShouldMakeSignal(Dictionary<Junction, JunctionSignalGroup> registry, Junction junction)
//        {
//            var inName = junction.inBranch.track.name;
//            SignalsMod.LogVerbose($"Testing track '{inName}' for signals...");

//            // Don't duplicate junctions and don't make signals at short dead ends.
//            if (registry.ContainsKey(junction) || InIsShortDeadEnd(junction))
//            {
//                return SignalCreationMode.None;
//            }

//            // If the in track belongs to a yard...
//            if (inName.StartsWith(YardNameStart))
//            {
//                // Check all out branches.
//                foreach (var branch in junction.outBranches)
//                {
//                    // Track ends after a switch for some reason, plop signal.
//                    if (!branch.track.outIsConnected)
//                    {
//                        return SignalCreationMode.IntoYard;
//                    }

//                    // Get the track after the switch track.
//                    var outName = branch.track.outBranch.track.name;

//                    SignalsMod.LogVerbose($"Testing branch '{outName}' for signals...");

//                    // If this yard track goes to a non yard track...
//                    if (!outName.StartsWith(YardNameStart))
//                    {
//                        return SignalCreationMode.IntoYardReverse;
//                    }
//                }

//                // Switch branches stay in the same yard, no signal needed.
//                return SignalCreationMode.Shunting;
//            }

//            // Count very small branches as if they were a yard.
//            // There's no regular mainline tracks fulfilling this condition.
//            if (AreAllBranchesSmallNonSign(junction))
//            {
//                return SignalCreationMode.IntoYard;
//            }

//            // In case we are in a mainline, check all out branches for yards.
//            foreach (var branch in junction.outBranches)
//            {
//                // Track ends after a switch for some reason, plop signal.
//                if (!branch.track.outIsConnected)
//                {
//                    return SignalCreationMode.IntoYard;
//                }

//                // Get the track after the switch track.
//                var outName = branch.track.outBranch.track.name;

//                SignalsMod.LogVerbose($"Testing branch '{outName}' for signals...");

//                // If this non yard track goes to a yard track...
//                if (outName.StartsWith(YardNameStart))
//                {
//                    // Special signals for passenger tracks.
//                    if (junction.outBranches.Any(x => IsPaxTrack(x.track.outBranch.track)))
//                    {
//                        return SignalCreationMode.IntoPax;
//                    }

//                    return SignalCreationMode.IntoYard;
//                }
//            }

//            return SignalCreationMode.Mainline;
//        }

//        private static JunctionSignalGroup CreateMainlineSignals(SignalPack pack, Junction junction)
//        {
//            SignalsMod.LogVerbose($"Making mainline signals for junction '{junction.junctionData.junctionIdLong}'");

//            var old = IsOld(pack, junction);
//            var left = junction.IsLeft();
//            var group = new JunctionSignalGroup(junction,
//                CreateSignalAtJunction(junction, pack.GetJunctionSignal(old, left)),
//                CreateBranchSignals(junction, pack.GetMainlineSignal(old)));

//            foreach (var signal in group.AllControllers)
//            {
//                signal.IsOld = old;
//                signal.Type = SignalType.Mainline;
//                signal.PrefabType = PrefabType.Mainline;
//            }

//            if (group.JunctionSignal != null)
//            {
//                group.JunctionSignal.PrefabType = left ? PrefabType.JunctionLeft : PrefabType.JunctionRight;
//            }

//            return group;
//        }

//        private static JunctionSignalGroup CreateIntoYardSignals(SignalPack pack, Junction junction)
//        {
//            var smallBranches = AreAllBranchesSmallNonSign(junction);

//            // Create more regular signals rather than the full into yard group.
//            if (!smallBranches && junction.outBranches.Any(x => !x.track.outBranch.track.IsPartOfYard()))
//            {
//                return CreateIntoYardMainlineSignals(pack, junction);
//            }

//            SignalsMod.LogVerbose($"Making into {(smallBranches ? "small branches" : string.Empty)} " +
//                $"yard signals for junction '{junction.junctionData.junctionIdLong}'");

//            var old = IsOld(pack, junction);
//            var group = new JunctionSignalGroup(junction,
//                CreateSignalAtJunction(junction, smallBranches ? pack.GetMainlineSignal(old) : pack.GetEntrySignal(old)),
//                CreateSignalFromJunction(junction, pack.GetMainlineSignal(old)));

//            if (group.JunctionSignal != null)
//            {
//                group.JunctionSignal.IsOld = old;
//                group.JunctionSignal.Type = smallBranches ? SignalType.Mainline : SignalType.Entry;
//                group.JunctionSignal.PrefabType = smallBranches ? PrefabType.Mainline : PrefabType.Entry;
//            }

//            if (group.ReverseJunctionSignal != null)
//            {
//                group.ReverseJunctionSignal.IsOld = old;
//                group.ReverseJunctionSignal.Type = SignalType.Mainline;
//                group.ReverseJunctionSignal.PrefabType = PrefabType.Mainline;
//            }

//            return group;
//        }

//        private static JunctionSignalGroup CreateIntoYardMainlineSignals(SignalPack pack, Junction junction)
//        {
//            SignalsMod.LogVerbose($"Making into yard mainline signals for junction '{junction.junctionData.junctionIdLong}'");

//            var old = IsOld(pack, junction);
//            var group = new JunctionSignalGroup(junction,
//                CreateSignalAtJunction(junction, pack.GetEntrySignal(old)),
//                CreateBranchSignals(junction, pack.GetMainlineSignal(old)));

//            foreach (var signal in group.AllControllers)
//            {
//                signal.IsOld = old;
//                signal.Type = SignalType.Mainline;
//                signal.PrefabType = PrefabType.Mainline;
//            }

//            if (group.JunctionSignal != null)
//            {
//                group.JunctionSignal.Type = SignalType.Entry;
//                group.JunctionSignal.PrefabType = PrefabType.Entry;
//            }

//            return group;
//        }

//        private static JunctionSignalGroup CreateIntoYardReverseSignals(SignalPack pack, Junction junction)
//        {
//            SignalsMod.LogVerbose($"Making into yard reverse signals for junction '{junction.junctionData.junctionIdLong}'");

//            var old = IsOld(pack, junction);
//            var left = junction.IsLeft();
//            var inPax = IsPaxTrack(junction.inBranch.track) && (old ? pack.OldPassengerSignal : pack.PassengerSignal) != null;
//            var group = new JunctionSignalGroup(junction,
//                CreateSignalAtJunction(junction, inPax ? pack.GetPassengerSignal(old) : pack.GetJunctionSignal(old, left)),
//                CreateBranchSignals(junction, pack.GetMainlineSignal(old)));

//            foreach (var signal in group.AllControllers)
//            {
//                signal.IsOld = old;
//                signal.Type = SignalType.Mainline;
//                signal.PrefabType = PrefabType.Mainline;
//            }

//            if (group.JunctionSignal != null)
//            {
//                group.JunctionSignal.Type = inPax ? SignalType.ExitPax : SignalType.Mainline;
//                group.JunctionSignal.PrefabType = inPax ? PrefabType.ExitPax : (left ? PrefabType.JunctionLeft : PrefabType.JunctionRight);
//            }

//            return group;
//        }

//        private static JunctionSignalGroup CreateIntoPaxSignals(SignalPack pack, Junction junction)
//        {
//            SignalsMod.LogVerbose($"Making into pax signals for junction '{junction.junctionData.junctionIdLong}'");

//            var old = IsOld(pack, junction);
//            var group = new JunctionSignalGroup(junction,
//                CreateSignalAtJunction(junction, pack.GetEntrySignal(old)),
//                CreateBranchSignalsPax(junction, pack.GetPassengerSignal(old), pack.GetMainlineSignal(old)));

//            foreach (var signal in group.AllControllers)
//            {
//                signal.IsOld = old;
//            }

//            if (group.JunctionSignal != null)
//            {
//                group.JunctionSignal.Type = SignalType.Entry;
//                group.JunctionSignal.PrefabType = PrefabType.Entry;
//            }

//            return group;
//        }

//        private static JunctionSignalGroup CreateShuntingSignals(SignalPack pack, Junction junction)
//        {
//            SignalsMod.LogVerbose($"Making shunting signals for junction '{junction.junctionData.junctionIdLong}'");

//            var old = IsOld(pack, junction);
//            var def = pack.GetShuntingSignal(old);

//            if (def == null) return new JunctionSignalGroup(junction);

//            var group = new JunctionSignalGroup(junction, null,
//                //CreateSignalAtJunction(junction, def),
//                CreateShuntingBranchSignals());

//            return group;

//            List<TrackSignalController> CreateShuntingBranchSignals()
//            {
//                EquiPointSet.Point? prev = null;
//                float baseSpan = BranchPlacementDistance;
//                var list = new List<TrackSignalController>();

//                // Check potential placements to see if they're too close to eachother.
//                foreach (var branch in junction.outBranches)
//                {
//                    var track = branch.track.outBranch.track;
//                    var kpSet = track.GetKinkedPointSet();
//                    var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
//                    var index = kpSet.GetPointIndexForSpan(kpSet.GetSpan(BranchPlacementDistance, tDirT));
//                    var point = kpSet.points[index];

//                    if (prev != null)
//                    {
//                        var distance = (point.position - prev.Value.position).sqrMagnitude;

//                        // Just 1 confirmation is needed, so all of them are moved.
//                        if (distance < BranchDistanceThreshold)
//                        {
//                            baseSpan = BranchPlacementDistance + 10;
//                            break;
//                        }
//                    }

//                    prev = point;
//                }

//                foreach (var branch in junction.outBranches)
//                {
//                    var track = branch.track.outBranch.track;

//                    // Don't spam switches with them, only the actual numbered tracks and big ones.
//                    if (!IsLogicYardTrack(track) && track.GetLength() <= SmallTrackThreshold) continue;

//                    var kpSet = track.GetKinkedPointSet();
//                    var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
//                    var tSpan = kpSet.GetSpan(baseSpan, tDirT);
//                    var index = kpSet.GetPointIndexForSpan(tSpan);
//                    var point = kpSet.points[index];

//                    var placement = new SignalPlacementInfo(track, tDirT, index, tSpan);
//                    var signal = InstantiateFromDef(def, point.position, tDirT.IsOut() ? point.forward : -point.forward, false, track);

//                    list.Add(new TrackSignalController(signal, branch.track, tDirT, placement));
//                }

//                return list;
//            }
//        }

//        private static TrackSignalController? CreateSignalFromJunction(Junction junction, SignalControllerDefinition definition)
//        {
//            var track = junction.inBranch.track;
//            var kpSet = track.GetKinkedPointSet();
//            var tDirJ = TrackUtils.TrackDirectionFromJunction(track, junction);
//            var tSpan = kpSet.GetSpan(JunctionPlacementDistance, tDirJ);
//            var index = kpSet.GetPointIndexForSpan(tSpan);
//            var point = kpSet.points[index];

//            tDirJ = tDirJ.Flipped();
//            var placement = new SignalPlacementInfo(track, tDirJ, index, tSpan);
//            var signal = InstantiateFromDef(definition, point.position, tDirJ.IsOut() ? point.forward : -point.forward, false, track);

//            return new TrackSignalController(signal, track, tDirJ.Flipped(), placement);
//        }

//        private static List<TrackSignalController> CreateBranchSignals(Junction junction, SignalControllerDefinition definition)
//        {
//            var signals = new List<TrackSignalController>();

//            EquiPointSet.Point? prev = null;
//            float baseSpan = BranchPlacementDistance;

//            // Check potential placements to see if they're too close to eachother.
//            foreach (var branch in junction.outBranches)
//            {
//                var track = branch.track.outBranch.track;
//                var kpSet = track.GetKinkedPointSet();
//                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
//                var index = kpSet.GetPointIndexForSpan(kpSet.GetSpan(BranchPlacementDistance, tDirT));
//                var point = kpSet.points[index];

//                if (prev != null)
//                {
//                    var distance = (point.position - prev.Value.position).sqrMagnitude;

//                    // Just 1 confirmation is needed, so all of them are moved.
//                    if (distance < BranchDistanceThreshold)
//                    {
//                        baseSpan = BranchPlacementDistance + 10;
//                        break;
//                    }
//                }

//                prev = point;
//            }

//            foreach (var branch in junction.outBranches)
//            {
//                var track = branch.track.outBranch.track;

//                // Skip very small branch tracks to avoid placement issues.
//                if (IsSmallTrack(track)) continue;

//                var kpSet = track.GetKinkedPointSet();
//                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
//                var tSpan = kpSet.GetSpan(baseSpan, tDirT);
//                var index = kpSet.GetPointIndexForSpan(tSpan);
//                var point = kpSet.points[index];

//                var placement = new SignalPlacementInfo(track, tDirT, index, tSpan);
//                var signal = InstantiateFromDef(definition, point.position, tDirT.IsOut() ? point.forward : -point.forward, false, track);

//                signals.Add(new TrackSignalController(signal, branch.track, TrackDirection.In, placement));
//            }

//            return signals;
//        }

//        private static List<TrackSignalController> CreateBranchSignalsPax(Junction junction, SignalControllerDefinition pax, SignalControllerDefinition nonPax)
//        {
//            var signals = new List<TrackSignalController>();

//            EquiPointSet.Point? prev = null;
//            float baseSpan = BranchPlacementDistance;

//            // Check potential placements to see if they're too close to eachother.
//            foreach (var branch in junction.outBranches)
//            {
//                var track = branch.track.outBranch.track;
//                var kpSet = track.GetKinkedPointSet();
//                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
//                var index = kpSet.GetPointIndexForSpan(kpSet.GetSpan(BranchPlacementDistance, tDirT));
//                var point = kpSet.points[index];

//                if (prev != null)
//                {
//                    var distance = (point.position - prev.Value.position).sqrMagnitude;

//                    // Just 1 confirmation is needed, so all of them are moved.
//                    if (distance < BranchDistanceThreshold)
//                    {
//                        baseSpan = BranchPlacementDistance + 10;
//                        break;
//                    }
//                }

//                prev = point;
//            }

//            foreach (var branch in junction.outBranches)
//            {
//                var track = branch.track.outBranch.track;
//                var kpSet = track.GetKinkedPointSet();
//                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
//                var tSpan = kpSet.GetSpan(baseSpan, tDirT);
//                var index = kpSet.GetPointIndexForSpan(tSpan);
//                var point = kpSet.points[index];

//                var paxEnd = IsPaxTrack(track);
//                var placement = new SignalPlacementInfo(track, tDirT, index, tSpan);
//                var signal = InstantiateFromDef(paxEnd ? pax : nonPax,
//                    point.position, tDirT.IsOut() ? point.forward : -point.forward, false, track);

//                var controller = new TrackSignalController(signal, branch.track, TrackDirection.In, placement)
//                {
//                    Type = paxEnd ? SignalType.ExitPax : SignalType.Mainline,
//                    PrefabType = paxEnd ? PrefabType.ExitPax : PrefabType.Mainline
//                };

//                signals.Add(controller);
//            }

//            return signals;
//        }
//    }
//}
