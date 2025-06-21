using UnityEngine;

namespace Signals.Game
{
    public static class OldAreaCalculator
    {
        private class Area
        {
            public Vector2 Centre;
            public float RadiusSqr;

            public Area(float x, float y, float r, bool scale = true)
            {
                if (scale)
                {
                    x *= 1000;
                    y *= 1000;
                    r *= 1000;
                }

                Centre = new Vector2(x, y);
                RadiusSqr = r * r;
            }
        }

        private static readonly Area[] s_areas = new[]
        {
            // IMW
            new Area( 2.0f, 14.0f, 2.5f),
            // SW
            new Area( 3.0f,  2.0f, 3.0f),
            // FRS
            new Area( 6.0f,  3.0f, 1.7f),
            // CMS
            new Area( 8.0f,  3.0f, 1.9f),
            // East
            new Area(14.0f,  6.0f, 1.0f),
            // IME
            new Area(13.0f, 15.0f, 3.0f),
            // CME
            new Area(15.0f, 11.5f, 1.5f),
            // FRC
            new Area( 5.7f,  8.3f, 1.0f),
            // Home
            new Area( 5.0f,  7.0f, 0.5f),
        };

        public static bool IsWithinOldArea(Vector3 position)
        {
            var flat = new Vector2(position.x, position.z);

            for (int i = 0; i < s_areas.Length; i++)
            {
                if (Vector2.SqrMagnitude(s_areas[i].Centre - flat) < s_areas[i].RadiusSqr) return true;
            }

            return false;
        }

        internal static void DebugCreateDummies()
        {
            var t = RailTrackRegistryBase.Junctions[0].transform;

            foreach (var area in s_areas)
            {
                var size = Mathf.Sqrt(area.RadiusSqr) * 2.0f;
                var instance = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
                instance.parent = t;
                instance.transform.localScale = new Vector3(size, 1000, size);
                instance.transform.position = new Vector3(area.Centre.x, 0, area.Centre.y);

                foreach (var col in instance.GetComponentsInChildren<Collider>())
                {
                    Object.Destroy(col);
                }
            }
        }
    }
}
