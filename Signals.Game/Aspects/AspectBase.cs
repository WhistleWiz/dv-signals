using Signals.Common.Aspects;
using Signals.Game.Controllers;
using Signals.Game.Lights;
using Signals.Game.Railway;
using System.Linq;
using UnityEngine;

namespace Signals.Game.Aspects
{
    /// <summary>
    /// Helper interface to handle generics. Do not implement directly, use <see cref="AspectBase"/> instead.
    /// </summary>
    public interface IAspect : IHudDisplayable
    {
        public string Id { get; }
        public bool MeetsConditions();
        public void Apply();
        public void Unapply();
    }

    /// <summary>
    /// Base class for a signal aspect. It handles lights, animation and sound from a
    /// <see cref="AspectBaseDefinition"/> automatically.
    /// </summary>
    /// <typeparam name="T">The <see cref="AspectBaseDefinition"/> for the aspect.</typeparam>
    public abstract class AspectBase<T> : IAspect
        where T : AspectBaseDefinition
    {
        private Coroutine? _sync;
        private SignalLight[] _on = null!;
        private SignalLight[] _blink = null!;
        private SignalLightSequence[] _sequences = null!;

        public T Definition { get; private set; }
        public Signal Signal { get; private set; }

        public string Id => Definition.Id;
        public TrackBlock? Block => Signal.Block;
        public bool Active { get; private set; }
        public bool ShouldDisplay => true;
        public int DisplayOrder => Definition.HUDDisplayOrder;
        public string? DisplayText => Signal.DisplayText;
        public Sprite? Sprite => Definition.HUDSprite;
        public Color TextColour => Signal.TextColour;
        public BasicSignalController Controller => Signal.Controller;

        public AspectBase(AspectBaseDefinition definition, Signal signal)
        {
            Signal = signal;
            Definition = (T)definition;

            if (Definition == null) throw new System.ArgumentException($"Type mismatch between definition and expected {typeof(T)}", nameof(definition));

            _on = definition.OnLights.Select(x => x.GetController(signal)).ToArray();
            _blink = definition.BlinkingLights.Select(x => x.GetController(signal)).ToArray();
            _sequences = definition.LightSequences.Select(x => x.GetController(signal)).ToArray();

            Active = false;
        }

        public AspectBaseDefinition GetDefinition() => Definition;

        /// <summary>
        /// Checks if the conditions for this aspect to be used are true.
        /// </summary>
        public abstract bool MeetsConditions();

        /// <summary>
        /// Applies the aspect.
        /// </summary>
        /// <remarks>
        /// When overriding, keep the call to the base version to correctly apply the base features (lights, sound, and animation).
        /// </remarks>
        public virtual void Apply()
        {
            if (Signal.Definition.SynchroniseLamps && CheckLamps())
            {
                Signal.Definition.StartCoroutine(SynchoniseLamps());
            }
            else
            {
                ApplyBlinkingLamps();
            }

            foreach (SignalLight light in _on)
            {
                light.TurnOn(false);
            }

            foreach (var t in Definition.Movers)
            {
                t.Apply();
            }

            PlaySound();

            Active = true;
        }

        /// <summary>
        /// Unapplies the aspect.
        /// </summary>
        /// <remarks>
        /// When overriding, keep the call to the base version to correctly unapply the base features (lights).
        /// <para>It will also not play sound.</para>
        /// </remarks>
        public virtual void Unapply()
        {
            if (_sync != null)
            {
                Signal.Definition.StopCoroutine(_sync);
            }

            foreach (SignalLight light in _on)
            {
                light.TurnOff();
            }

            foreach (SignalLight light in _blink)
            {
                light.TurnOff();
            }

            foreach (SignalLightSequence sequence in _sequences)
            {
                sequence.Deactivate();
            }

            foreach (var t in Definition.Movers)
            {
                t.Unapply();
            }

            Active = false;
        }

        /// <summary>
        /// Selects a random activation audio clip and plays it.
        /// </summary>
        protected void PlaySound()
        {
            if (Definition.ActivationAudios.Length > 0)
            {
                Definition.ActivationAudios.Play(Definition.transform.position, mixerGroup: AudioManager.Instance.switchGroup);
            }
        }

        private System.Collections.IEnumerator SynchoniseLamps()
        {
            while (true)
            {
                if (CheckLamps())
                {
                    yield return null;
                    continue;
                }

                break;
            }

            ApplyBlinkingLamps();
            _sync = null;
        }

        private bool CheckLamps()
        {
            return _blink.Any(x => x.IsActive) || _sequences.Any(x => x.AnyLightActive());
        }

        private void ApplyBlinkingLamps()
        {
            foreach (SignalLight light in _blink)
            {
                light.TurnOn(true);
            }

            foreach (SignalLightSequence sequence in _sequences)
            {
                sequence.Activate();
            }
        }

        protected static bool ApplyInvert(bool result, bool invert)
        {
            return invert ? !result : result;
        }
    }
}
