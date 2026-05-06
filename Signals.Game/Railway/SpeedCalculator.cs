using Signals.Game.Curves;
using System.Collections.Generic;
using UnityEngine;

namespace Signals.Game.Railway
{
    public static class SpeedCalculator
    {
        private struct SpeedInfo
        {
            public float SpeedIn;
            public float SpeedOut;

            public SpeedInfo(float speedIn, float speedOut)
            {
                SpeedIn = speedIn;
                SpeedOut = speedOut;
            }

            // Flipped as when going towards out, the in side is counted.
            public readonly float GetSpeed(TrackDirection direction) => direction.IsOut() ? SpeedIn : SpeedOut;
        }

        // Max speed uses a radius of 1200m, so use a value slightly higher.
        private const float MinCurvature = 1.0f / 1500.0f;
        private const float YardMainSpeed = 50;
        private const float YardSpeed = 30;
        private const int RadiusOffset = 10;

        public const float EndDistance = 500;

        private static Dictionary<RailTrack, SpeedInfo> s_speeds = new Dictionary<RailTrack, SpeedInfo>();

        internal static void ClearCache()
        {
            s_speeds.Clear();
        }

        public static float GetSpeed(RailTrack track, TrackDirection direction)
        {
            if (s_speeds.TryGetValue(track, out var info))
            {
                return info.GetSpeed(direction);
            }

            if (track.curve.pointCount < 2) return 0;

            // Cap maximum speed for yards.
            var limit = track.IsPartOfStation() ? (track.GetId().IsGeneric() ? YardMainSpeed : YardSpeed) : float.PositiveInfinity;

            // Calculate radius.
            CubicBezier curve;
            float curvature;
            var radius = float.PositiveInfinity;
            var length = 0.0f;

            for (int i = 1; i < track.curve.pointCount; i++)
            {
                LoopLogic(i);

                if (length > EndDistance) break;
            }

            var radiusIn = radius + RadiusOffset;
            radius = float.PositiveInfinity;
            length = 0.0f;

            for (int i = track.curve.pointCount - 1; i > 0; i--)
            {
                LoopLogic(i);

                if (length > EndDistance) break;
            }

            var radiusOut = radius + RadiusOffset;
            return AddEntry(TrackUtils.GetSpeedLimit(radiusIn), TrackUtils.GetSpeedLimit(radiusOut)).GetSpeed(direction);

            void LoopLogic(int i)
            {
                curve = CubicBezier.FromBezierCurveBehind(track.curve, i);

                for (float f = 0.05f; f < 1; f += 0.30f)
                {
                    CurvatureLogic(f);
                }

                length += curve.EndsDistance();
            }

            void CurvatureLogic(float t)
            {
                curvature = BezierHelper.GetCurvature(curve, t);

                if (curvature < MinCurvature) return;

                radius = Mathf.Min(radius, 1.0f / curvature);
            }

            SpeedInfo AddEntry(float inSpeed, float outSpeed)
            {
                var info = new SpeedInfo(Mathf.Min(limit, inSpeed), Mathf.Min(limit, outSpeed));
                s_speeds[track] = info;
                return info;
            }
        }
    }
}
