using System.Collections;
using UnityEngine;

namespace Signals.Common
{
    public class TransformMover : MonoBehaviour
    {
        public enum MoveMode
        {
            Linear,
            Smooth
        }

        [Header("Original")]
        public Vector3 OriginalPosition = Vector3.zero;
        public Vector3 OriginalRotation = Vector3.zero;
        public Vector3 OriginalScale = Vector3.one;

        [Header("Transformed")]
        public Vector3 TransformedPosition = Vector3.zero;
        public Vector3 TransformedRotation = Vector3.zero;
        public Vector3 TransformedScale = Vector3.one;

        [Space]
        public MoveMode Mode = MoveMode.Linear;
        [Min(0.0f)]
        public float Duration = 1.0f;

        private float _current = 0.0f;
        private float _target = 0.0f;
        private Coroutine? _moveCoro;

        public void Reset()
        {
            OriginalPosition = transform.localPosition;
            OriginalRotation = transform.localRotation.eulerAngles;
            OriginalScale = transform.localScale;

            TransformedPosition = OriginalPosition;
            TransformedRotation = OriginalRotation;
            TransformedScale = OriginalScale;
        }

        public void ToTransformed()
        {
            SetTargetAndStart(1.0f);
        }

        public void ToOriginal()
        {
            SetTargetAndStart(0.0f);
        }

        private void SetTargetAndStart(float value)
        {
            _target = value;

            // Would love to
            // _moveCoro ??= StartCoroutine(MoveRoutine());
            // But should I?
            if (_moveCoro == null)
            {
                _moveCoro = StartCoroutine(MoveRoutine());
            }
        }

        private IEnumerator MoveRoutine()
        {
            // Eh this optimisation isn't really needed.
            //bool pos = OriginalPosition != TransformedPosition;
            //bool rot = OriginalRotation != TransformedRotation;
            //bool scale = OriginalScale != TransformedScale;

            while (_current != _target)
            {
                _current = Mathf.MoveTowards(_current, _target, Time.deltaTime / Duration);

                transform.localPosition = Lerp(OriginalPosition, TransformedPosition, _current, Mode);
                transform.localRotation = Quaternion.Euler(Lerp(OriginalRotation, TransformedRotation, _current, Mode));
                transform.localScale = Lerp(OriginalScale, TransformedScale, _current, Mode);

                yield return null;
            }

            _moveCoro = null;
        }

        // Only designed to work in [0..1], clamping not used for performance.
        private static Vector3 Lerp(Vector3 a, Vector3 b, float t, MoveMode mode)
        {
            return mode switch
            {
                MoveMode.Linear => a + (b - a) * t,
                MoveMode.Smooth => a + (b - a) * (t * t * (3.0f - 2.0f * t)),
                _ => t > 0 ? b : a,
            };
        }
    }
}
