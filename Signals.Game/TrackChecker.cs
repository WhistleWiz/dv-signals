using Signals.Common;
using Signals.Game.Curves;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Signals.Game
{
    /// <summary>
    /// Helper class with track utility functions.
    /// </summary>
    public static class TrackChecker
    {
        /// <summary>
        /// Contains information about the intersections of a track with others.
        /// </summary>
        public class TrackIntersectionPoints
        {
            private const float CheckHeight = 4.0f;
            private const float CheckRadius = 2.5f;
            private const int TrainLayerMask = 1 << 10;

            private static Collider[] s_testCache = new Collider[1];

            public RailTrack Track { get; private set; }
            public List<(RailTrack Track, Transform Position)> IntersectionPoints { get; private set; }

            public TrackIntersectionPoints(RailTrack track)
            {
                Track = track;
                IntersectionPoints = new List<(RailTrack Track, Transform Position)>();
            }

            /// <summary>
            /// Checks if there is a train over any intersection point.
            /// </summary>
            public bool TestIntersections()
            {
                foreach (var point in IntersectionPoints)
                {
                    if (Physics.OverlapCapsuleNonAlloc(point.Position.position, point.Position.position + Vector3.up * CheckHeight,
                        CheckRadius, s_testCache, TrainLayerMask) > 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Checks if there's any train on the connected tracks.
            /// </summary>
            public bool TestTracks()
            {
                foreach (var point in IntersectionPoints)
                {
                    if (point.Track.HasBogies())
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Returns <see langword="true"/> if there are any intersection points with <paramref name="track"/>.
            /// </summary>
            /// <param name="track">The other track.</param>
            public bool HasIntersectionWithTrack(RailTrack track)
            {
                return IntersectionPoints.Any(x  => x.Track == track);
            }
        }

        private const float Distance = 0.01f;
        private const float DefaultPrecision = 0.5f;

        private static Dictionary<RailTrack, TrackIntersectionPoints> s_intersectionMap = new Dictionary<RailTrack, TrackIntersectionPoints>();

        /// <summary>
        /// Called when the intersection map finishes building.
        /// </summary>
        public static Action<Dictionary<RailTrack, TrackIntersectionPoints>>? OnMapBuilt;

        /// <summary>
        /// Checks if a track is occupied by a train.
        /// </summary>
        /// <param name="track">The track to check.</param>
        /// <param name="check">The check behaviour if there are cached intersections for the track.</param>
        public static bool IsOccupied(RailTrack track, CrossingCheckMode check)
        {
            if (track.HasBogies())
            {
                return true;
            }

            if (!s_intersectionMap.TryGetValue(track, out var intersection))
            {
                return false;
            }

            return check switch
            {
                CrossingCheckMode.WholeTrack => intersection.TestTracks(),
                CrossingCheckMode.IntersectionOnly => intersection.TestIntersections(),
                _ => false,
            };
        }

        internal static void StartBuildingMap()
        {
            RailTrackRegistry.Instance.StartCoroutine(BuildRoutine());
        }

        private static System.Collections.IEnumerator BuildRoutine()
        {
            SignalsMod.Log($"Started building intersection map...");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            s_intersectionMap.Clear();
            var tracks = RailTrackRegistry.Instance.AllTracks;

            int length = tracks.Length;

            for (int i = 0; i < length; i++)
            {
                var track = tracks[i];
                s_intersectionMap.TryGetValue(track, out TrackIntersectionPoints? point);

                for (int j = i + 1; j < length; j++)
                {
                    var other = tracks[j];

                    // Don't intersect with itself.
                    // Don't intersect if the tracks connect to eachother normally.
                    // Don't intersect if the tracks are from the same junction.
                    if (track == other || AreTracksConnected(track, other) || AreTracksFromSameJunction(track, other)) continue;

                    // Skip if no intersection was detected.
                    if (!BezierHelper.Intersects(track.curve, other.curve, DefaultPrecision, out var intersection)) continue;

                    SignalsMod.LogVerbose($"Found intersection between track '{track.logicTrack.ID}' and '{other.logicTrack.ID}'");

                    if (point == null)
                    {
                        point = new TrackIntersectionPoints(track);
                        s_intersectionMap.Add(track, point);
                    }

                    if (!s_intersectionMap.TryGetValue(other, out TrackIntersectionPoints? otherPoint))
                    {
                        otherPoint = new TrackIntersectionPoints(other);
                        s_intersectionMap.Add(other, otherPoint);
                    }

                    var t = new GameObject("[Intersection Point]").transform;
                    t.parent = track.transform.parent;
                    t.position = intersection;

                    //var debug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //debug.transform.parent = t;
                    //debug.transform.localPosition = Vector3.zero;

                    point.IntersectionPoints.Add((other, t));
                    otherPoint.IntersectionPoints.Add((track, t));
                }
            }

            BezierHelper.Clear();

            sw.Stop();
            SignalsMod.Log($"Finished building intersection map with {s_intersectionMap.Count} entries ({sw.Elapsed.TotalSeconds:F4}s)");

            OnMapBuilt?.Invoke(s_intersectionMap);
            yield return null;
        }

        /// <summary>
        /// Calculates approximate intersections of a <see cref="RailTrack"/> with others.
        /// </summary>
        /// <param name="track">The <see cref="RailTrack"/> to check.</param>
        /// <param name="others">A collection of <see cref="RailTrack"/> to check against.</param>
        /// <param name="precision">The minimum precision allowed.</param>
        /// <returns>A <see cref="TrackIntersectionPoints"/> if there are any intersections, <see langword="null"/> otherwise.</returns>
        /// <remarks>
        /// This recursive method attempts to approximate intersections between the curves that make up the tracks.
        /// For more information check the <see cref="BezierHelper.Intersects(BezierCurve, BezierCurve, float, out Vector3)"/> method.
        /// </remarks>
        public static TrackIntersectionPoints? CalculateIntersections(RailTrack track, IEnumerable<RailTrack> others, float precision)
        {
            TrackIntersectionPoints? result = null;

            foreach (RailTrack other in others)
            {
                // Don't intersect with itself.
                // Don't intersect if the tracks connect to eachother normally.
                // Don't intersect if the tracks are from the same junction.
                if (track == other || AreTracksConnected(track, other) || AreTracksFromSameJunction(track, other)) continue;

                // Skip if no intersection was detected.
                if (!BezierHelper.Intersects(track.curve, other.curve, precision, out var intersection)) continue;

                SignalsMod.LogVerbose($"Found intersection between track '{track.logicTrack.ID}' and '{other.logicTrack.ID}'");

                result ??= new TrackIntersectionPoints(track);

                var t = new GameObject("[Intersection Point]").transform;
                t.parent = track.transform.parent;
                t.position = intersection;

                //var debug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //debug.transform.parent = t;
                //debug.transform.localPosition = Vector3.zero;

                result.IntersectionPoints.Add((other, t));
            }

            return result;
        }

        /// <summary>
        /// Returns <see langword="true"/> if 2 tracks are directly connected, otherwise <see langword="false"/>.
        /// </summary>
        public static bool AreTracksConnected(RailTrack r1, RailTrack r2)
        {
            return Helpers.DistanceSqr(r1.curve[0].position, r2.curve[0].position) < Distance ||
                Helpers.DistanceSqr(r1.curve[0].position, r2.curve[r2.curve.pointCount - 1].position) < Distance ||
                Helpers.DistanceSqr(r1.curve[r1.curve.pointCount - 1].position, r2.curve[0].position) < Distance ||
                Helpers.DistanceSqr(r1.curve[r1.curve.pointCount - 1].position, r2.curve[r2.curve.pointCount - 1].position) < Distance;
        }

        /// <summary>
        /// Returns <see langword="true"/> if 2 tracks are both out branches of the same junction, otherwise <see langword="false"/>.
        /// </summary>
        public static bool AreTracksFromSameJunction(RailTrack r1, RailTrack r2)
        {
            return r1.isJunctionTrack && r2.isJunctionTrack && r1.inJunction == r2.inJunction;
        }
    }
}
