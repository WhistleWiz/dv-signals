using Signals.Game.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;

using Branch = Junction.Branch;

namespace Signals.Game.Railway
{
    /// <summary>
    /// Helper class to traverse tracks.
    /// </summary>
    public static class TrackWalker
    {
        public class JunctionInfo
        {
            public Junction? Junction;
            public TrackDirection JunctionDirection;
        }

        public class ControllerInfo
        {
            public BasicSignalController? Signal;
            public TrackDirection SignalDirection;
            public bool IsDeadEnd;
            public bool IsSelfLoop;
        }

        public const int MaxDepth = 64;

        private static bool HasMainSignal(BasicSignalController controller)
        {
            return controller.Type != SignalType.Shunting &&
                controller.Type != SignalType.Spacing &&
                controller.GetControllerSignal() != null;
        }

        private static bool HasMainOrSpacingSignal(BasicSignalController controller)
        {
            return controller.Type != SignalType.Shunting &&
                controller.GetControllerSignal() != null;
        }

        private static bool Any(BasicSignalController controller) => controller.Signals.Length > 0 || controller.ShuntingSignals.Length > 0;

        private static bool ContainsTrack(RailTrack from, Branch branch, TrackDirection direction)
        {
            var nextBranch = direction.IsOut() ? branch.track.GetOutBranch() : branch.track.GetInBranch();

            // If the next visited track was going to be track we came from, we must go the other way.
            if (nextBranch != null && nextBranch.track == from)
            {
                return true;
            }

            var nextJuntion = direction.IsOut() ? branch.track.outJunction : branch.track.inJunction;

            // Check the junction if the branch was null.
            if (nextJuntion != null)
            {
                if (nextJuntion.inBranch != null && nextJuntion.inBranch.track == from)
                {
                    return true;
                }

                return nextJuntion.outBranches.Any(x => x.track == from);
            }

            return false;
        }

        public static bool GoesToDeadEnd(RailTrack track, TrackDirection direction)
        {
            int depth = 0;
            var visited = new HashSet<RailTrack>();
            var tracks = new List<RailTrack>();

            // Keep looping until a certain depth is reached, the track exists and the track has not been visited yet.
            while (depth++ < MaxDepth && track != null && !visited.Contains(track))
            {
                visited.Add(track);

                Junction? junction = direction.IsOut() ? track.outJunction : track.inJunction;
                Branch? branch;

                // Found junction, check if we're going into. If we are, it's not a dead end.
                if (junction != null && !track.isJunctionTrack)
                {
                    return false;
                }

                branch = direction.IsOut() ? track.GetOutBranch() : track.GetInBranch();

                // No branch means we reach a dead end.
                if (branch == null || branch.track == null)
                {
                    return true;
                }

                // Check if the current track is the next track of the next branch.
                if (ContainsTrack(track, branch, direction))
                {
                    // Direction must be flipped.
                    direction = direction.Flipped();
                }

                track = branch.track;
                tracks.Add(track);
            }

            return false;
        }

        public static bool RemainsInTheSameStation(RailTrack track, TrackDirection direction, string station)
        {
            int depth = 0;
            var visited = new HashSet<RailTrack>();
            Queue<Branch> branchQueue = new Queue<Branch>();

            // Keep looping until a certain depth is reached, the track exists and the track has not been visited yet.
            while (depth++ < MaxDepth && track != null && !visited.Contains(track))
            {
                visited.Add(track);

                Junction? junction = direction.IsOut() ? track.outJunction : track.inJunction;
                Branch? branch;

                // Found junction, check if we're going into. If we are, it's not a dead end.
                if (junction != null)
                {
                    var other = junction.GetStation();

                    if (string.IsNullOrEmpty(other) || station != other)
                    {
                        return false;
                    }
                }

                branch = direction.IsOut() ? track.GetOutBranch() : track.GetInBranch();

                // No branch means we reach a dead end.
                if (branch == null || branch.track == null)
                {
                    // If there's no possible branches, end early.
                    if (branchQueue.Count == 0) return true;

                    branch = branchQueue.Dequeue();
                    direction = TrackDirection.Out;
                }
                else
                {
                    // Add the other junction branches in case we just split.
                    if (branch.track.isJunctionTrack && junction == branch.track.inJunction)
                    {
                        foreach (var possibleBranch in junction.outBranches)
                        {
                            if (possibleBranch.track != branch.track)
                            {
                                branchQueue.Enqueue(possibleBranch);
                            }
                        }
                    }
                }

                // Check if the current track is the next track of the next branch.
                if (ContainsTrack(track, branch, direction))
                {
                    // Direction must be flipped.
                    direction = direction.Flipped();
                }

                track = branch.track;
            }

            return true;
        }

