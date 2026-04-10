using Signals.Common;
using Signals.Game.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game
{
    public class TrackBlock
    {
        private static int s_idGen = 0;
        private static object s_lock = new object();

        private static int GetGenId()
        {
            int value;

            lock (s_lock)
            {
                value = s_idGen++;
            }

            return value;
        }

        private string? _trackNumber;
        private string? _trackType;
        private string? _yard;
        private string? _station;

        public readonly int Id;
        public RailTrack[] Tracks { get; private set; }
        public RailTrack[] ExtraTracks { get; private set; }
        public BasicSignalController? NextSignal { get; private set; }
        public float Length { get; private set; }
        /// <summary>
        /// Station covered by this block. Empty if not available.
        /// </summary>
        public string Station
        {
            get
            {
                _station ??= TrackUtils.NextStation(Tracks);

                return _station;
            }
        }
        /// <summary>
        /// Yard covered by this block. Empty if not available.
        /// </summary>
        public string Yard
        {
            get
            {
                _yard ??= TrackUtils.NextTrackYard(Tracks);

                return _yard;
            }
        }
        /// <summary>
        /// Track number covered by this block. Empty if not available.
        /// </summary>
        public string TrackNumber
        {
            get
            {
                _trackNumber ??= TrackUtils.NextTrackNumber(Tracks);

                return _trackNumber;
            }
        }
        /// <summary>
        /// Track type covered by this block. Empty if not available.
        /// </summary>
        public string TrackType
        {
            get
            {
                _trackType ??= TrackUtils.NextTrackType(Tracks);

                return _trackType;
            }
        }

        private TrackBlock(IEnumerable<RailTrack> tracks, BasicSignalController? nextSignal)
        {
            Id = GetGenId();
            Tracks = tracks.ToArray();
            NextSignal = nextSignal;

            // Add all junction tracks to better handle diverging trains.
            ExtraTracks = tracks.Where(x => x.isJunctionTrack).SelectMany(x => x.inJunction.GetAllTracks()).ToArray();
            Length = (float)TrackUtils.GetTotalLength(Tracks);
        }

        private TrackBlock(BasicSignalController? nextSignal, float distance)
        {
            Id = GetGenId();
            Tracks = System.Array.Empty<RailTrack>();
            ExtraTracks = System.Array.Empty<RailTrack>();
            NextSignal = nextSignal;
            Length = distance;
        }

        public bool IsOccupied(CrossingCheckMode crossingMode)
        {
            return Tracks.Any(x => x.IsOccupied(crossingMode)) || ExtraTracks.Any(x => x.IsOccupied(crossingMode));
        }

        public static TrackBlock CreateUntilSignal(RailTrack starting, TrackDirection direction, bool includeShunting, BasicSignalController? ignore = null)
        {
            var tracks = new List<RailTrack> { starting };
            tracks.AddRange(TrackWalker.GetTracksUntilSignal(starting, direction, includeShunting, ignore, out var info));

            return new TrackBlock(tracks, info.Signal);
        }

        public static TrackBlock CreateForDistant(BasicSignalController next, float distance)
        {
            return new TrackBlock(next, distance);
        }

        public static TrackBlock CreateForShunting(RailTrack track)
        {
            return new TrackBlock(new[] { track }, null);
        }
    }
}
