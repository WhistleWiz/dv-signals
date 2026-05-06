using Signals.Game.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Signals.Game.Curves
{
    public class CubicBezier
    {
        public Vector3 P0;
        public Vector3 P1;
        public Vector3 P2;
        public Vector3 P3;

        public CubicBezier() : this(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero) { }

        public CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }

        public Vector3 this[int index] => index switch
        {
            0 => P0,
            1 => P1,
            2 => P2,
            3 => P3,
            _ => throw new System.ArgumentOutOfRangeException(nameof(index))
        };

        public Bounds GetBounds()
        {
            return Helpers.AABBFromCollection(GetPoints());
        }

        public IEnumerable<Vector3> GetPoints()
        {
            yield return P0;
            yield return P1;
            yield return P2;
            yield return P3;
        }

        public Vector3 Derivative(float t)
        {
            float tInv = 1.0f - t;
            float tInvSqr = tInv * tInv;
            float tSqr = t * t;

            return 3.0f * tInvSqr * (P1 - P0) + 6.0f * tInv * t * (P2 - P1) + 3.0f * tSqr * (P3 - P2);
        }

        public Vector3 SecondDerivative(float t)
        {
            return 6.0f * (1.0f - t) * (P2 - 2.0f * P1 + P0) + 6.0f * t * (P3 - 2.0f * P2 + P1);
        }

        public float EndsDistanceSqr()
        {
            return (P3 - P0).sqrMagnitude;
        }

        public float EndsDistance()
        {
            return (P3 - P0).magnitude;
        }

        public static CubicBezier FromBezierCurve(BezierCurve curve, int index)
        {
            return new CubicBezier(curve[index].position, curve[index].globalHandle2, curve[index + 1].globalHandle1, curve[index + 1].position);
        }

        public static CubicBezier FromBezierCurveBehind(BezierCurve curve, int index)
        {
            return new CubicBezier(curve[index - 1].position, curve[index - 1].globalHandle2, curve[index].globalHandle1, curve[index].position);
        }
    }
}
