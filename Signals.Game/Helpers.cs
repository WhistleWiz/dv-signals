using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Signals.Game
{
    internal class Helpers
    {
        public static System.Collections.IEnumerator DisableBehaviour(Behaviour behaviour, float time)
        {
            yield return new WaitForSeconds(time);
            behaviour.enabled = false;
        }

        public static Bounds AABBFromPoint(params Vector3[] points)
        {
            return AABBFromCollection(points);
        }

        public static Bounds AABBFromCollection(IEnumerable<Vector3> points)
        {
            var b = new Bounds(points.First(), Vector3.zero);

            foreach (var point in points)
            {
                b.Encapsulate(point);
            }

            return b;
        }

        public static Bounds FromMinMax(Vector3 min, Vector3 max)
        {
            var bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }

        public static Bounds Intersection(Bounds b1, Bounds b2)
        {
            return SmallestBounds(FromMinMax(b1.min, b2.max), FromMinMax(b1.max, b2.min));
        }

        public static Bounds SmallestBounds(Bounds b1, Bounds b2)
        {
            return b1.extents.sqrMagnitude < b2.extents.sqrMagnitude ? b1 : b2;
        }

        public static float DistanceSqr(Vector3 v1, Vector3 v2)
        {
            return (v2 - v1).sqrMagnitude;
        }
    }
}
