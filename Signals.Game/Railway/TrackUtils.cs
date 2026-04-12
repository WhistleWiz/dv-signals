using Signals.Game.Util;
using System.Collections.Generic;
using System.Linq;

using JData = Junction.JunctionData;

namespace Signals.Game.Railway
{
    public static class TrackUtils
    {
        private const float PositionDistance = 0.01f;

        /// <summary>
        /// Returns <see langword="true"/> if 2 tracks are directly connected, otherwise <see langword="false"/>.
        /// </summary>
        public static bool AreTracksConnected(RailTrack r1, RailTrack r2)
        {
            var bi = r1.GetAllInBranches();
            var bo = r1.GetAllOutBranches();
            return bi != null && bi.Any(x => x.track == r2) || bo != null && bo.Any(x => x.track == r2);
        }

        /// <summary>
        /// Returns <see langword="true"/> if 2 tracks are directly connected, otherwise <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// Checks if the positions of the track ends match, instead of checking for logic connections.
        /// </remarks>
        public static bool AreTracksConnectedPosition(RailTrack r1, RailTrack r2)
        {
            return Helpers.DistanceSqr(r1.curve[0].position, r2.curve[0].position) < PositionDistance ||
                Helpers.DistanceSqr(r1.curve[0].position, r2.curve[r2.curve.pointCount - 1].position) < PositionDistance ||
                Helpers.DistanceSqr(r1.curve[r1.curve.pointCount - 1].position, r2.curve[0].position) < PositionDistance ||
                Helpers.DistanceSqr(r1.curve[r1.curve.pointCount - 1].position, r2.curve[r2.curve.pointCount - 1].position) < PositionDistance;
        }

        /// <summary>
        /// Returns <see langword="true"/> if 2 tracks are both out branches of the same junction, otherwise <see langword="false"/>.
        /// </summary>
        public static bool AreTracksFromSameJunction(RailTrack r1, RailTrack r2)
        {
            return r1.isJunctionTrack && r2.isJunctionTrack && r1.inJunction == r2.inJunction;
        }

        /// <summary>
        /// Finds the first station ID in a <see cref="RailTrack"/> collection.
        /// </summary>
        /// <param name="tracks">The tracks to check.</param>
        /// <returns>The first track valid result found, or <see cref="string.Empty"/> if no result is found.</returns>
        public static string NextStation(IEnumerable<RailTrack> tracks)
        {
            foreach (var track in tracks)
            {
                if (track.GetID().IsGeneric()) continue;

                var text = track.GetID().yardId;

                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Finds the first track yard in a <see cref="RailTrack"/> collection.
        /// </summary>
        /// <param name="tracks">The tracks to check.</param>
        /// <returns>The first track valid result found, or <see cref="string.Empty"/> if no result is found.</returns>
        public static string NextTrackYard(IEnumerable<RailTrack> tracks)
        {
            foreach (var track in tracks)
            {
                if (track.GetID().IsGeneric()) continue;

                var text = track.GetID().SignIDSubYardPart;

                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Finds the first track number in a <see cref="RailTrack"/> collection.
        /// </summary>
        /// <param name="tracks">The tracks to check.</param>
        /// <returns>The first track valid result found, or <see cref="string.Empty"/> if no result is found.</returns>
        public static string NextTrackNumber(IEnumerable<RailTrack> tracks)
        {
            foreach (var track in tracks)
            {
                var id = track.GetID();

                if (id.IsGeneric()) continue;

                var text = ReflectionHelpers.GetTrimmedOrderNumber(id);

                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Finds the first track number and type in a <see cref="RailTrack"/> collection.
        /// </summary>
        /// <param name="tracks">The tracks to check.</param>
        /// <returns>The first valid result found, or <see cref="string.Empty"/> if no result is found.</returns>
        public static string NextTrackType(IEnumerable<RailTrack> tracks)
        {
            foreach (var track in tracks)
            {
                var id = track.GetID();

                if (id.IsGeneric()) continue;

                var text = ReflectionHelpers.GetTrackType(id);

                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }

            return string.Empty;
        }

        public static string JunctionStation(Junction junction)
        {
            var id = junction.junctionData.junctionIdLong;

            if (!id.StartsWith(JData.ID_MARKER_STATION))
            {
                return string.Empty;
            }

            var split = id.Split('-');

            if (split.Length != 3)
            {
                return string.Empty;
            }

            return split[2];
        }

        public static TrackDirection TrackDirectionFromJunction(RailTrack track, Junction junction)
        {
            return track.inJunction == junction ? TrackDirection.Out : TrackDirection.In;
        }

        public static TrackDirection TrackDirectionFromTrack(RailTrack track, RailTrack from)
        {
            if (track.inIsConnected)
            {
                return track.GetInBranch().track == from ? TrackDirection.Out : TrackDirection.In;
            }

            if (track.outIsConnected)
            {
                return track.GetOutBranch().track == from ? TrackDirection.In : TrackDirection.Out;
            }

            return TrackDirection.Out;
        }

        public static double GetTotalLength(IEnumerable<RailTrack> tracks)
        {
            double length = 0.0;

            foreach (var track in tracks)
            {
                length += track.GetLength();
            }

            return length;
        }
    }
}
