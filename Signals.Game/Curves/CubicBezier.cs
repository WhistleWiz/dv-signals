using System.Collections;
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
