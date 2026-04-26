using DV;
using DV.Highlighting;
using Signals.Game.Railway;
using UnityEngine;

namespace Signals.Game
{
    internal class CommsRadioSignalReserver : MonoBehaviour, ICommsRadioMode
    {
        // Block reservation durations.
        private static readonly float[] s_durations = new[]
        {
            30f,
            60f,
            120f,
            180f,
            300f
        };

        private const int Mask = 1 << 15;
        private const float ReturnToMainScreenTime = 4.0f;

        private int _durationIndex;
        private bool _active = false;
        private bool _displayOverriden = false;
        private RaycastHit _hit;
        private Signal? _signal;
        private Coroutine? _resetDisplayCoro;

        public CommsRadioController Controller = null!;
        public CommsRadioDisplay Display = null!;
        public Transform SignalOrigin = null!;

        private float Duration => s_durations[_durationIndex];
        private AudioClip? ConfirmSound => Controller.crewVehicleControl.confirmSound;
        private AudioClip? SuccessSound => Controller.crewVehicleControl.spawnVehicleSound;
        private AudioClip? CancelSound => Controller.crewVehicleControl.cancelSound;

        public ButtonBehaviourType ButtonBehaviour { get; private set; }

        private void Awake()
        {
            SignalOrigin = Controller.laserBeam.transform;
            Display = Controller.crewVehicleControl.display;
        }

        public void Enable()
        {
            _active = false;
            ButtonBehaviour = ButtonBehaviourType.Regular;
            SetStartingDisplay();
        }

        public void Disable()
        {
            _active = false;
            StopDisplayCoro();
        }

        public void OnUpdate()
        {
            if (!_active) return;

            if (Physics.Raycast(SignalOrigin.position, SignalOrigin.forward, out _hit, 1000f, Mask) &&
                _hit.transform.TryGetComponent(out SignalDefinitionToInstance comp))
            {
                var signal = comp.Signal;

                if (signal != _signal)
                {
                    HighlightSignal(_signal, false);
                    HighlightSignal(signal, true);

                    _signal = comp.Signal;

                    SetDisplayToSignal();
                }
            }
            else
            {
                if (_signal != null)
                {
                    HighlightSignal(_signal, false);
                    _signal = null;
                    SetDisplayToSignal();
                }

                _signal = null;
            }
        }

        public void OnUse()
        {
            if (!_active)
            {
                _active = true;
                ButtonBehaviour = ButtonBehaviourType.Override;
                PlayRadioSound(ConfirmSound);
                SetDisplayToSignal();
                return;
            }

            if (_displayOverriden)
            {
                PlayRadioSound(CancelSound);
                StopDisplayCoro();
                SetDisplayToSignal();
                return;
            }

            if (_signal == null)
            {
                _active = false;
                ButtonBehaviour = ButtonBehaviourType.Regular;
                PlayRadioSound(CancelSound);
                SetStartingDisplay();
                return;
            }

            if (TrackReserver.ReserveForSignal(_signal, Duration))
            {
                PlayRadioSound(SuccessSound);
                SetSuccessDisplay();
            }
            else
            {
                PlayRadioSound(CancelSound);
                SetFailedDisplay();
            }

            StartDisplayCoro();
        }

        public bool ButtonACustomAction()
        {
            _durationIndex--;

            if (_durationIndex < 0)
            {
                _durationIndex = s_durations.Length - 1;
            }

            SetDisplayToSignal();
            return true;
        }

        public bool ButtonBCustomAction()
        {
            _durationIndex = (_durationIndex + 1) % s_durations.Length;

            SetDisplayToSignal();
            return true;
        }

        public Color GetLaserBeamColor()
        {
            return Color.red;
        }

        public void OverrideSignalOrigin(Transform signalOrigin)
        {
            SignalOrigin = signalOrigin;
        }

        public void SetStartingDisplay()
        {
            StopDisplayCoro();
            Display.SetDisplay("Signal Reserver", "Click to begin");
        }

        private void SetFailedDisplay()
        {
            StopDisplayCoro();
            Display.SetContentAndAction("Could not reserve signal");
        }

        private void SetSuccessDisplay()
        {
            StopDisplayCoro();
            Display.SetContentAndAction($"Reserved signal for {Duration:F0} seconds");
        }

        private void SetDisplayToSignal()
        {
            if (_displayOverriden) return;

            if (_signal == null)
            {
                Display.SetContentAndAction($"Signal: None\nDuration: {Duration} seconds", "");
            }
            else
            {
                Display.SetContentAndAction($"Signal: {_signal.Id}\nDuration: {Duration} seconds", "Reserve");
            }
        }

        private void PlayRadioSound(AudioClip? clip)
        {
            if (clip != null)
            {
                CommsRadioController.PlayAudioFromRadio(clip, transform);
            }
        }

        private void StartDisplayCoro()
        {
            _resetDisplayCoro = StartCoroutine(ReturnToStartRoutine());
        }

        private void StopDisplayCoro()
        {
            if (_resetDisplayCoro != null)
            {
                StopCoroutine(_resetDisplayCoro);
            }

            _displayOverriden = false;
        }

        private System.Collections.IEnumerator ReturnToStartRoutine()
        {
            _displayOverriden = true;
            yield return WaitFor.Seconds(ReturnToMainScreenTime);
            _displayOverriden = false;

            if (_active)
            {
                SetDisplayToSignal();
            }
            else
            {
                SetStartingDisplay();
            }
        }

        private static void HighlightSignal(Signal? signal, bool on)
        {
            if (signal == null) return;

            foreach (var renderer in signal.HighlightRenderers)
            {
                AGeneralHighlighter.Instance.ToggleHighlight(on, renderer, AGeneralHighlighter.HighlightType.Sign, false);
            }
        }
    }
}
