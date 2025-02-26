using Signals.Common;
using Signals.Game.Aspects;
using Signals.Game.Displays;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Signals.Game.Controllers
{
    public class BasicSignalController
    {
        protected const int OffValue = -1;

        private int? _baseAnimation;
        private SignalHover _hover;

        protected Coroutine? AnimatorDisabler;

        public SignalType Type = SignalType.NotSet;
        public string NameOverride = string.Empty;

        public SignalControllerDefinition Definition { get; private set; }
        public int CurrentAspectIndex { get; private set; }
        public AspectBase[] AllAspects { get; private set; }
        public SignalLight[] AllLights { get; private set; }
        public InfoDisplay[] AllDisplays { get; private set; }

        public virtual string Name => NameOverride;
        public bool Exists => Definition != null;
        public bool IsOn => CurrentAspectIndex >= 0;
        /// <summary>
        /// Returns <see langword="null"/> if the signal is off.
        /// </summary>
        public AspectBase? CurrentAspect => IsOn ? AllAspects[CurrentAspectIndex] : null;

        public BasicSignalController(SignalControllerDefinition def)
        {
            Definition = def;
            CurrentAspectIndex = OffValue;

            // Instantiate all aspect implementations.
            AllAspects = def.Aspects.Select(x => AspectCreator.Create(this, x)).Where(x => x != null).ToArray()!;

            // Get an array of all lights.
            AllLights = def.GetComponentsInChildren<SignalLight>(true);

            // Same as aspects but for displays.
            AllDisplays = def.Displays.Select(x => DisplayCreator.Create(this, x)).Where(x => x != null).ToArray()!;

            // If there's an animator, set up the default state
            if (def.Animator != null)
            {
                _baseAnimation = def.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
                def.Animator.enabled = false;
            }

            _hover = def.GetComponent<SignalHover>();
            _hover.Initialise(def.OffStateHUDSprite);

            TrackChecker.OnMapBuilt += FixPositionDueToCrossing;
        }

        private void FixPositionDueToCrossing(Dictionary<RailTrack, TrackChecker.TrackIntersectionPoints> junctionMap)
        {
            TrackChecker.OnMapBuilt -= FixPositionDueToCrossing;

            if (!Exists) return;

            foreach (var item in junctionMap)
            {
                foreach (var (_, Position) in item.Value.IntersectionPoints)
                {
                    if (Helpers.DistanceSqr(Definition.transform.position, Position.position) < 25.0f)
                    {
                        Definition.transform.position -= Definition.transform.forward * 5.0f;
                    }
                }
            }
        }

        internal void DisableAnimator(float time)
        {
            if (Definition.Animator == null) return;

            if (AnimatorDisabler != null)
            {
                Definition.StopCoroutine(AnimatorDisabler);
            }

            // Disable the animator after some time. Since the animations are instant
            AnimatorDisabler = Definition.StartCoroutine(Helpers.DisableBehaviour(Definition.Animator, time));
        }

        /// <summary>
        /// Turns off the signal until the next state update.
        /// </summary>
        /// <param name="keep">If true, keeps the signal off.</param>
        public void TurnOff()
        {
            if (!IsOn) return;

            CurrentAspect?.Unapply();

            foreach (var item in AllLights)
            {
                item.TurnOff();
            }

            // Try to reset the animator state if it exists.
            if (Definition.Animator != null && _baseAnimation.HasValue)
            {
                var time = CurrentAspect != null ? CurrentAspect.Definition.AnimationTime : 1;
                Definition.Animator.enabled = true;
                Definition.Animator.CrossFadeInFixedTime(_baseAnimation.Value, time, 0);
                DisableAnimator(time + 0.1f);
            }

            CurrentAspectIndex = OffValue;
            UpdateDisplays();
        }

        /// <summary>
        /// Changes the current aspect to a new one.
        /// </summary>
        /// <param name="newAspect">The index of the new aspect. Negative values turn off the signal.</param>
        /// <returns><see langword="true"/> if the aspect changed, <see langword="false"/> otherwise.</returns>
        /// <remarks>Does nothing if the aspect is the same.</remarks>
        public bool ChangeAspect(int newAspect)
        {
            // Check if the state changes. All negative numbers are treated as off.
            if (newAspect == CurrentAspectIndex || !IsOn && newAspect < 0)
            {
                return false;
            }

            // Out of range, ignore request. Maybe make them open the signal (last state)?
            if (newAspect >= AllAspects.Length)
            {
                SignalsMod.Error($"Failed to set state on signal '{Name}': {newAspect} >= {AllAspects.Length}");
                return false;
            }

            // Turn off on negative numbers.
            if (newAspect < 0)
            {
                SignalsMod.LogVerbose($"Turning off signal '{Name}'");
                TurnOff();
                return true;
            }

            CurrentAspect?.Unapply();

            SignalsMod.LogVerbose($"Setting signal '{Name}' to state '{AllAspects[newAspect].Definition.Id}'");
            CurrentAspectIndex = newAspect;
            AllAspects[newAspect].Apply();
            UpdateDisplays();
            return true;
        }

        public void UpdateHoverDisplay()
        {
            if (IsOn)
            {
                // Current aspect is not null when the signal is on.
                _hover.UpdateStateDisplay(this, CurrentAspect!.Definition.HUDSprite);
            }
            else
            {
                _hover.UpdateStateDisplay(this, Definition.OffStateHUDSprite);
            }
        }

        public void UpdateDisplays()
        {
            foreach (var item in AllDisplays)
            {
                item.UpdateDisplay();
            }

            UpdateHoverDisplay();
        }

        /// <summary>
        /// Update the signal automatically.
        /// </summary>
        /// <returns>
        /// The next signal found, <see langword="null"/> if there's no next signal.
        /// </returns>
        public virtual void UpdateAspect() { }
    }
}