        public static List<TrackInfo> WalkTracks(RailTrack track, TrackDirection direction, int count)
        {
            int depth = 0;
            var visited = new HashSet<RailTrack>();
            var tracks = new List<TrackInfo>();

            // Keep looping until a certain depth is reached, the track exists and the track has not been visited yet.
            while (depth++ < count && track != null && !visited.Contains(track))
            {
                visited.Add(track);

                var branch = direction.IsOut() ? track.GetOutBranch() : track.GetInBranch();

                // No branch means we have no track to go, stop looping.
                if (branch == null || branch.track == null) break;

                // Check if the current track is the next track of the next branch.
                if (ContainsTrack(track, branch, direction))
                {
                    // Direction must be flipped.
                    direction = direction.Flipped();
                }

                track = branch.track;
                tracks.Add(new TrackInfo(track, direction));
            }

            return tracks;
        }

        public static List<TrackInfo> GetTracksUntilJunction(RailTrack track, TrackDirection direction, bool includeFinalBranchTracks, out JunctionInfo info)
        {
            int depth = 0;
            var visited = new HashSet<RailTrack>();
            var tracks = new List<TrackInfo>();
            info = new JunctionInfo();

            // Keep looping until a certain depth is reached, the track exists and the track has not been visited yet.
            while (depth++ < MaxDepth && track != null && !visited.Contains(track))
            {
                visited.Add(track);

                Junction? junction = direction.IsOut() ? track.outJunction : track.inJunction;
                Branch? branch;

                // Found junction, stop looping.
                if (junction != null)
                {
                    info.Junction = junction;
                    info.JunctionDirection = track.isJunctionTrack ? TrackDirection.In : TrackDirection.Out;
                    break;
                }

                branch = direction.IsOut() ? track.GetOutBranch() : track.GetInBranch();

                // No branch means we have no track to go, stop looping.
                if (branch == null || branch.track == null) break;

                // Check if the current track is the next track of the next branch.
                if (ContainsTrack(track, branch, direction))
                {
                    // Direction must be flipped.
                    direction = direction.Flipped();
                }

                track = branch.track;

                // If the track is a junction branch and branches are included,
                // stop looping before adding it to the list.
                if (!includeFinalBranchTracks && track.isJunctionTrack)
                {
                    info.Junction = track.inJunction;
                    info.JunctionDirection = TrackDirection.In;
                    break;
                }

                tracks.Add(new TrackInfo(track, direction));
            }

            return tracks;
        }

        public static List<TrackInfo> GetTracksUntilMainSignal(RailTrack track, TrackDirection direction, out ControllerInfo info)
        {
            return GetTracksUntilMainSignal(track, direction, null, out info);
        }

        public static List<TrackInfo> GetTracksUntilMainSignal(RailTrack track, TrackDirection direction,
            BasicSignalController? ignore, out ControllerInfo info)
        {
            return GetTracksUntilSignal(track, direction, ignore, HasMainSignal, out info);
        }

        public static List<TrackInfo> GetTracksUntilMainOrSpacingSignal(RailTrack track, TrackDirection direction,
            BasicSignalController? ignore, out ControllerInfo info)
        {
            return GetTracksUntilSignal(track, direction, ignore, HasMainOrSpacingSignal, out info);
        }

        public static List<TrackInfo> GetTracksUntilAnySignal(RailTrack track, TrackDirection direction,
            BasicSignalController? ignore, out ControllerInfo info)
        {
            return GetTracksUntilSignal(track, direction, ignore, Any, out info);
        }

