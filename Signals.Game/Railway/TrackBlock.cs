using Signals.Common;
using Signals.Game.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game.Railway
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

        private bool _tracksCanChange;
        private bool _dirty;
        private string? _station;
        private string? _yard;
        private string? _trackNumber;
        private string? _trackTrimmedNumber;
        private string? _trackType;
        private HashSet<RailTrack>? _tracks;
        private BasicSignalController? _nextController;
        private Dictionary<Junction, byte> _junctionStates;

        public readonly int Id;
        public TrackInfo[] Tracks { get; private set; }
        public RailTrack[] ExtraTracks { get; private set; }
        public BasicSignalController? NextController
        {
            get
            {
                if (_nextController != null && !_nextController.SafetyCheck())
                {
                    FlagAsDirty();
                    _nextController = null;
                }

                return _nextController;
            }

            private set => _nextController = value;
        }
        public float Length { get; private set; }
        public bool IsDeadEnd { get; private set; }
        public bool IsSelfLoop { get; private set; }
        public bool ShouldBeUpdated => _dirty || JunctionsChanged();
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
        /// Track number with trimmed leading zeros covered by this block. Empty if not available.
        /// </summary>
        public string TrackTrimmerNumber
        {
            get
            {
                _trackTrimmedNumber ??= TrackUtils.NextTrackTrimmedNumber(Tracks);

                return _trackTrimmedNumber;
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

        /// <summary>
        /// Includes both <see cref="Tracks"/> and <see cref="ExtraTracks"/>, without duplicates.
        /// </summary>
        public HashSet<RailTrack> AllTracks
        {
            get
            {
                if (_tracks == null)
                {
                    _tracks = Tracks.Select(x => x.Track).ToHashSet();
                    _tracks.UnionWith(ExtraTracks);
                }

                return _tracks;
            }
        }

        private TrackBlock(IEnumerable<TrackInfo> tracks, BasicSignalController? nextSignal)
        {
            Id = GetGenId();
            Tracks = tracks.ToArray();
            NextController = nextSignal;

            // Add all junction tracks to better handle diverging trains.
            ExtraTracks = tracks.Where(x => x.IsJunctionTrack).SelectMany(x => x.Track.inJunction.GetAllTracks()).ToArray();
            Length = (float)TrackUtils.GetTotalLength(Tracks);

            _junctionStates = new Dictionary<Junction, byte>();
            CheckIfTracksCanChange();
            BuildJunctionCache();
        }

        private TrackBlock(BasicSignalController? nextSignal, float distance)
        {
            Id = GetGenId();
            Tracks = System.Array.Empty<TrackInfo>();
            ExtraTracks = System.Array.Empty<RailTrack>();
            NextController = nextSignal;
            Length = distance;

            _junctionStates = new Dictionary<Junction, byte>();
            CheckIfTracksCanChange();
            BuildJunctionCache();
        }

        private void CheckIfTracksCanChange()
        {
            // Skip the first track, because if it is a junction signal, it'll update the blocks when the switch is changed.
            for (int i = 1; i < Tracks.Length; i++)
            {
                var track = Tracks[i];

                if (track.IsJunctionTrack && track.Direction.IsOut())
                {
                    _tracksCanChange = true;
                    return;
                }
            }

            _tracksCanChange = false;
        }

        private void BuildJunctionCache()
        {
            for (int i = 0; i < Tracks.Length; i++)
            {
                var track = Tracks[i];

                if (track.IsJunctionTrack && track.Direction.IsOut())
                {
                    var junction = track.Track.inJunction;

                    if (_junctionStates.ContainsKey(junction)) continue;

                    _junctionStates[junction] = junction.selectedBranch;
                }
            }
        }

        private bool JunctionsChanged()
        {
            if (!_tracksCanChange) return false;

            foreach (var item in _junctionStates)
            {
                if (item.Key.selectedBranch != item.Value) return true;
            }

            return false;
        }

        public bool IsOccupied(CrossingCheckMode crossingMode)
        {
            return Tracks.Any(x => x.Track.IsOccupied(crossingMode)) || ExtraTracks.Any(x => x.IsOccupied(crossingMode));
        }

        public void FlagAsDirty()
        {
            _dirty = true;
        }

        /// <summary>
        /// Creates a <see cref="TrackBlock"/> covering all tracks until a main signal.
        /// </summary>
        /// <param name="starting">The starting track.</param>
        /// <param name="direction">The direction of the starting track.</param>
        /// <param name="ignore">Optional controller to ignore.</param>
        public static TrackBlock CreateUntilMainSignal(RailTrack starting, TrackDirection direction, BasicSignalController? ignore = null)
        {
            var tracks = new List<TrackInfo> { new TrackInfo(starting, direction) };
            tracks.AddRange(TrackWalker.GetTracksUntilMainSignal(starting, direction, ignore, out var info));

            return new TrackBlock(tracks, info.Signal) { IsDeadEnd = info.IsDeadEnd, IsSelfLoop = info.IsSelfLoop };
        }

        /// <summary>
        /// Creates a <see cref="TrackBlock"/> for a distant signal.
        /// </summary>
        /// <param name="home">The home signal.</param>
        /// <param name="distance">The distance to the home signal.</param>
        /// <returns></returns>
        public static TrackBlock CreateForDistant(BasicSignalController home, float distance)
        {
            return new TrackBlock(home, distance);
        }

        /// <summary>
        /// Creates a <see cref="TrackBlock"/> covering all tracks until a shunting signal.
        /// </summary>
        /// <param name="starting">The starting track.</param>
        /// <param name="direction">The direction of the starting track.</param>
        /// <param name="ignore">Optional controller to ignore.</param>
        public static TrackBlock CreateForShunting(RailTrack starting, TrackDirection direction, BasicSignalController? ignore = null)
        {
            var tracks = new List<TrackInfo> { new TrackInfo(starting, direction) };
            tracks.AddRange(TrackWalker.GetTracksUntilAnySignal(starting, direction, ignore, out var info));

            return new TrackBlock(tracks, info.Signal) { IsDeadEnd = info.IsDeadEnd, IsSelfLoop = info.IsSelfLoop };
        }

        /// <summary>
        /// Creates a <see cref="TrackBlock"/> covering all tracks until a spacing or main signal.
        /// </summary>
        /// <param name="starting"></param>
        /// <param name="direction"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static TrackBlock CreateForSpacing(RailTrack starting, TrackDirection direction, BasicSignalController? ignore = null)
        {
            var tracks = new List<TrackInfo> { new TrackInfo(starting, direction) };
            tracks.AddRange(TrackWalker.GetTracksUntilMainOrSpacingSignal(starting, direction, ignore, out var info));

            return new TrackBlock(tracks, info.Signal) { IsDeadEnd = info.IsDeadEnd, IsSelfLoop = info.IsSelfLoop };
        }

        // Honestly unsure if this is more optimised than not doing the check and replacing the block every time.
        // After all, the blocks are still calculated...
        public static bool TracksMatch(TrackBlock? a, TrackBlock? b)
        {
            if (a == null || b == null) return false;

            if (!a.AllTracks.SetEquals(b.AllTracks)) return false;

            if (a.ExtraTracks.Length != b.ExtraTracks.Length) return false;

            if (a.Tracks.Length != b.Tracks.Length) return false;

            for (int i = 0; i < a.ExtraTracks.Length; i++)
            {
                if (a.ExtraTracks[i] != b.ExtraTracks[i]) return false;
            }

            for (int i = 0; i < a.Tracks.Length; i++)
            {
                if (a.Tracks[i] != b.Tracks[i]) return false;
            }

            return true;
        }
    }
}
