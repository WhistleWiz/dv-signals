using System.Collections.Generic;
using UnityEngine;

namespace Signals.Common
{
    [AddComponentMenu("DV Signals/Signal Reserving Object")]
    public class SignalReservingObjectDefinition : MonoBehaviour
    {
        public float Time = 60.0f;
        public Transform Button = null!;
        public List<Renderer> HighlightRenderers = new List<Renderer>();
        [Tooltip("Plays when a reservation is successfuly made")]
        public AudioClip? SuccessSound;
        [Tooltip("Plays when a reservation cannot be made")]
        public AudioClip? FailureSound;
        [Tooltip("Plays when a reservation is cancelled")]
        public AudioClip? CancelSound;
    }
}
