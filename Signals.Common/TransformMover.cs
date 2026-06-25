using System;
using System.Collections;
using UnityEngine;

namespace Signals.Common
{
    [AddComponentMenu("DV Signals/Transform Mover"), DisallowMultipleComponent]
    public class TransformMover : MonoBehaviour
    {
        [Header("Original")]
        public Vector3 OriginalPosition = Vector3.zero;
        public Vector3 OriginalRotation = Vector3.zero;
        public Vector3 OriginalScale = Vector3.one;

        [Header("Interpolation Settings")]
        public Tweening.EasingMode Mode = Tweening.EasingMode.Linear;
        [Min(0.0f)]
        public float Duration = 1.0f;
        public AnimationCurve CustomCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private Coroutine? _moveCoro;
        private Vector3 _targetPosition;
        private Vector3 _targetRotation;
        private Vector3 _targetScale;
        private Vector3 _lastPosition;
        private Vector3 _lastRotation;
        private Vector3 _lastScale;

        public bool Moving => _moveCoro != null;

        public Action<bool>? OnReachTarget;

        public void Reset()
        {
            OriginalPosition = transform.localPosition;
            OriginalRotation = transform.localRotation.eulerAngles;
            OriginalScale = transform.localScale;
        }

        private void Awake()
        {
            transform.localPosition = _lastPosition = _targetPosition = OriginalPosition;
            transform.localRotation = Quaternion.Euler(_lastRotation = _targetRotation = OriginalRotation);
            transform.localScale = _lastScale = _targetScale = OriginalScale;
        }

        public void SetTarget(TransformMoverTarget? target)
        {
            if (target == null)
            {
                _targetPosition = OriginalPosition;
                _targetRotation = OriginalRotation;
                _targetScale = OriginalScale;
            }
            else
            {
                _targetPosition = target.Position;
                _targetRotation = target.Rotation;
                _targetScale = target.Scale;
            }

            if (_moveCoro == null && TargetIsDifferent())
            {
                _moveCoro = StartCoroutine(MoveRoutine());
            }
        }

        private IEnumerator MoveRoutine()
        {
            // Delay the start a frame to prevent setting and unsetting.
            yield return null;

            var startP = _lastPosition;
            var startR = _lastRotation;
            var startS = _lastScale;
            var endP = _targetPosition;
            var endR = _targetRotation;
            var endS = _targetScale;

            // If the duration is too low, just skip to the end.
            if (Duration > 0.001f)
            {
                float percent = 0.0f;
                float t;

                while (percent < 1)
                {
                    percent += Time.deltaTime / Duration;
                    t = Tweening.Interpolate(percent, Mode, CustomCurve);

                    transform.localPosition = Vector3.LerpUnclamped(startP, endP, t);
                    transform.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(startR, endR, t));
                    transform.localScale = Vector3.LerpUnclamped(startS, endS, t);

                    yield return null;
                }
            }

            // Snap to final position.
            transform.localPosition = _lastPosition = endP;
            transform.localRotation = Quaternion.Euler(_lastRotation = endR);
            transform.localScale = _lastScale = endS;

            OnReachTarget?.Invoke(IsOriginalTransform());

            // Start movement again if the target changed while this routine ran.
            _moveCoro = TargetIsDifferent() ? StartCoroutine(MoveRoutine()) : null;
        }

        private bool TargetIsDifferent()
        {
            return _lastPosition != _targetPosition || _lastRotation != _targetRotation || _lastScale != _targetScale;
        }

        private bool IsOriginalTransform()
        {
            return _lastPosition == OriginalPosition && _lastRotation == OriginalRotation && _lastScale == OriginalScale;
        }
    }
}
