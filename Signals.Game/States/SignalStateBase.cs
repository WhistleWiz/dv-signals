using Signals.Common.States;
using System.Linq;
using UnityEngine;

namespace Signals.Game.States
{
    /// <summary>
    /// Base class for a signal state. It handles lights, animation and sound from a
    /// <see cref="SignalStateBaseDefinition"/> automatically.
    /// </summary>
    public abstract class SignalStateBase
    {
        public SignalStateBaseDefinition Definition;
        public SignalController Controller;

        private SignalLight[] _on = null!;
        private SignalLight[] _blink = null!;
        private int? _animationId;

        public string Id => Definition.Id;

        public SignalStateBase(SignalStateBaseDefinition def, SignalController controller)
        {
            Definition = def;
            Controller = controller;

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

        /// <summary>
        /// Applies the state.
        /// </summary>
        /// <remarks>
        /// When overriding, keep the call to the base version to correctly apply the base features (lights, sound, and animation).
        /// </remarks>
        public virtual void Apply()
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

        /// <summary>
        /// Unapplies the state.
        /// </summary>
        /// <remarks>
        /// When overriding, keep the call to the base version to correctly unapply the base features (lights).
        /// <para>Animation state is only restored to default by the signal controller this state belongs to.</para>
        /// <para>It will also not play sound.</para>
        /// </remarks>
        public virtual void Unapply()
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

        /// <summary>
        /// Selects a random activation audio clip and plays it.
        /// </summary>
        protected void PlaySound()
        {
            if (Definition.ActivationAudios.Length > 0)
            {
                Definition.ActivationAudios.Play(Definition.AudioPosition != null ? Definition.AudioPosition.position : Definition.transform.position,
                    mixerGroup: AudioManager.Instance.switchGroup);
            }
        }
    }
}
