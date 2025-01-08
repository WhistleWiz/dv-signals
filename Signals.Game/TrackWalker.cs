using System.Collections.Generic;

namespace Signals.Game
{
    public static class TrackWalker
    {
        public const int MaxDepth = 25;

        private static SignalPair? s_lastSignals = null;
        private static bool? s_lastDirection = null;

        private static void Clear()
        {
            s_lastSignals = null;
            s_lastDirection = null;
        }

        public static IEnumerable<RailTrack> WalkUntilNextSignal(SignalController controller)
        {
            return WalkUntilNextSignal(controller.AssignedJunction, controller.TowardsSplit);
        }

        public static IEnumerable<RailTrack> WalkUntilNextSignal(Junction from, bool direction)
        {
            var track = direction ? from.outBranches[from.selectedBranch].track : from.inBranch.track;
            return WalkUntilNextSignal(track, JunctionTrackDirection(from, track));
        }

        public static IEnumerable<RailTrack> WalkUntilNextSignal(RailTrack from, bool direction)
        {
            Clear();

            // If we start from null, stop.
            if (from == null) yield break;

            //SignalsMod.LogVerbose($"Starting walk from '{from.name}' ({from.logicTrack.ID})...");

            RailTrack? current = from;
            Junction? junction;
            int depth = 0;

            while (depth++ <= MaxDepth)
            {
                // Return the track we're on.
                yield return current;

                // Check if there's a junction at the exit for the next track.
                junction = direction ? current.outJunction : current.inJunction;

                // If there is a junction, stop looking if it has a signal. Store that and the direction.
                if (junction != null && SignalManager.Instance.TryGetSignals(junction, out s_lastSignals))
                {
                    s_lastDirection = current == junction.inBranch.track;
                    break;
                }

                // Store the current track as the previous one and get the possible next track.
                from = current;
                current = GetNextTrack(current, direction);

                // There are no more tracks, stop looking.
                if (current == null)
                {
                    break;
                }

                var branch = current.GetOutBranch();

                // Check if there's an out branch.
                if (branch != null)
                {
                    // If the out branch is not where we came from, then we want it.
                    direction = branch.track != from;
                }
                else
                {
                    // Now for the in branch.
                    branch = current.GetInBranch();

                    if (branch != null)
                    {
                        // If the in branch is where we came from, then we want the out branch.
                        direction = branch.track == from;
                    }
                    else
                    {
                        // Track is not connected anywhere, yeet.
                        // It should be impossible to reach this place so log easter egg it is.
                        SignalsMod.Log("Achievement get: how did we get here?");
                        yield break;
                    }
                }
            }
        }

        private static RailTrack? GetNextTrack(RailTrack current, bool nextIsOut) => nextIsOut
                ? current.outIsConnected ? current.GetOutBranch().track : null
                : current.inIsConnected ? current.GetInBranch().track : null;

        private static bool JunctionTrackDirection(Junction junction, RailTrack track)
        {
            return track.inJunction == junction;
        }

        public static SignalController? GetNextSignal(SignalController controller)
        {
            WalkUntilNextSignal(controller);
            return GetNextSignal();
        }

        public static SignalController? GetNextSignal(Junction from, bool direction)
        {
            WalkUntilNextSignal(from, direction);
            return GetNextSignal();
        }

        public static SignalController? GetNextSignal(RailTrack from, bool direction)
        {
            WalkUntilNextSignal(from, direction);
            return GetNextSignal();
        }

        private static SignalController? GetNextSignal()
        {
            if (!s_lastDirection.HasValue || s_lastSignals == null)
            {
                return null;
            }

            return s_lastDirection.Value ? s_lastSignals.To : s_lastSignals.From;
        }
    }
}
