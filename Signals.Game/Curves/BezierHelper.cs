using System.Collections.Generic;
using UnityEngine;

namespace Signals.Game.Curves
{
    public static class BezierHelper
    {
        private const float AproxStep = 0.02f;

        private static Dictionary<BezierCurve, Bounds> s_boundCache = new Dictionary<BezierCurve, Bounds>();
        private static Vector3 s_misalignedFix = Vector3.up * 0.5f;

        internal static void Clear()
        {
            s_boundCache.Clear();
        }

        public static bool Intersects(BezierCurve c1, BezierCurve c2, float precision, out Vector3 result)
        {
            // Get or create the big bounding boxes for each curve.
            if (!s_boundCache.TryGetValue(c1, out var b1))
            {
                var list = new List<Vector3> { c1[0].position };

                for (int i = 1; i < c1.pointCount; i++)
                {
                    list.Add(c1[i - 1].globalHandle2);
                    list.Add(c1[i].globalHandle1);
                    list.Add(c1[i].position);
                }

                b1 = Helpers.AABBFromCollection(list);
                s_boundCache.Add(c1, b1);
            }

            if (!s_boundCache.TryGetValue(c2, out var b2))
            {
                var list = new List<Vector3> { c2[0].position };

                for (int i = 1; i < c2.pointCount; i++)
                {
                    list.Add(c2[i - 1].globalHandle2);
                    list.Add(c2[i].globalHandle1);
                    list.Add(c2[i].position);
                }

                b2 = Helpers.AABBFromCollection(list);
                s_boundCache.Add(c2, b2);
            }

            if (!b1.Intersects(b2))
            {
                result = Vector3.zero;
                return false;
            }

            for (int i = 1; i < c1.pointCount; i++)
            {
                for (int j = 1; j < c2.pointCount; j++)
                {
                    var intersects = Intersects(CubicBezier.FromBezierCurveBehind(c1, i),
                        CubicBezier.FromBezierCurveBehind(c2, j), precision, out result);

                    if (intersects)
                    {
                        return true;
                    }
                }
            }

            result = Vector3.zero;
            return false;
        }

        public static bool Intersects(CubicBezier c1,  CubicBezier c2, float precision, out Vector3 result)
        {
            Bounds b1 = c1.GetBounds();
            Bounds b2 = c2.GetBounds();

            // Vertical margin for misaligned tracks.
            b1.Encapsulate(c1.P0 + s_misalignedFix);
            b2.Encapsulate(c2.P0 + s_misalignedFix);

            if (!b1.Intersects(b2))
            {
                result = Vector3.zero;
                return false;
            }

            b1.Encapsulate(b2);

            if (b1.Area2D() < precision)
            {
                result = b1.center;
                return true;
            }

            foreach (var split1 in SplitBezier(c1, 0.5f))
            {
                foreach (var split2 in SplitBezier(c2, 0.5f))
                {
                    var intersects = Intersects(split1, split2, precision, out result);

                    if (intersects)
                    {
                        return true;
                    }
                }
            }

            result = Vector3.zero;
            return false;
        }

        public static CubicBezier[] SplitBezier(CubicBezier curve, float f)
        {
            CubicBezier[] results = new CubicBezier[2];
            results[0] = new CubicBezier();
            results[1] = new CubicBezier();

            Vector3 mid = curve.P1 + (curve.P2 - curve.P1) * f;

            results[0].P0 = curve.P0;
            results[0].P1 = curve.P0 + (curve.P1 - curve.P0) * f;
            results[0].P2 = results[0].P1 + (mid - results[0].P1) * f;
            results[0].P3 = BezierCurve.GetCubicCurvePoint(curve.P0, curve.P1, curve.P2, curve.P3, f);

            results[1].P3 = curve.P3;
            results[1].P2 = curve.P3 + (curve.P2 - curve.P3) * f;
            results[1].P1 = results[1].P2 + (mid - results[1].P2) * f;
            results[1].P0 = results[0].P3;

            return results;
        }

        public static (Vector3 Point, Vector3 Direction, float Distance) GetAproxPointAtLength(BezierCurve curve, float length)
        {
            float total = 0;
            Vector3 prev = curve.GetPointAt(0);
            Vector3 next;

            for (float f = 0; f < 1; f += AproxStep)
            {
                next = curve.GetPointAt(f);

                if (total >= length)
                {
                    return (curve.GetPointAt(f), curve.GetTangentAt(f), total);
                }

                total += Vector3.Magnitude(next - prev);
                prev = next;
            }

            return (curve.GetPointAt(1), curve.GetTangentAt(1), total);
        }

        public static (Vector3 Point, Vector3 Direction, float Distance) GetAproxPointAtLengthReverse(BezierCurve curve, float length)
        {
            float total = 0;
            Vector3 prev = curve.GetPointAt(1);
            Vector3 next;

            for (float f = 1; f > 0; f -= AproxStep)
            {
                next = curve.GetPointAt(f);

                if (total >= length)
                {
                    return (curve.GetPointAt(f), curve.GetTangentAt(f), total);
                }

                total += Vector3.Magnitude(next - prev);
                prev = next;
            }

            return (curve.GetPointAt(0), curve.GetTangentAt(0), total);
        }
    }
}
