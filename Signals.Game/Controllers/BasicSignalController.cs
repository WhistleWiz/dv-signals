using Signals.Common;
using Signals.Game.Aspects;
using Signals.Game.Displays;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Signals.Game.Controllers
{
    public class BasicSignalController
    {
        // Used to add artifical delays so that signals don't all update on the same frame.
        private static System.Random s_random = new System.Random();

        protected const float UpdateTime = 1.0f;
        protected const float OptimiseDistanceSqr = 2500 * 2500;
        protected const int OffValue = -1;

        private int? _baseAnimation;
        private SignalHover _hover;
        private Coroutine? _opCoro;

        protected Coroutine? AnimatorDisabler;

        public SignalType Type = SignalType.NotSet;
        public string NameOverride = string.Empty;

        public SignalControllerDefinition Definition { get; private set; }
        public int CurrentAspectIndex { get; private set; }
        public AspectBase[] AllAspects { get; private set; }
        public SignalLight[] AllLights { get; private set; }
        public InfoDisplay[] AllDisplays { get; private set; }
        public AspectBase[] AllIndicators { get; private set; }
        public TrackInfo? TrackInfo { get; protected set; }

        public virtual string Name => NameOverride;
        public bool Exists => Definition != null;
        public bool IsOn => CurrentAspectIndex >= 0;
        /// <summary>
        /// Returns <see langword="null"/> if the signal is off.
        /// </summary>
        public AspectBase? CurrentAspect => IsOn ? AllAspects[CurrentAspectIndex] : null;

        public Action<AspectBase?>? OnAspectChanged;
        public Action<InfoDisplay[]>? OnDisplaysUpdated;

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

            // And finally the same for indicators.
            AllIndicators = def.Indicators.Select(x => AspectCreator.Create(this, x)).Where(x => x != null).ToArray()!;

            // If there's an animator, set up the default state
            if (def.Animator != null)
            {
                _baseAnimation = def.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
                def.Animator.enabled = false;
            }

            _hover = def.GetComponent<SignalHover>();
            _hover.Initialise(def.OffStateHUDSprite);

            _opCoro = Definition.StartCoroutine(OptimiseRoutine());

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

        // Used to disable child objects of the signal to increase performance.
        // The distance set is 2.5km, which is far enough that signals can realistically be seen,
        // but still not too far that it is useless.
        private System.Collections.IEnumerator OptimiseRoutine()
        {
            // Wait for the player to load.
            while (PlayerManager.ActiveCamera == null) yield return null;

            yield return GetStartDelay();

            while (Exists)
            {
                yield return new WaitForSeconds(UpdateTime);

                if (PlayerManager.ActiveCamera == null) continue;

                bool active = GetCameraDistanceSqr() <= OptimiseDistanceSqr;

                foreach (Transform t in Definition.transform)
                {
                    t.gameObject.SetActive(active);
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
        /// Calculates the squared distance from the signal to the camera.
        /// </summary>
        protected float GetCameraDistanceSqr()
        {
            return Helpers.DistanceSqr(Definition.transform.position, PlayerManager.ActiveCamera.transform.position);
        }

        /// <summary>
        /// Turns off the signal until the next state update.
        /// </summary>
        /// <param name="keep">If true, keeps the signal off.</param>
        /// <returns><see langword="true"/> if the signal was turned off successfuly, <see langword="false"/> otherwise.</returns>
        public bool TurnOff()
        {
            if (!IsOn) return false;

            CurrentAspect?.Unapply();

            foreach (var item in AllIndicators)
            {
                item.Unapply();
            }

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

            OnAspectChanged?.Invoke(null);
            return true;
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
                return TurnOff();
            }

            CurrentAspect?.Unapply();

            SignalsMod.LogVerbose($"Setting signal '{Name}' to state '{AllAspects[newAspect].Definition.Id}'");
            CurrentAspectIndex = newAspect;
            AllAspects[newAspect].Apply();

            OnAspectChanged?.Invoke(AllAspects[newAspect]);
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

        public void UpdateDisplays(bool aspectChanged)
        {
            foreach (var item in AllDisplays)
            {
                item.CheckAndUpdate(aspectChanged);
            }

            UpdateHoverDisplay();

            OnDisplaysUpdated?.Invoke(AllDisplays);
        }

        public void UpdateIndicators()
        {
            if (!IsOn) return;

            // Turn off all indicators first. This needs 2 loops to prevent
            // conflicts where one indicator turns another that was already
            // on, off.
            foreach (var item in AllIndicators)
            {
                item.Unapply();
            }

            foreach (var item in AllIndicators)
            {
                // Turn on the ones that meet conditions.
                if (item.MeetsConditions())
                {
                    item.Apply();
                }
            }
        }

        /// <summary>
        /// Update the signal automatically.
        /// </summary>
        public virtual void UpdateAspect()
        {
            bool changed;

            for (int i = 0; i < AllAspects.Length; i++)
            {
                if (AllAspects[i].MeetsConditions())
                {
                    changed = ChangeAspect(i);
                    goto Finalise;
                }
            }

            // Turn off if no conditions are met.
            changed = TurnOff();

            Finalise:

            // Update displays and indicators.
            UpdateDisplays(changed);
            UpdateIndicators();
        }

        protected static WaitForSeconds GetStartDelay()
        {
            return new WaitForSeconds((float)(s_random.NextDouble() + 0.1));
        }
    }
}
