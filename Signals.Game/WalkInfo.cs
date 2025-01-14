using Signals.Game.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game
{
    /// <summary>
    /// Class holding information about a track walk.
    /// </summary>
    public class WalkInfo
    {
        /// <summary>
        /// The tracks walked until the next signal.
        /// </summary>
        public RailTrack[] Tracks;
        /// <summary>
        /// The next mainline signal.
        /// </summary>
        public JunctionSignalController? NextMainlineSignal;
        /// <summary>
        /// The total track length walked, including the starting track.
        /// </summary>
        public float DistanceWalked;
        /// <summary>
        /// The total track length walked, excluding the starting track.
        /// </summary>
        public float DistanceWalkedWithoutStartingTrack;

        public WalkInfo(IEnumerable<RailTrack> tracks, JunctionSignalController? nextMainlineSignal)
        {
            Tracks = tracks.ToArray();
            NextMainlineSignal = nextMainlineSignal;

            if (Tracks.Length > 0)
            {
                DistanceWalked = 0;

                for (int i = 0; i < Tracks.Length; i++)
                {
                    DistanceWalked += (float)Tracks[i].logicTrack.length;
                }

                if (Tracks.Length > 1)
                {
                    DistanceWalkedWithoutStartingTrack = DistanceWalked - (float)Tracks[0].logicTrack.length;
                }
                else
                {
                    DistanceWalkedWithoutStartingTrack = 0;
                }
            }
            else
            {
                DistanceWalked = 0;
                DistanceWalkedWithoutStartingTrack = 0;
            }
        }
    }
}
