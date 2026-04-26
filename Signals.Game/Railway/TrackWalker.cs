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
        }

        public const int MaxDepth = 64;

        private static bool HasMainSignal(BasicSignalController controller)
        {
            return controller.GetControllerSignal() != null;
        }

        private static bool HasShuntingSignal(BasicSignalController controller)
        {
            return controller.ShuntingSignal != null;
        }

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

        public static List<RailTrack> GetTracksUntilJunction(RailTrack track, TrackDirection direction, bool includeFinalBranchTracks, out JunctionInfo info)
        {
            int depth = 0;
            HashSet<RailTrack> visited = new HashSet<RailTrack>();
            List<RailTrack> tracks = new List<RailTrack>();
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

                tracks.Add(track);
            }

            return tracks;
        }

        public static List<RailTrack> GetTracksUntilMainSignal(RailTrack track, TrackDirection direction, out ControllerInfo info)
        {
            return GetTracksUntilMainSignal(track, direction, null, out info);
        }

        public static List<RailTrack> GetTracksUntilMainSignal(RailTrack track, TrackDirection direction,
            BasicSignalController? ignore, out ControllerInfo info)
        {
            return GetTracksUntilSignal(track, direction, ignore, HasMainSignal, out info);
        }

        private static List<RailTrack> GetTracksUntilSignal(RailTrack track, TrackDirection direction,
            BasicSignalController? ignore, Predicate<BasicSignalController> condition, out ControllerInfo info)
        {
            int depth = 0;
            HashSet<RailTrack> visited = new HashSet<RailTrack>();
            List<RailTrack> tracks = new List<RailTrack>();
            info = new ControllerInfo();

            // Keep looping until a certain depth is reached, the track exists and the track has not been visited yet.
            while (depth++ < MaxDepth && track != null && !visited.Contains(track))
            {
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
                    info.Signal = match;
                    info.SignalDirection = TrackDirection.In;
                    break;
                }

                tracks.Add(track);
            }

            return tracks;

            bool MeetsConditions(BasicSignalController? controller)
            {
                return controller != null && controller != ignore && condition(controller);
            }
        }
    }
}
