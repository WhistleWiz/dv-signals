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
        private RailTrack[] _tracks;
        private JunctionSignalController? _nextMainlineSignal;
        private JunctionSignalController? _nextShuntingSignal;
        private float? _distanceWalked;
        private float? _distanceWalkedWithoutStartingTrack;
        private string? _nextYardTrackNumber;
        private string? _nextYardTrackSign;

        /// <summary>
        /// The tracks walked until the next signal.
        /// </summary>
        public RailTrack[] Tracks => _tracks;
        /// <summary>
        /// The next mainline (non shunting) signal.
        /// </summary>
        public JunctionSignalController? NextMainlineSignal => _nextMainlineSignal;
        /// <summary>
        /// The next shunting signal.
        /// </summary>
        public JunctionSignalController? NextShuntingSignal => _nextShuntingSignal;
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
        /// The next yard track number. Is <see langword="null"/> if there is no yard track until the next signal.
        /// </summary>
        public string NextYardTrackNumber
        {
            get
            {
                if (_nextYardTrackNumber == null)
                {
                    CalculateTrackNumber();
                }

                return _nextYardTrackNumber!;
            }
        }
        /// <summary>
        /// The next yard track ID. Is <see langword="null"/> if there is no yard track until the next signal.
        /// </summary>
        public string NextYardTrackSign
        {
            get
            {
                if (_nextYardTrackSign == null)
                {
                    CalculateNextTrackSign();
                }

                return _nextYardTrackSign!;
            }
        }

        public WalkInfo(IEnumerable<RailTrack> tracks, JunctionSignalController? nextMainlineSignal, JunctionSignalController? nextShuntingSignal)
        {
            _tracks = tracks.ToArray();
            _nextMainlineSignal = nextMainlineSignal;
            _nextShuntingSignal = nextShuntingSignal;
        }

        private void CalculateDistances()
        {
            if (Tracks.Length > 0)
            {
                _distanceWalked = 0;

                for (int i = 0; i < Tracks.Length; i++)
                {
                    _distanceWalked += (float)Tracks[i].logicTrack.length;
                }

                if (Tracks.Length > 1)
                {
                    _distanceWalkedWithoutStartingTrack = DistanceWalked - (float)Tracks[0].logicTrack.length;
                }
                else
                {
                    _distanceWalkedWithoutStartingTrack = 0;
                }
            }
            else
            {
                _distanceWalked = 0;
                _distanceWalkedWithoutStartingTrack = 0;
            }
        }

        private void CalculateTrackNumber()
        {
            foreach (var track in _tracks)
            {
                var number = ReflectionHelpers.GetTrimmedOrderNumber(track.logicTrack.ID);

                if (!string.IsNullOrEmpty(number))
                {
                    _nextYardTrackNumber = number;
                    return;
                }
            }

            _nextYardTrackNumber = string.Empty;
        }

        private void CalculateNextTrackSign()
        {
            foreach (var track in _tracks)
            {
                if (!string.IsNullOrEmpty(track.logicTrack.ID.SignIDTrackPart))
                {
                    _nextYardTrackSign = track.logicTrack.ID.SignIDTrackPart;
                    return;
                }
            }

            _nextYardTrackSign = string.Empty;
        }
    }
}
