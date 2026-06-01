using DV.CabControls;
using DV.CabControls.Spec;
using Signals.Common;
using Signals.Game.Railway;
using UnityEngine;

namespace Signals.Game.Misc
{
    public class SignalReservingObject : MonoBehaviour
    {
        private float _time;
        private Signal _signal = null!;
        private Transform _button = null!;
        private AudioClip? _success;
        private AudioClip? _failure;
        private AudioClip? _cancel;

        public static SignalReservingObject Create(SignalReservingObjectDefinition source, Signal signal)
        {
            source.Button.gameObject.SetActive(false);

            var rb = source.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            var button = source.Button.gameObject.AddComponent<Button>();
            button.colliderGameObjects = new[] { button.gameObject };
            button.gameObject.layer = 13;

            var highlight = button.gameObject.AddComponent<HighlightTag>();
            highlight.renderers = source.HighlightRenderers;

            var comp = source.gameObject.AddComponent<SignalReservingObject>();
            comp._time = source.Time;
            comp._success = source.SuccessSound;
            comp._failure = source.FailureSound;
            comp._cancel = source.CancelSound;
            comp._button = source.Button;
            comp._signal = signal;

            Destroy(source);
            return comp;
        }

        private void Start()
        {
            _button.gameObject.SetActive(true);

            var button = GetComponentInChildren<ButtonBase>(true);
            button.Used += ButtonUsed;
        }

        private void ButtonUsed()
        {
            if (TrackReserver.HasReservation(_signal))
            {
                TrackReserver.ClearFromSignal(_signal);
                _cancel?.Play(transform.position);
                return;
            }

            if (TrackReserver.ReserveForSignal(_signal, _time))
            {
                _success?.Play(transform.position);
            }
            else
            {
                _failure?.Play(transform.position);
            }
        }
    }
}
