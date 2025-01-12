using Signals.Common.Aspects;
using System.Linq;
using UnityEngine;

namespace Signals.Game.Aspects
{
    /// <summary>
    /// Base class for a signal aspect. It handles lights, animation and sound from a
    /// <see cref="SignalAspectBaseDefinition"/> automatically.
    /// </summary>
    public abstract class SignalAspectBase
    {
        public SignalAspectBaseDefinition Definition;
        public SignalController Controller;

        private SignalLight[] _on = null!;
        private SignalLight[] _blink = null!;
        private int? _animationId;

        public string Id => Definition.Id;

        public SignalAspectBase(SignalAspectBaseDefinition def, SignalController controller)
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
        /// Checks if the conditions for this aspect to be used are true.
        /// </summary>
        public abstract bool MeetsConditions(RailTrack[] tracksToNextSignal, SignalController? nextSignal);

        /// <summary>
        /// Applies the aspect.
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
        /// Unapplies the aspect.
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
