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
                if (track.GetId().IsGeneric()) continue;

                var text = track.GetId().yardId;

                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Finds the first station ID in a <see cref="TrackInfo"/> collection.
        /// </summary>
        /// <param name="tracks">The tracks to check.</param>
        /// <returns>The first track valid result found, or <see cref="string.Empty"/> if no result is found.</returns>
        public static string NextStation(IEnumerable<TrackInfo> tracks)
        {
            foreach (var track in tracks)
            {
                if (track.GetId().IsGeneric()) continue;

                var text = track.GetId().yardId;

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
                if (track.GetId().IsGeneric()) continue;

                var text = track.GetId().SignIDSubYardPart;

                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Finds the first track yard in a <see cref="TrackInfo"/> collection.
        /// </summary>
        /// <param name="tracks">The tracks to check.</param>
        /// <returns>The first track valid result found, or <see cref="string.Empty"/> if no result is found.</returns>
        public static string NextTrackYard(IEnumerable<TrackInfo> tracks)
        {
            foreach (var track in tracks)
            {
                if (track.GetId().IsGeneric()) continue;

                var text = track.GetId().SignIDSubYardPart;

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
                var id = track.GetId();

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
        /// Finds the first track number in a <see cref="TrackInfo"/> collection.
        /// </summary>
        /// <param name="tracks">The tracks to check.</param>
        /// <returns>The first track valid result found, or <see cref="string.Empty"/> if no result is found.</returns>
        public static string NextTrackNumber(IEnumerable<TrackInfo> tracks)
        {
            foreach (var track in tracks)
            {
                var id = track.GetId();

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
                var id = track.GetId();

                if (id.IsGeneric()) continue;

                var text = ReflectionHelpers.GetTrackType(id);

                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Finds the first track number and type in a <see cref="TrackInfo"/> collection.
        /// </summary>
        /// <param name="tracks">The tracks to check.</param>
        /// <returns>The first valid result found, or <see cref="string.Empty"/> if no result is found.</returns>
        public static string NextTrackType(IEnumerable<TrackInfo> tracks)
        {
            foreach (var track in tracks)
            {
                var id = track.GetId();

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

        public static double GetTotalLength(IEnumerable<TrackInfo> tracks)
        {
            double length = 0.0;

            foreach (var track in tracks)
            {
                length += track.Length;
            }

            return length;
        }

        public static float GetSpeedLimit(float radius)
        {
            if (radius < 50f) return 10f;
            if (radius < 70f) return 20f;
            if (radius < 95f) return 30f;
            if (radius < 130f) return 40f;
            if (radius < 170f) return 50f;
            if (radius < 230f) return 60f;
            if (radius < 360f) return 70f;
            if (radius < 700f) return 80f;
            if (radius < 900f) return 90f;
            if (radius < 1200f) return 100f;
            return 120f;
        }
    }
}
