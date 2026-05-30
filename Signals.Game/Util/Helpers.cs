using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Signals.Game.Util
{
    internal class Helpers
    {
        public static System.Collections.IEnumerator DisableBehaviour(Behaviour behaviour, float time)
        {
            yield return WaitFor.Seconds(time);
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

        public static int ClampBounds<T>(int index, T[] array)
        {
            return Mathf.Clamp(index, 0, array.Length - 1);
        }

        public static double ClampD(double value, double min, double max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }

        public static Quaternion FlattenLook(Vector3 direction)
        {
            return Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        }

        public static string OrientationSimple(Vector3 north, Vector3 direction)
        {
            var angle = Vector3.SignedAngle(direction, north, Vector3.up);

            if (angle < -135 || angle >= 135)
            {
                return "S";
            }
            if (angle < -45)
            {
                return "E";
            }
            if (angle >= 45)
            {
                return "W";
            }
            return "N";
        }

        public static string Orientation(Vector3 north, Vector3 direction)
        {
            var angle = Vector3.SignedAngle(direction, north, Vector3.up);

            if (angle < -157.5 || angle >= 157.5)
            {
                return "S";
            }
            if (angle < -112.5)
            {
                return "SE";
            }
            if (angle >= 112.5)
            {
                return "SW";
            }
            if (angle < -67.5)
            {
                return "E";
            }
            if (angle >= 67.5)
            {
                return "W";
            }
            if (angle < -22.5)
            {
                return "NE";
            }
            if (angle >= 22.5)
            {
                return "NW";
            }
            return "N";
        }
    }
}
