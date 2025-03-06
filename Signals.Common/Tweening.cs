using UnityEngine;

namespace Signals.Common
{
    public static class Tweening
    {
        public enum EasingMode
        {
            Linear,
            Smooth,
            Bounce,
            Curve = 1000
        }

        // Only designed to work in [0..1], clamping not used for performance.
        public static Vector3 Ease(Vector3 a, Vector3 b, float t, EasingMode mode, AnimationCurve curve = null!)
        {
            return a + (b - a) * Ease(t, mode, curve);
        }

        public static float Ease(float t, EasingMode mode, AnimationCurve curve = null!)
        {
            return mode switch
            {
                EasingMode.Linear => Linear(t),
                EasingMode.Smooth => Smooth(t),
                EasingMode.Bounce => Bounce(t),

                EasingMode.Curve => curve.Evaluate(t),
                _ => t > 0 ? 1 : 0,
            };
        }

        public static float Linear(float t)
        {
            return t;
        }

        public static float Smooth(float t)
        {
            return t * t * (3.0f - 2.0f * t);
        }

        public static float Bounce(float k)
        {
            if (k < (1f / 2.75f))
            {
                return 7.5625f * k * k;
            }
            else if (k < (2f / 2.75f))
            {
                return 7.5625f * (k -= 1.5f / 2.75f) * k + 0.75f;
            }
            else if (k < (2.5 / 2.75))
            {
                return 7.5625f * (k -= 2.25f / 2.75f) * k + 0.9375f;
            }
            else
            {
                return 7.5625f * (k -= 2.625f / 2.75f) * k + 0.984375f;
            }
        }
    }
}
