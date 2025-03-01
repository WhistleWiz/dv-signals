using Signals.Game.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game
{
    /// <summary>
    /// Class holding information about a tracks.
    /// </summary>
    public class TrackInfo
    {
        private RailTrack[] _tracks;
        private float? _distanceWalked;
        private float? _distanceWalkedWithoutStartingTrack;
        private string? _nextTrackNumber;
        private string? _nextTrackNumberType;
        private string? _nextTrackYardNumberType;
        private string? _nextTrackYardNumber;
        private string? _nextTrackYard;

        public RailTrack[] Tracks => _tracks;
        /// <summary>
        /// The next mainline (non shunting) signal.
        /// </summary>
        public BasicSignalController? NextMainlineSignal { get; private set; }
        /// <summary>
        /// The next shunting signal.
        /// </summary>
        public BasicSignalController? NextShuntingSignal { get; private set; }
        /// <summary>
        /// The next junction.
        /// </summary>
        public Junction? NextJunction { get; private set; }

        /// <summary>
        /// The total track length walked, including the starting track.
        /// </summary>
        public float DistanceWalked
        {
            get
            {
                if (_distanceWalked == null)
                {
                    CalculateDistances();
                }

                return _distanceWalked!.Value;
            }
        }
        /// <summary>
        /// The total track length walked, excluding the starting track.
        /// </summary>
        public float DistanceWalkedWithoutStartingTrack
        {
            get
            {
                if (_distanceWalkedWithoutStartingTrack == null)
                {
                    CalculateDistances();
                }

                return _distanceWalkedWithoutStartingTrack!.Value;
            }
        }

        /// <summary>
        /// The next yard track's number. Empty if not available.
        /// </summary>
        public string NextTrackNumber
        {
            get
            {
                _nextTrackNumber ??= TrackUtils.NextTrackNumber(_tracks);

                return _nextTrackNumber;
            }
        }
        /// <summary>
        /// The next yard track's number and type. Empty if not available.
        /// </summary>
        public string NextTrackNumberType
        {
            get
            {
                _nextTrackNumberType ??= TrackUtils.NextTrackNumberType(_tracks);

                return _nextTrackNumberType;
            }
        }
        /// <summary>
        /// The next yard track's yard, number and type. Empty if not available.
        /// </summary>
        public string NextTrackYardNumberType
        {
            get
            {
                _nextTrackYardNumberType ??= TrackUtils.NextTrackYardNumberType(_tracks);

                return _nextTrackYardNumberType;
            }
        }
        /// <summary>
        /// The next yard track's yard and number. Empty if not available.
        /// </summary>
        public string NextTrackYardNumber
        {
            get
            {
                _nextTrackYardNumber ??= TrackUtils.NextTrackYardNumber(_tracks);

                return _nextTrackYardNumber;
            }
        }
        /// <summary>
        /// The next yard track's yard. Empty if not available.
        /// </summary>
        public string NextTrackYard
        {
            get
            {
                _nextTrackYard ??= TrackUtils.NextTrackYard(_tracks);

                return _nextTrackYard;
            }
        }

        /// <summary>
        /// Creates a complete WalkInfo.
        /// </summary>
        /// <param name="tracks"></param>
        /// <param name="nextMainlineSignal"></param>
        /// <param name="nextShuntingSignal"></param>
        public TrackInfo(IEnumerable<RailTrack>? tracks = null, BasicSignalController? nextMainlineSignal = null, BasicSignalController? nextShuntingSignal = null,
            Junction? nextJunction = null)
        {
            _tracks = tracks == null ? Array.Empty<RailTrack>() : tracks.ToArray();

            NextMainlineSignal = nextMainlineSignal;
            NextShuntingSignal = nextShuntingSignal;
            NextJunction = nextJunction;
        }

        private void CalculateDistances()
        {
            if (Tracks.Length > 0)
            {
                _distanceWalked = (float)Tracks[0].logicTrack.length;
                _distanceWalkedWithoutStartingTrack = 0.0f;

                for (int i = 0; i < Tracks.Length; i++)
                {
                    _distanceWalked += (float)Tracks[i].logicTrack.length;
                    _distanceWalkedWithoutStartingTrack = (float)Tracks[0].logicTrack.length;
                }
            }
            else
            {
                _distanceWalked = 0;
                _distanceWalkedWithoutStartingTrack = 0;
            }
        }

        public static TrackInfo NextSignalTrackInfo(BasicSignalController controller)
        {
            return new TrackInfo(nextMainlineSignal: controller);
        }

        public static TrackInfo NextSignalTrackInfo(BasicSignalController controller, Junction junction)
        {
            return new TrackInfo(nextMainlineSignal: controller, nextJunction: junction);
        }
    }
}
