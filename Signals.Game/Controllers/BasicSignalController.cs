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
        protected const float UpdateTime = 1.0f;
        protected const float OptimiseDistanceSqr = 2500 * 2500;
        protected const int OffValue = -1;

        private int? _baseAnimation;
        private SignalHover _hover;

        protected Coroutine? AnimatorDisabler;

        public SignalType Type = SignalType.NotSet;
        public bool IsOld;
        /// <summary>
        /// Override the name of this signal.
        /// </summary>
        public string NameOverride = string.Empty;
        /// <summary>
        /// The signal will not self update if this is true. Optimisation routines still execute.
        /// </summary>
        public bool ManualOperationOnly = false;

        public SignalControllerDefinition Definition { get; private set; }
        public int CurrentAspectIndex { get; private set; }
        public AspectBase[] AllAspects { get; private set; }
        public SignalLight[] AllLights { get; private set; }
        public InfoDisplay[] AllDisplays { get; private set; }
        public AspectBase[] AllIndicators { get; private set; }
        public TrackInfo? TrackInfo { get; protected set; }

        public virtual string Name => NameOverride;
        /// <summary>
        /// Is <see langword="true"/> if this signal exists in the world.
        /// </summary>
        public bool Exists => Definition != null;
        public bool IsOn => CurrentAspectIndex >= 0;
        /// <summary>
        /// Is <see langword="null"/> if the signal is off.
        /// </summary>
        public AspectBase? CurrentAspect => IsOn ? AllAspects[CurrentAspectIndex] : null;
        /// <summary>
        /// The position in the world of this signal.
        /// </summary>
        public Vector3 Position => Definition.transform.position;

        public Action<AspectBase?>? AspectChanged;
        public Action<InfoDisplay[]>? DisplaysUpdated;
        public Action<BasicSignalController>? Destroyed;

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

            TrackChecker.OnMapBuilt += FixPositionDueToCrossing;
            SignalManager.Instance.RegisterSignal(this);
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
        internal void Optimise()
        {
            if (PlayerManager.ActiveCamera == null) return;

            bool active = GetCameraDistanceSqr() <= OptimiseDistanceSqr;

            foreach (Transform t in Definition.transform)
            {
                t.gameObject.SetActive(active);
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
        /// Checks if it is safe to continue using this signal instance.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if further processing can be done, otherwise <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// If <see langword="false"/> is returned, this signal should be discarded and all further processing
        /// stopped. It is likely the instanced object assigned to it has been destroyed.
        /// </remarks>
        public bool SafetyCheck()
        {
            if (Exists)
            {
                return true;
            }

            Destroy();
            return false;
        }

        /// <summary>
        /// Destroys this signal (controller and game object).
        /// </summary>
        public void Destroy()
        {
            if (Definition != null)
            {
                UnityEngine.Object.Destroy(Definition.gameObject);
            }

            SignalManager.Instance.UnregisterSignal(this);
            Destroyed?.Invoke(this);
        }

        /// <summary>
        /// Turns off the signal until the next state update.
        /// </summary>
        /// <param name="keep">If true, keeps the signal off.</param>
        /// <returns><see langword="true"/> if the signal was turned off successfuly, <see langword="false"/> otherwise.</returns>
        /// <remarks>In case the aspect was turned off successfully, <see cref="AspectChanged"/> will be called.</remarks>
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

            AspectChanged?.Invoke(null);
            return true;
        }

        /// <summary>
        /// Changes the current aspect to a new one.
        /// </summary>
        /// <param name="newAspect">The index of the new aspect. Negative values turn off the signal.</param>
        /// <returns><see langword="true"/> if the aspect changed, <see langword="false"/> otherwise.</returns>
        /// <remarks>In case the aspect is successfully changed, <see cref="AspectChanged"/> will be called.</remarks>
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

            AspectChanged?.Invoke(AllAspects[newAspect]);
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

            DisplaysUpdated?.Invoke(AllDisplays);
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
        /// Whether the normal update cycle should be skipped or not.
        /// </summary>
        /// <returns></returns>
        public virtual bool ShouldSkipUpdate()
        {
            return ManualOperationOnly;
        }

        /// <summary>
        /// Updates the current aspect based on the conditions of <see cref="AllAspects"/>.
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
    }
}
