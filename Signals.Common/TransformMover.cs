using System.Collections;
using UnityEngine;

namespace Signals.Common
{
    public class TransformMover : MonoBehaviour
    {
        [Header("Original")]
        public Vector3 OriginalPosition = Vector3.zero;
        public Vector3 OriginalRotation = Vector3.zero;
        public Vector3 OriginalScale = Vector3.one;

        [Header("Transformed")]
        public Vector3 TransformedPosition = Vector3.zero;
        public Vector3 TransformedRotation = Vector3.zero;
        public Vector3 TransformedScale = Vector3.one;

        [Header("Interpolation Settings")]
        public Tweening.EasingMode Mode = Tweening.EasingMode.Linear;
        [Min(0.0f)]
        public float Duration = 1.0f;
        public bool UseAbsoluteValue = true;
        public AnimationCurve CustomCurve = AnimationCurve.Linear(0, 0, 1, 1);

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
            // Delay the start a frame to prevent setting and unsetting.
            yield return null;

            // Eh this optimisation isn't really needed.
            //bool pos = OriginalPosition != TransformedPosition;
            //bool rot = OriginalRotation != TransformedRotation;
            //bool scale = OriginalScale != TransformedScale;

            float target = _target;
            float t;
            float dif;

            while (_current != target)
            {
                _current = Duration == 0 ? target : Mathf.MoveTowards(_current, target, Time.deltaTime / Duration);

                // For absolute values, use the difference between the current value and the target.
                // If the difference is positive, the normal process can still be used.
                if (UseAbsoluteValue && Mathf.Sign(dif = target - _current) < 0)
                {
                    t = 1 - Tweening.Interpolate(1 + dif, Mode, CustomCurve);
                }
                else
                {
                    t = Tweening.Interpolate(_current, Mode, CustomCurve);
                }

                transform.localPosition = Vector3.LerpUnclamped(OriginalPosition, TransformedPosition, t);
                transform.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(OriginalRotation, TransformedRotation, t));
                transform.localScale = Vector3.LerpUnclamped(OriginalScale, TransformedScale, t);

                yield return null;
            }

            _moveCoro = _target != target ? StartCoroutine(MoveRoutine()) : null;
        }
    }
}
