using DV.PointSet;
using Signals.Common;
using Signals.Game.Controllers;
using Signals.Game.Railway;
using Signals.Game.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Signals.Game.Generation
{
    public abstract class SignalPlacer
    {
        protected const string YardNameStart = "[Y]";
        protected const string PaxNameEnd = "LP]";
        protected const string OutputEnd = "O]";
        protected const string InputEnd = "I]";
        protected const string LoadingEnd = "L]";
        protected const string StorageEnd = "S]";
        protected const float JunctionPlacementDistance = 2.0f;
        protected const float LongJunctionPlacementDistance = 15.0f;
        protected const float BranchPlacementDistance = 17.5f;
        protected const float BranchDistanceThreshold = 4.5f * 4.5f;
        protected const float DeadEndThreshold = 100.0f;
        protected const float LongTrackThreshold = 300.0f;
        protected const float ClosenessThreshold = 125.0f;
        protected const float SmallTrackThreshold = 60.0f;
        protected const float VerySmallTrackThreshold = 15.0f;
        protected const float SpacingThreshold = 210.0f;

        #region Checks

        public static bool InIsShortDeadEnd(Junction junction)
        {
            // If the track is null here something is very wrong with the game, so don't check.
            var track = junction.inBranch.track;

            if (track.GetLength() > DeadEndThreshold)
            {
                return false;
            }

            var branch = track.inJunction == junction ?
                track.GetOutBranch() :
                track.GetInBranch();

            return branch == null;
        }

        public static bool IsSmallTrack(RailTrack track)
        {
            return track.GetLength() < SmallTrackThreshold;
        }

        public static bool IsPaxTrack(RailTrack track)
        {
            return track.name.EndsWith(PaxNameEnd);
        }

        public static bool IsOutputTrack(RailTrack track)
        {
            return track.name.EndsWith(OutputEnd);
        }

        public static bool IsInputTrack(RailTrack track)
        {
            return track.name.EndsWith(InputEnd);
        }

        public static bool IsInOrOutTrack(RailTrack track)
        {
            return IsOutputTrack(track) || IsInputTrack(track);
        }

        public static bool IsLoadingTrack(RailTrack track)
        {
            return track.name.EndsWith(LoadingEnd);
        }

        public static bool IsLogicYardTrack(RailTrack track)
        {
            return !track.GetId().IsGeneric();
        }

        public static bool IsShortDeadEnd(RailTrack track)
        {
            return !(track.inIsConnected && track.outIsConnected) && track.GetLength() < DeadEndThreshold;
        }

        public static bool AreAllBranchesSmallNonSign(Junction junction)
        {
            foreach (var item in junction.outBranches)
            {
                var track = item.track.outBranch.track;

                if (track == null || track.GetLength() > VerySmallTrackThreshold || !track.IsNonSign())
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsOld(Junction junction)
        {
            return OldAreaCalculator.IsWithinOldArea(junction.position);
        }

        protected static bool GetsDistant(SignalType type)
        {
            switch (type)
            {
                case SignalType.Mainline:
                case SignalType.Entry:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Basic Creation Methods

        public static SignalControllerDefinition GetForType(SignalPack pack, PrefabType prefabType, bool old) => prefabType switch
        {
            PrefabType.Diverging => pack.GetDivergingSignal(old),
            PrefabType.JunctionLeft => pack.GetLeftJunctionSignal(old),
            PrefabType.JunctionRight => pack.GetRightJunctionSignal(old),
            PrefabType.Entry => pack.GetEntrySignal(old),
            PrefabType.Exit => pack.GetExitSignal(old),
            PrefabType.ExitPax => pack.GetPassengerSignal(old),
            PrefabType.Shunting => pack.GetShuntingSignal(old),
            PrefabType.StationMainline => pack.GetStationMainlineSignal(old),
            _ => pack.GetMainlineSignal(old),
        };

        public static SignalControllerDefinition InstantiateFromDef(SignalControllerDefinition definition,
            Vector3d position, Vector3 direction, bool opposite, RailTrack track)
        {
            var go = Object.Instantiate(definition, (Vector3)position, Helpers.FlattenLook(direction), track.transform);
            go.transform.position += go.transform.right * (opposite ? -definition.Offset : definition.Offset);
            return go;
        }

        public static float BranchSignalDistance(Junction junction, float startingDistance)
        {
            EquiPointSet.Point? prev = null;

            // Check potential placements to see if they're too close to eachother.
            foreach (var branch in junction.outBranches)
            {
                var track = branch.track.outBranch.track;
                var kpSet = track.GetKinkedPointSet();
                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
                var index = kpSet.GetPointIndexForSpan(kpSet.GetSpan(BranchPlacementDistance, tDirT));
                var point = kpSet.points[index];

                if (prev != null)
                {
                    var distance = (point.position - prev.Value.position).sqrMagnitude;

                    // Just 1 confirmation is needed, so all of them are moved.
                    if (distance < BranchDistanceThreshold)
                    {
                        return startingDistance + 10;
                    }
                }

                prev = point;
            }

            return startingDistance;
        }

        protected static JunctionSignalController? CreateSignalAtJunction(Junction junction, SignalControllerDefinition definition,
            float distance = JunctionPlacementDistance)
        {
            var track = junction.inBranch.track;
            var kpSet = track.GetKinkedPointSet();
            var tDirJ = TrackUtils.TrackDirectionFromJunction(track, junction);
            var tSpan = kpSet.GetSpan(distance, tDirJ);
            var index = kpSet.GetPointIndexForSpan(tSpan);
            var point = kpSet.points[index];

            var placement = new SignalPlacementInfo(track, tDirJ, index, tSpan);
            var signal = InstantiateFromDef(definition, point.position, tDirJ.IsOut() ? point.forward : -point.forward, false, track);

            return new JunctionSignalController(signal, junction, null, placement);
        }

        protected static TrackSignalController? CreateSignalFromJunction(Junction junction, SignalControllerDefinition definition)
        {
            var track = junction.inBranch.track;
            var kpSet = track.GetKinkedPointSet();
            var tDirJ = TrackUtils.TrackDirectionFromJunction(track, junction);
            var tSpan = kpSet.GetSpan(JunctionPlacementDistance, tDirJ);
            var index = kpSet.GetPointIndexForSpan(tSpan);
            var point = kpSet.points[index];

            tDirJ = tDirJ.Flipped();
            var placement = new SignalPlacementInfo(track, tDirJ, index, tSpan);
            var signal = InstantiateFromDef(definition, point.position, tDirJ.IsOut() ? point.forward : -point.forward, false, track);

            return new TrackSignalController(signal, track, tDirJ.Flipped(), placement);
        }

        protected static List<TrackSignalController> CreateBranchSignals(Junction junction, SignalControllerDefinition definition)
        {
            var signals = new List<TrackSignalController>();

            EquiPointSet.Point? prev = null;
            float baseSpan = BranchPlacementDistance;

            // Check potential placements to see if they're too close to eachother.
            foreach (var branch in junction.outBranches)
            {
                var track = branch.track.outBranch.track;
                var kpSet = track.GetKinkedPointSet();
                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
                var index = kpSet.GetPointIndexForSpan(kpSet.GetSpan(BranchPlacementDistance, tDirT));
                var point = kpSet.points[index];

                if (prev != null)
                {
                    var distance = (point.position - prev.Value.position).sqrMagnitude;

                    // Just 1 confirmation is needed, so all of them are moved.
                    if (distance < BranchDistanceThreshold)
                    {
                        baseSpan = BranchPlacementDistance + 10;
                        break;
                    }
                }

                prev = point;
            }

            foreach (var branch in junction.outBranches)
            {
                var track = branch.track.outBranch.track;

                // Skip very small branch tracks to avoid placement issues.
                if (IsSmallTrack(track)) continue;

                var kpSet = track.GetKinkedPointSet();
                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
                var tSpan = kpSet.GetSpan(baseSpan, tDirT);
                var index = kpSet.GetPointIndexForSpan(tSpan);
                var point = kpSet.points[index];

                var placement = new SignalPlacementInfo(track, tDirT, index, tSpan);
                var signal = InstantiateFromDef(definition, point.position, tDirT.IsOut() ? point.forward : -point.forward, false, track);

                signals.Add(new TrackSignalController(signal, branch.track, TrackDirection.In, placement));
            }

            return signals;
        }

        protected static List<TrackSignalController> CreateBranchSignalsPax(Junction junction, SignalControllerDefinition pax, SignalControllerDefinition nonPax)
        {
            var signals = new List<TrackSignalController>();

            EquiPointSet.Point? prev = null;
            float baseSpan = BranchPlacementDistance;

            // Check potential placements to see if they're too close to eachother.
            foreach (var branch in junction.outBranches)
            {
                var track = branch.track.outBranch.track;
                var kpSet = track.GetKinkedPointSet();
                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
                var index = kpSet.GetPointIndexForSpan(kpSet.GetSpan(BranchPlacementDistance, tDirT));
                var point = kpSet.points[index];

                if (prev != null)
                {
                    var distance = (point.position - prev.Value.position).sqrMagnitude;

                    // Just 1 confirmation is needed, so all of them are moved.
                    if (distance < BranchDistanceThreshold)
                    {
                        baseSpan = BranchPlacementDistance + 10;
                        break;
                    }
                }

                prev = point;
            }

            foreach (var branch in junction.outBranches)
            {
                var track = branch.track.outBranch.track;
                var kpSet = track.GetKinkedPointSet();
                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
                var tSpan = kpSet.GetSpan(baseSpan, tDirT);
                var index = kpSet.GetPointIndexForSpan(tSpan);
                var point = kpSet.points[index];

                var paxEnd = IsPaxTrack(track);
                var placement = new SignalPlacementInfo(track, tDirT, index, tSpan);
                var signal = InstantiateFromDef(paxEnd ? pax : nonPax,
                    point.position, tDirT.IsOut() ? point.forward : -point.forward, false, track);

                var controller = new TrackSignalController(signal, branch.track, TrackDirection.In, placement)
                {
                    Type = paxEnd ? SignalType.ExitPax : SignalType.Mainline,
                    PrefabType = paxEnd ? PrefabType.ExitPax : PrefabType.Mainline
                };

                signals.Add(controller);
            }

            return signals;
        }

        protected static DistantSignalController CreateDistantForSignal(BasicSignalController home, SignalControllerDefinition definition, float distance)
        {
            // This is checked before calling the method.
            var placement = home.PlacementInfo!.Value;

            var isOut = placement.Direction.IsOut();
            var kpSet = placement.Track.GetKinkedPointSet();
            var point = kpSet.points[placement.PointIndex];
            var tSpan = point.span + (isOut ? distance : -distance);
            var index = kpSet.GetPointIndexForSpan(tSpan);

            point = kpSet.points[index];
            placement.PointIndex = index;
            placement.Span = tSpan;
            var signal = InstantiateFromDef(definition, point.position, isOut ? point.forward : -point.forward, false, placement.Track);

            return new DistantSignalController(signal, home, placement, distance);
        }

        #endregion

        #region Post processing

        protected static bool CanBeMerged(BasicSignalController source, BasicSignalController target)
        {
            return source.Type switch
            {
                SignalType.Shunting => false,
                SignalType.Distant => false,
                SignalType.Other => false,
                _ => target.Type switch
                {
                    SignalType.Mainline => true,
                    SignalType.Entry => true,
                    SignalType.Shunting => false,
                    SignalType.Distant => false,
                    SignalType.Exit => false,
                    SignalType.ExitPax => false,
                    SignalType.Other => false,
                    _ => false,
                }
            };
        }

        protected static SignalType GetHigher(SignalType a, SignalType b)
        {
            if (Check(SignalType.Entry)) return SignalType.Entry;
            if (Check(SignalType.ExitPax)) return SignalType.ExitPax;
            if (Check(SignalType.Exit)) return SignalType.Exit;
            if (Check(SignalType.Mainline)) return SignalType.Mainline;

            return a;

            bool Check(SignalType type)
            {
                return a == type || b == type;
            }
        }

        protected static PrefabType GetHigher(PrefabType a, PrefabType b)
        {
            if (Check(PrefabType.Entry)) return PrefabType.Entry;
            if (Check(PrefabType.ExitPax)) return PrefabType.ExitPax;
            if (Check(PrefabType.Exit)) return PrefabType.Exit;
            if (Check(PrefabType.JunctionLeft)) return PrefabType.JunctionLeft;
            if (Check(PrefabType.JunctionRight)) return PrefabType.JunctionRight;
            if (Check(PrefabType.Diverging)) return PrefabType.Diverging;
            if (Check(PrefabType.Mainline)) return PrefabType.Mainline;

            return a;

            bool Check(PrefabType type)
            {
                return a == type || b == type;
            }
        }

        protected static TrackSignalController? Merge(SignalPack pack, TrackSignalController remain, HashSet<TrackSignalController> remove)
        {
            if (!remain.PlacementInfo.HasValue) return null;

            var signalType = remain.Type;
            var prefabType = remain.PrefabType;

            foreach (var item in remove)
            {
                signalType = GetHigher(signalType, item.Type);
                prefabType = GetHigher(prefabType, item.PrefabType);
            }

            var info = remain.PlacementInfo.Value;
            var point = info.Track.GetKinkedPointSet().points[info.PointIndex];
            var signal = InstantiateFromDef(GetForType(pack, prefabType, remain.IsOld),
                point.position, remain.Definition.transform.forward, false, info.Track);

            TrackSignalController controller;

            if (remain is JunctionSignalController junctionRemain)
            {
                controller = new JunctionSignalController(signal, junctionRemain.Junction, null, info);
            }
            else
            {
                controller = new TrackSignalController(signal, remain.StartingTrack, remain.Direction, info);
            }

            controller.IsOld = remain.IsOld;
            controller.Type = signalType;
            controller.PrefabType = prefabType;
            return controller;
        }

        protected static TrackSignalController? Merge(SignalPack pack, TrackSignalController remain, TrackSignalController remove)
        {
            if (!remain.PlacementInfo.HasValue) return null;

            return Merge(pack, remain, new HashSet<TrackSignalController> { remove });
        }

        #endregion

        public abstract void CreateSignals(SignalPack pack, Dictionary<Junction, JunctionSignalGroup> registry);

        /// <summary>
        /// Create distant signals for valid candidates.
        /// </summary>
        /// <param name="pack">The current pack in use.</param>
        /// <param name="registry">The existing junction signals.</param>
        /// <param name="distantSignals">The list in which the distant signals will be added.</param>
        public virtual void CreateDistantSignals(SignalPack pack, Dictionary<Junction, JunctionSignalGroup> registry, List<DistantSignalController> distantSignals)
        {
            var skip = new HashSet<BasicSignalController>();
            var totalDistance = pack.DistantSignalDistance + pack.DistantTolerance;

            foreach (var junction in registry)
            {
                foreach (var controller in junction.Value.AllControllers)
                {
                    foreach (var (signal, controllers) in controller.GetPotentialNextControllers())
                    {
                        var prefab = GetPrefabForControllerType(pack, controller);

                        // Signal doesn't have a distant part, skip.
                        if (signal.DistantSignal == null && !signal.Definition.SelfActsAsDistant && prefab == null) continue;

                        var hasDistant = false;

                        // Check all potential next controllers.
                        foreach (var next in controllers)
                        {
                            if (!next.PlacementInfo.HasValue) continue;

                            var length = next.PlacementInfo.Value.Track.GetLength();

                            // This controller is too far from the next, so it'll have it's own signal,
                            // or the track is too short for a distant signal.
                            if (length > totalDistance || length < pack.DistantSignalMinimumDistance) continue;

                            // Controller within tolerance, flag it to keep the distant signal,
                            // and flag the next to not need its own signal.
                            controller.ActingAsDistant = true;
                            hasDistant = true;
                            skip.Add(next);

                            // Very close, mark as such.
                            if (length < pack.DistantSignalDistance)
                            {
                                controller.ShortDistance = true;
                            }
                        }

                        if (!hasDistant)
                        {
                            signal.DestroyDistant();
                            continue;
                        }

                        if (prefab != null && controller.PlacementInfo.HasValue)
                        {
                            var info = controller.PlacementInfo.Value;
                            var point = info.Track.GetKinkedPointSet().points[info.PointIndex];
                            var instance = InstantiateFromDef(prefab, point.position, controller.Definition.transform.forward, info.OppositeSide, info.Track);

                            switch (controller)
                            {
                                case JunctionSignalController jController:
                                    JunctionSignalController.Replace(jController, instance);
                                    break;
                                case TrackSignalController tController:
                                    TrackSignalController.Replace(tController, instance);
                                    break;
                                default:
                                    break;
                            }

                            // Stop the loop since the whole controller was replaced.
                            break;
                        }
                    }
                }
            }

            if (!pack.HasAnyDistantSignal) return;

            foreach (var junction in registry)
            {
                foreach (var controller in junction.Value.AllControllers)
                {
                    // Signal type doesn't have a distant signal to match it.
                    if (!GetsDistant(controller.Type)) continue;

                    // We already have a signal handling it.
                    if (skip.Contains(controller)) continue;

                    // No signal to use or signal has no placement information.
                    if (controller.Signals.Length == 0) continue;
                    if (!controller.PlacementInfo.HasValue) continue;

                    // Prefab isn't available.
                    var prefab = pack.GetDistantSignal(controller.IsOld);
                    if (prefab == null) continue;

                    // Don't place distant signals in tracks inside stations.
                    var placement = controller.PlacementInfo.Value;
                    if (placement.Track.IsPartOfStation()) continue;

                    // Check the minimum track length.
                    var kpSet = placement.Track.GetKinkedPointSet();
                    if (kpSet.span < pack.DistantSignalMinimumDistance) continue;

                    // If the placement falls outside the track bounds...
                    var tSpan = placement.Span + (placement.Direction.IsOut() ? pack.DistantSignalDistance : -pack.DistantSignalDistance);
                    if (tSpan < 0 || tSpan > kpSet.span) continue;

                    // Actually place one.
                    distantSignals.Add(CreateDistantForSignal(controller, prefab, pack.DistantSignalDistance));
                }
            }

            static SignalControllerDefinition? GetPrefabForControllerType(SignalPack pack, BasicSignalController controller) => controller.PrefabType switch
            {
                PrefabType.Mainline => controller.IsOld ? pack.OldCombinedSignal : pack.CombinedSignal,
                PrefabType.JunctionLeft => controller.IsOld ? pack.OldCombinedLeftJunctionSignal : pack.CombinedLeftJunctionSignal,
                PrefabType.JunctionRight => controller.IsOld ? pack.OldCombinedRightJunctionSignal : pack.CombinedRightJunctionSignal,
                _ => null,
            };
        }

        /// <summary>
        /// Create repeater signals for valid candidates.
        /// </summary>
        /// <param name="pack">The current pack in use.</param>
        /// <param name="registry">The existing junction signals.</param>
        /// <param name="repeaterSignals">The list in which the repeaters will be added.</param>
        public virtual void CreateRepeaterSignals(SignalPack pack, Dictionary<Junction, JunctionSignalGroup> registry, List<DistantSignalController> repeaterSignals)
        {
            if (!pack.HasAnyRepeaterSignal) return;

            foreach (var junction in registry)
            {
                foreach (var controller in junction.Value.AllControllers)
                {
                    // Signal type doesn't have a repeater signal to match it.
                    if (!GetsDistant(controller.Type)) continue;

                    // No signal to use or signal has no placement information.
                    if (controller.Signals.Length == 0) continue;
                    if (!controller.PlacementInfo.HasValue) continue;

                    // Prefab isn't available.
                    var prefab = pack.GetRepeaterSignal(controller.IsOld);
                    if (prefab == null) continue;

                    // Don't place repeater signals in tracks inside stations.
                    var placement = controller.PlacementInfo.Value;
                    if (placement.Track.IsPartOfStation()) continue;

                    // Check the minimum track length.
                    var kpSet = placement.Track.GetKinkedPointSet();
                    if (kpSet.span < pack.RepeaterSignalMinimumDistance) continue;

                    // If the placement falls outside the track bounds...
                    var tSpan = placement.Span + (placement.Direction.IsOut() ? pack.RepeaterSignalDistance : -pack.RepeaterSignalDistance);
                    if (tSpan < 0 || tSpan > kpSet.span) continue;

                    var pointA = placement.Direction.IsOut() ? kpSet.points[0] : kpSet.points[kpSet.points.Length - 1];
                    var pointB = kpSet.points[kpSet.GetPointIndexForSpan(tSpan)];

                    // Visibility should be good, don't place.
                    if (Vector3.Dot(pointA.forward, pointB.forward) > 0.75f) continue;

                    // Actually place one.
                    repeaterSignals.Add(CreateDistantForSignal(controller, prefab, pack.RepeaterSignalDistance));
                }
            }
        }

        public void CreateTurntableSignals(SignalPack pack, List<TurntableSignalController> turntableSignals)
        {
            if (!pack.HasAnyTurntableSignal) return;

            var tracks = Object.FindObjectsOfType<TurntableRailTrack>();

            foreach (var turntableTrack in tracks)
            {
                // Exclude CS museum turntable.
                if (turntableTrack.uniqueID == "T-CS-1") continue;

                var prefab = pack.GetTurntableSignal(pack.EnableOldVersions && OldAreaCalculator.IsWithinOldArea(turntableTrack.visuals.position));

                if (prefab == null) continue;

                var track = turntableTrack.Track;
                var kpSet = track.GetKinkedPointSet();

                Create(0, TrackDirection.Out);
                Create(kpSet.points.Length - 1, TrackDirection.In);

                void Create(int index, TrackDirection direction)
                {
                    var point = kpSet.points[index];
                    var instance = InstantiateFromDef(prefab, point.position, direction.IsOut() ? point.forward : -point.forward, false, track);
                    var placement = new SignalPlacementInfo(track, direction, index, point.span);
                    var controller = new TurntableSignalController(instance, turntableTrack, direction, placement);

                    turntableSignals.Add(controller);
                    instance.transform.parent = turntableTrack.visuals;
                }
            }
        }

        public virtual int MergeSignals(SignalPack pack, Dictionary<Junction, JunctionSignalGroup> registry)
        {
            var toDelete = new Dictionary<TrackSignalController, HashSet<TrackSignalController>>();
            var toMerge = new Dictionary<TrackSignalController, HashSet<TrackSignalController>>();

            // Check for possible merge targets.
            foreach (var group in registry)
            {
                foreach (var signal in group.Value.AllControllers)
                {
                    signal.UpdateBlocks();

                    foreach (var block in signal.GetPotentialBlocks())
                    {
                        var next = block.NextController;

                        if (next is TrackSignalController track)
                        {
                            var reverse = IsSignalReverse(signal) && IsSignalReverse(track);
                            var source = reverse ? track : signal;
                            var target = reverse ? signal : track;

                            // Close enough to merge, signal types allow merging, and the signal that will be
                            // removed does not have a block that is too long.
                            if (block.Length <= ClosenessThreshold && CanBeMerged(source, target))
                            {
                                if (!reverse)
                                {
                                    next.UpdateBlocks();
                                    var nextBlock = next.GetLongestBlock();

                                    if (nextBlock != null && nextBlock.Length < LongTrackThreshold)
                                    {
                                        AddToMap(target, source);
                                    }
                                }
                                else
                                {
                                    AddToMap(target, source);
                                }
                            }
                        }
                    }
                }
            }

            // Remove any target for which any signal leading to it does not meet conditions.
            foreach (var group in registry)
            {
                foreach (var signal in group.Value.AllControllers)
                {
                    bool atLeastOneNext = false;

                    foreach (var block in signal.GetPotentialBlocks())
                    {
                        var next = block.NextController;
                        atLeastOneNext |= next != null;

                        if (next is TrackSignalController track)
                        {
                            var reverse = IsSignalReverse(signal) && IsSignalReverse(track);
                            var source = reverse ? track : signal;
                            var target = reverse ? signal : track;

                            // Fail if the target cannot be merged or the signal will be the result of a merge.
                            if (!CanBeMerged(source, target) || toMerge.ContainsKey(target))
                            {
                                // If reverse, don't check for the length, else fail for long tracks too.
                                if (reverse)
                                {
                                    RemoveFromMap(target);
                                }
                                else
                                {
                                    var longBlock = next.GetLongestBlock();

                                    // If reverse, don't check for the length, else fail for long tracks too.
                                    if (longBlock != null && longBlock.Length > LongTrackThreshold)
                                    {
                                        RemoveFromMap(target);
                                    }
                                }
                            }
                        }
                    }

                    // End of track signals are blocked from being merged.
                    if (!atLeastOneNext)
                    {
                        RemoveFromMap(signal);
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
                    else
                    {
                        Debug.LogError($"Remain signal {remain.Name} has no group, this shouldn't happen!");
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

            void RemoveFromMap(TrackSignalController signal)
            {
                if (!toDelete.TryGetValue(signal, out var set)) return;

                foreach (var item in set)
                {
                    if (toMerge.TryGetValue(item, out var set2))
                    {
                        set2.Remove(signal);
                    }
                }

                toDelete.Remove(signal);
            }

            static bool IsSignalReverse(TrackSignalController signal)
            {
                return signal.Group != null && signal.Group.ReverseJunctionSignal == signal;
            }
        }
    }
}
