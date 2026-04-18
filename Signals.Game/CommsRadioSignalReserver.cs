using DV;
using DV.Highlighting;
using Signals.Game.Controllers;
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
        private BasicSignalController? _controller;
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
                _hit.transform.TryGetComponent(out SignalDefinitionToController comp))
            {
                var controller = comp.Controller;

                if (controller != _controller)
                {
                    HighlightController(_controller, false);
                    HighlightController(controller, true);

                    _controller = comp.Controller;

                    SetDisplayToSignal();
                }
            }
            else
            {
                if (_controller != null)
                {
                    HighlightController(_controller, false);
                    _controller = null;
                    SetDisplayToSignal();
                }

                _controller = null;
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

            if (_controller == null)
            {
                _active = false;
                ButtonBehaviour = ButtonBehaviourType.Regular;
                PlayRadioSound(CancelSound);
                SetStartingDisplay();
                return;
            }

            if (TrackReserver.ReserveForSignal(_controller, Duration))
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

            if (_controller == null)
            {
                Display.SetContentAndAction($"Signal: None\nDuration: {Duration} seconds", "");
            }
            else
            {
                Display.SetContentAndAction($"Signal: {_controller.Id}\nDuration: {Duration} seconds", "Reserve");
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

        private static void HighlightController(BasicSignalController? controller, bool on)
        {
            if (controller == null) return;

            foreach (var renderer in controller.HighlightRenderers)
            {
                AGeneralHighlighter.Instance.ToggleHighlight(on, renderer, AGeneralHighlighter.HighlightType.Sign, false);
            }
        }
    }
}
