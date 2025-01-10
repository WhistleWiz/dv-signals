using System.Collections.Generic;
using System.Linq;

namespace Signals.Game
{
    /// <summary>
    /// Helper class to traverse tracks.
    /// </summary>
    public static class TrackWalker
    {
        public const int MaxDepth = 50;

        private static SignalPair? s_lastSignals = null;
        private static bool? s_lastDirection = null;

        private static void Clear()
        {
            s_lastSignals = null;
            s_lastDirection = null;
        }

        /// <summary>
        /// Returns all tracks after a signal until another signal is found.
        /// </summary>
        /// <param name="controller">The <see cref="SignalController"/> from where to start.</param>
        public static IEnumerable<RailTrack> WalkUntilNextSignal(SignalController controller)
        {
            return WalkUntilNextSignal(controller.AssignedJunction, controller.TowardsBranches);
        }

        /// <summary>
        /// Returns all tracks after a junction until a signal is found.
        /// </summary>
        /// <param name="from">The <see cref="Junction"/> where to start.</param>
        /// <param name="direction">The search direction. <see langword="true"/> for the outbound tracks, <see langword="false"/> for the inbound track.</param>
        /// <remarks>Uses the currently selected branch.</remarks>
        public static IEnumerable<RailTrack> WalkUntilNextSignal(Junction from, bool direction)
        {
            return WalkUntilNextSignal(from, direction, from.selectedBranch);
        }

        /// <summary>
        /// Returns all tracks after a junction until a signal is found.
        /// </summary>
        /// <param name="from">The <see cref="Junction"/> where to start.</param>
        /// <param name="direction">The search direction. <see langword="true"/> for the outbound tracks, <see langword="false"/> for the inbound track.</param>
        /// <param name="branch">The junction branch to follow.</param>
        /// <returns></returns>
        public static IEnumerable<RailTrack> WalkUntilNextSignal(Junction from, bool direction, int branch)
        {
            var track = direction ? from.outBranches[branch].track : from.inBranch.track;
            return WalkUntilNextSignal(track, JunctionTrackDirection(from, track));
        }

        /// <summary>
        /// Returns all tracks until a signal is found.
        /// </summary>
        /// <param name="from">The <see cref="RailTrack"/> where to start.</param>
        /// <param name="direction">The search direction. <see langword="true"/> for the outbound track, <see langword="false"/> for the inbound track.</param>
        /// <remarks>Includes the starting track in the output.</remarks>
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

        private static RailTrack? GetNextTrack(RailTrack current, bool direction) => direction
                ? current.outIsConnected ? current.GetOutBranch().track : null
                : current.inIsConnected ? current.GetInBranch().track : null;

        private static bool JunctionTrackDirection(Junction junction, RailTrack track)
        {
            return track.inJunction == junction;
        }

        /// <summary>
        /// Returns the first signal after the current one.
        /// </summary>
        /// <param name="controller">The current <see cref="SignalController"/>.</param>
        /// <returns>The first <see cref="SignalController"/> after this one, or <see langword="null"/> if none is found.</returns>
        public static SignalController? GetNextSignal(SignalController controller)
        {
            WalkUntilNextSignal(controller);
            return GetNextSignal();
        }

        /// <summary>
        /// Returns the first signal after this junction.
        /// </summary>
        /// <param name="from">The <see cref="Junction"/> where to start.</param>
        /// <param name="direction">The search direction. <see langword="true"/> for the outbound tracks, <see langword="false"/> for the inbound track.</param>
        /// <returns>The first <see cref="SignalController"/> after this one, or <see langword="null"/> if none is found.</returns>
        public static SignalController? GetNextSignal(Junction from, bool direction)
        {
            WalkUntilNextSignal(from, direction);
            return GetNextSignal();
        }

        /// <summary>
        /// Returns the first signal after this junction.
        /// </summary>
        /// <param name="from">The <see cref="Junction"/> where to start.</param>
        /// <param name="direction">The search direction. <see langword="true"/> for the outbound tracks, <see langword="false"/> for the inbound track.</param>
        /// <param name="branch">The junction branch to follow.</param>
        /// <returns></returns>
        public static SignalController? GetNextSignal(Junction from, bool direction, int branch)
        {
            WalkUntilNextSignal(from, direction, branch);
            return GetNextSignal();
        }

        /// <summary>
        /// Returns the first signal after this track.
        /// </summary>
        /// <param name="from">The <see cref="RailTrack"/> where to start.</param>
        /// <param name="direction">The search direction. <see langword="true"/> for the outbound track, <see langword="false"/> for the inbound track.</param>
        /// <returns>The first <see cref="SignalController"/> after this one, or <see langword="null"/> if none is found.</returns>
        public static SignalController? GetNextSignal(RailTrack from, bool direction)
        {
            WalkUntilNextSignal(from, direction);
            return GetNextSignal();
        }

        /// <summary>
        /// Returns the cached <see cref="SignalController"/> after a walk method has been called.
        /// </summary>
        /// <remarks>
        /// This method is used to avoid running a walk again if both the tracks and the signal are needed.
        /// <para>Value is cleared after every call to a walk method.</para>
        /// </remarks>
        public static SignalController? GetNextSignal()
        {
            if (!s_lastDirection.HasValue || s_lastSignals == null)
            {
                return null;
            }

            return s_lastSignals.GetSignal(s_lastDirection.Value);
        }

        public static void GetTracksAndNextSignal(SignalController from, out RailTrack[] tracks, out SignalController? nextSignal)
        {
            tracks = WalkUntilNextSignal(from).ToArray();
            nextSignal = GetNextSignal();
        }
    }
}