        private static List<TrackInfo> GetTracksUntilSignal(RailTrack track, TrackDirection direction,
            BasicSignalController? ignore, Predicate<BasicSignalController> condition, out ControllerInfo info)
        {
            int depth = 0;
            var visited = new HashSet<RailTrack>();
            var tracks = new List<TrackInfo>();
            info = new ControllerInfo();

            // Keep looping until a certain depth is reached, the track exists and the track has not been visited yet.
            while (depth++ < MaxDepth && track != null)
            {
                // Looped back into tracks already visited, stop checking.
                if (visited.Contains(track))
                {
                    info.IsSelfLoop = true;
                    break;
                }

                visited.Add(track);

                Junction? junction = direction.IsOut() ? track.outJunction : track.inJunction;
                Branch? branch;

                // Found junction, check signal.
                if (junction != null && SignalManager.Instance.TryGetJunctionGroup(junction, out var group))
                {
                    if (track.isJunctionTrack)
                    {
                        var found = group.ReverseJunctionSignal;

                        if (MeetsConditions(found))
                        {
                            info.Signal = found;
                            info.SignalDirection = TrackDirection.In;
                            break;
                        }
                    }
                    else
                    {
                        var found = group.JunctionSignal;

                        if (MeetsConditions(found))
                        {
                            info.Signal = found;
                            info.SignalDirection = TrackDirection.Out;
                            break;
                        }
                    }
                }

                branch = direction.IsOut() ? track.GetOutBranch() : track.GetInBranch();

                // No branch means we have no track to go, stop looping.
                if (branch == null || branch.track == null)
                {
                    info.IsDeadEnd = true;
                    break;
                }

                // Check if the current track is the next track of the next branch.
                if (ContainsTrack(track, branch, direction))
                {
                    // Direction must be flipped.
                    direction = direction.Flipped();
                }

                track = branch.track;

                // If the next track is a junction branch, check if there's a signal there before actually going into it.
                if (track.isJunctionTrack && SignalManager.Instance.TryGetJunctionGroup(track.inJunction, out group) &&
                    !direction.IsOut() && group.TryGetControllerForTrack(track, out var match) && MeetsConditions(match))
                {
                    info.Signal = match;
                    info.SignalDirection = TrackDirection.In;
                    break;
                }

                tracks.Add(new TrackInfo(track, direction));
            }

            return tracks;

            bool MeetsConditions(BasicSignalController? controller)
            {
                return controller != null && controller != ignore && condition(controller);
            }
        }

        public static HashSet<BasicSignalController> GetAllPossibleMainControllers(RailTrack track, TrackDirection direction,
            BasicSignalController? ignore)
        {
            return GetAllPossibleControllers(track, direction, ignore, HasMainSignal);
        }

        private static HashSet<BasicSignalController> GetAllPossibleControllers(RailTrack track, TrackDirection direction,
            BasicSignalController? ignore, Predicate<BasicSignalController> condition)
        {
            int depth = 0;
            var visited = new HashSet<RailTrack>();
            var tracks = new Queue<RailTrack>();
            var controllers = new HashSet<BasicSignalController>();

            tracks.Enqueue(track);

            while (tracks.Count > 0)
            {
                track = tracks.Dequeue();

                // Keep looping until a certain depth is reached, the track exists and the track has not been visited yet.
                while (depth++ < MaxDepth && track != null && !visited.Contains(track))
                {
                    visited.Add(track);

                    Junction? junction = direction.IsOut() ? track.outJunction : track.inJunction;
                    Branch? branch = direction.IsOut() ? track.GetOutBranch() : track.GetInBranch();

                    // Found junction, check signal.
                    if (junction != null && SignalManager.Instance.TryGetJunctionGroup(junction, out var group))
                    {
                        if (track.isJunctionTrack)
                        {
                            var found = group.ReverseJunctionSignal;

                            if (MeetsConditions(found))
                            {
                                controllers.Add(found!);
                                break;
                            }
                        }
                        else
                        {
                            var found = group.JunctionSignal;

                            if (MeetsConditions(found))
                            {
                                controllers.Add(found!);
                                break;
                            }

                            foreach (var outBranch in junction.outBranches)
                            {
                                if (outBranch.track == branch.track) continue;

                                tracks.Enqueue(outBranch.track);
                            }
                        }
                    }


                    // No branch means we have no track to go, stop looping.
                    if (branch == null || branch.track == null) break;

                    // Check if the current track is the next track of the next branch.
                    if (ContainsTrack(track, branch, direction))
                    {
                        // Direction must be flipped.
                        direction = direction.Flipped();
                    }

                    track = branch.track;

                    // If the next track is a junction branch, check if there's a signal there before actually going into it.
                    if (track.isJunctionTrack && SignalManager.Instance.TryGetJunctionGroup(track.inJunction, out group) &&
                        !direction.IsOut() && group.TryGetControllerForTrack(track, out var match) && MeetsConditions(match))
                    {
                        controllers.Add(match!);
                        break;
                    }
                }

                // All queued tracks are junction branches, so the direction is always Out.
                direction = TrackDirection.Out;
            }

            return controllers;

            bool MeetsConditions(BasicSignalController? controller)
            {
                return controller != null && controller != ignore && condition(controller);
            }
        }

