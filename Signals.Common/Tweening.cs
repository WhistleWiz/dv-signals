using UnityEngine;

namespace Signals.Common
{
    public static class Tweening
    {
        public enum EasingMode
        {
            Linear = 0,

            QuadStart = 10,
            QuadStop,
            Smooth,

            BounceStart = 100,
            BounceStop,

            ElasticStart = 120,
            ElasticStop,

            Curve = 1000
        }

        const float C4 = 2.0f * Mathf.PI / 3.0f;

        // Only designed to work in [0..1], clamping is excluded.
        public static float Interpolate(float t, EasingMode mode, AnimationCurve curve = null!)
        {
            return mode switch
            {
                EasingMode.Linear => Linear(t),

                EasingMode.QuadStart => QuadStart(t),
                EasingMode.QuadStop => QuadStop(t),
                EasingMode.Smooth => Smooth(t),

                EasingMode.BounceStart => BounceStart(t),
                EasingMode.BounceStop => BounceStop(t),

                EasingMode.ElasticStart => ElasticStart(t),
                EasingMode.ElasticStop => ElasticStop(t),

                EasingMode.Curve => curve.Evaluate(t),
                _ => t > 0 ? 1 : 0,
            };
        }

        public static Vector3 Interpolate(Vector3 a, Vector3 b, float t, EasingMode mode, AnimationCurve curve = null!)
        {
            return a + (b - a) * Interpolate(t, mode, curve);
        }

        public static float Linear(float t)
        {
            return t;
        }

        public static float Smooth(float t)
        {
            return t * t * (3.0f - 2.0f * t);
        }

        public static float QuadStart(float t)
        {
            return t * t;
        }

        public static float QuadStop(float t)
        {
            return 1 - QuadStart(1 - t);
        }

        public static float BounceStart(float t)
        {
            return 1 - BounceStop(1 - t);
        }

        public static float BounceStop(float t)
        {
            if (t < (1f / 2.75f))
            {
                return 7.5625f * t * t;
            }
            else if (t < (2f / 2.75f))
            {
                return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
            }
            else if (t < (2.5 / 2.75))
            {
                return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
            }
            else
            {
                return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
            }
        }

        public static float ElasticStart(float t)
        {
            return 1 - ElasticStop(1 - t);
        }

        public static float ElasticStop(float t)
        {
            if (t == 0) return 0;
            if (t == 1) return 1;

            float f = 1.1f / (10.0f * (t + 0.1f)) - 0.1f;
            // f = MathF.Pow(2, -10 * t);

            return f * Mathf.Sin((t * 10 - 0.75f) * C4) + 1;
        }
    }
}
