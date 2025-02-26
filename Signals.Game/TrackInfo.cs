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
        private string? _nextYardTrackNumber;
        private string? _nextYardTrackSign;

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

        private void CalculateTrackNumber()
        {
            foreach (var track in _tracks)
            {
                if (track.logicTrack.ID.IsGeneric()) continue;

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
                if (track.logicTrack.ID.IsGeneric()) continue;

                if (!string.IsNullOrEmpty(track.logicTrack.ID.SignIDTrackPart))
                {
                    _nextYardTrackSign = track.logicTrack.ID.SignIDTrackPart;
                    return;
                }
            }

            _nextYardTrackSign = string.Empty;
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
