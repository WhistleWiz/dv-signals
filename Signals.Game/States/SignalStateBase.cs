using Signals.Common.States;
using System.Linq;
using UnityEngine;

namespace Signals.Game.States
{
    public abstract class SignalStateBase
    {
        public SignalController Controller = null!;
        public SignalStateBaseDefinition Definition;

        private SignalLight[] _on = null!;
        private SignalLight[] _blink = null!;
        private int? _animationId;

        public string Id => Definition.Id;

        public SignalStateBase(SignalStateBaseDefinition def)
        {
            Definition = def;

            _on = def.OnLights.Select(x => x.GetController()).ToArray();
            _blink = def.BlinkingLights.Select(x => x.GetController()).ToArray();

            if (!string.IsNullOrEmpty(def.AnimationName))
            {
                _animationId = Animator.StringToHash(def.AnimationName);
            }
        }

        /// <summary>
        /// Checks if the conditions for this state to be used are true.
        /// </summary>
        public abstract bool MeetsConditions(RailTrack[] tracksToNextSignal, SignalController? nextSignal);

        public void Apply()
        {
            foreach (SignalLight light in _on)
            {
                light.TurnOn(false);
            }

            foreach (SignalLight light in _blink)
            {
                light.TurnOn(true);
            }

            PlayAnimation();
            PlaySound();
        }

        public void Unapply()
        {
            foreach (SignalLight light in _on)
            {
                light.TurnOff();
            }

            foreach (SignalLight light in _blink)
            {
                light.TurnOff();
            }
        }

        private void PlayAnimation()
        {
            if (Controller.Definition.Animator == null || !_animationId.HasValue)
            {
                return;
            }

            Controller.Definition.Animator.enabled = true;
            Controller.Definition.Animator.CrossFadeInFixedTime(_animationId.Value, Definition.AnimationTime, 0);

            if (Definition.DisableAnimatorAfterChanging)
            {
                Controller.DisableAnimator(Definition.AnimationTime + 0.1f);
            }
        }

        private void PlaySound()
        {
            if (Definition.ActivationAudios.Length > 0)
            {
                Definition.ActivationAudios.Play(Definition.AudioPosition != null ? Definition.AudioPosition.position : Definition.transform.position,
                    mixerGroup: AudioManager.Instance.switchGroup);
            }
        }
    }
}