        public static Junction? GetNextJunction(RailTrack track, TrackDirection direction)
        {
            int depth = 0;
            var visited = new HashSet<RailTrack>();
            var tracks = new List<RailTrack>();

            // Keep looping until a certain depth is reached, the track exists and the track has not been visited yet.
            while (depth++ < MaxDepth && track != null && !visited.Contains(track))
            {
                visited.Add(track);

                Junction? junction = direction.IsOut() ? track.outJunction : track.inJunction;
                Branch? branch;

                // Found junction, return it. Track it comes from doesn't matter.
                if (junction != null)
                {
                    return junction;
                }

                branch = direction.IsOut() ? track.GetOutBranch() : track.GetInBranch();

                // No branch means we reach a dead end.
                if (branch == null || branch.track == null)
                {
                    return null;
                }

                // Check if the current track is the next track of the next branch.
                if (ContainsTrack(track, branch, direction))
                {
                    // Direction must be flipped.
                    direction = direction.Flipped();
                }

                track = branch.track;
                tracks.Add(track);
            }

            return null;
        }

        public static Junction? GetNextJunctionDivergingOnly(RailTrack track, TrackDirection direction)
        {
            int depth = 0;
            var visited = new HashSet<RailTrack>();
            var tracks = new List<RailTrack>();

            // Keep looping until a certain depth is reached, the track exists and the track has not been visited yet.
            while (depth++ < MaxDepth && track != null && !visited.Contains(track))
            {
                visited.Add(track);

                Junction? junction = direction.IsOut() ? track.outJunction : track.inJunction;
                Branch? branch;

                // Found junction, return it if we're facing the branches.
                if (junction != null && !track.isJunctionTrack)
                {
                    return junction;
                }

                branch = direction.IsOut() ? track.GetOutBranch() : track.GetInBranch();

                // No branch means we reach a dead end.
                if (branch == null || branch.track == null)
                {
                    return null;
                }

                // Check if the current track is the next track of the next branch.
                if (ContainsTrack(track, branch, direction))
                {
                    // Direction must be flipped.
                    direction = direction.Flipped();
                }

                track = branch.track;
                tracks.Add(track);
            }

            return null;
        }

        public static HashSet<RailTrack> GetReachableTracks(RailTrack track, TrackDirection direction, IEnumerable<RailTrack> targets)
        {
            int depth = 0;
            int targetCount = targets.Count();
            var targetsMet = new HashSet<RailTrack>();

            if (targetCount == 0) return targetsMet;

            var visited = new HashSet<RailTrack>();
            var tracks = new Queue<RailTrack>();

            tracks.Enqueue(track);

            while (tracks.Count > 0)
            {
                track = tracks.Dequeue();

                // Keep looping until a certain depth is reached, the track exists and the track has not been visited yet.
                while (depth++ < MaxDepth && track != null && !visited.Contains(track))
                {
                    visited.Add(track);

                    Junction? junction = direction.IsOut() ? track.outJunction : track.inJunction;
                    Branch? branch = direction.IsOut() ? track.GetOutBranch() : track.GetInBranch();

                    // Found junction, check signal.
                    if (junction != null && !track.isJunctionTrack)
                    {
                        foreach (var outBranch in junction.outBranches)
                        {
                            if (outBranch.track == branch.track) continue;

                            tracks.Enqueue(outBranch.track);
                        }
                    }

                    // No branch means we have no track to go, stop looping.
                    if (branch == null || branch.track == null) break;

                    // Check if the current track is the next track of the next branch.
                    if (ContainsTrack(track, branch, direction))
                    {
                        // Direction must be flipped.
                        direction = direction.Flipped();
                    }

                    track = branch.track;

                    if (targets.Contains(track))
                    {
                        targetsMet.Add(track);
                        break;
                    }
                }

                // All queued tracks are junction branches, so the direction is always Out.
                direction = TrackDirection.Out;

                // Stop looping if we already reached all tracks.
                if (targetsMet.Count == targetCount) break;
            }

            return targetsMet;
        }
    }
}
