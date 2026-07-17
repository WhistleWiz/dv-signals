using System.Collections.Generic;
using UnityEngine;

namespace Signals.Common
{
    [AddComponentMenu("DV Signals/Signal Reserving Object")]
    public class SignalReservingObjectDefinition : MonoBehaviour
    {
        public float Time = 60.0f;
        public Transform Button = null!;
        public Vector3 PushedLocalOffset = Vector3.zero;
        public bool DisableVRTouchUse = true;
        public List<Renderer> HighlightRenderers = new List<Renderer>();

        [Space]
        [Tooltip("Plays when a reservation is successfuly made")]
        public AudioClip? SuccessSound;
        [Tooltip("Plays when a reservation cannot be made")]
        public AudioClip? FailureSound;
        [Tooltip("Plays when a reservation is cancelled")]
        public AudioClip? CancelSound;

        private void OnDrawGizmos()
        {
            if (PushedLocalOffset.sqrMagnitude <= 0.001 || Button == null) return;

            Gizmos.DrawLine(Button.position, Button.TransformPoint(PushedLocalOffset));
        }
    }
}
