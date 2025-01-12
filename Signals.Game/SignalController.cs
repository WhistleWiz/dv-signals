﻿using Signals.Common;
using Signals.Game.Aspects;
using System.Collections.Generic;
using UnityEngine;

namespace Signals.Game
{
    /// <summary>
    /// Controls a signal instance.
    /// </summary>
    /// <remarks>
    /// Signal aspect is updated every second when a player is within 2km of the signal, and slows to
    /// an update every 5 seconds when further away.
    /// </remarks>
    public class SignalController
    {
        // Signals at over this distance from the camera update at a slower rate.
        // Can't completely stop updates or else signals reading states that are
        // far may be stuck at the wrong state.
        private const float OptimiseDistance = 2000;
        private const float OptimiseDistanceSqr = OptimiseDistance * OptimiseDistance;
        private const float UpdateTime = 1.0f;
        private const float LongUpdateTime = 5.0f;
        private const int OffState = -1;

        // Used to add artifical delays so that signals don't all update on the same frame.
        private static System.Random s_random = new System.Random();

        private bool _forceOff;
        private int? _baseAnimation;
        private SignalHover _hover;

        internal Coroutine? AnimatorDisabler;

        public SignalControllerDefinition Definition { get; private set; }
        public Junction AssignedJunction { get; private set; }
        /// <summary>
        /// Whether the signal refers to the junction's branches or the inbound track.
        /// </summary>
        public bool TowardsBranches { get; private set; }
        public int CurrentAspectIndex { get; private set; }
        public bool IsOn => CurrentAspectIndex >= 0;
        public AspectBase[] AllAspects { get; private set; }
        /// <summary>
        /// Returns <see langword="null"/> if the signal is off.
        /// </summary>
        public AspectBase? CurrentAspect => IsOn ? AllAspects[CurrentAspectIndex] : null;
        public SignalLight[] AllLights { get; private set; }
        public string Name => $"{AssignedJunction.junctionData.junctionIdLong}-{(TowardsBranches ? 'T' : 'F')}";
        /// <summary>
        /// Forces this signal to stay off.
        /// </summary>
        public bool ForceOff
        {
            get => _forceOff;
            set
            {
                _forceOff = value;
                UpdateAspect();
            }
        }

        public SignalController(SignalControllerDefinition def, Junction junction, bool direction)
        {
            _forceOff = false;
            Definition = def;
            AssignedJunction = junction;
            TowardsBranches = direction;
            CurrentAspectIndex = OffState;

            List<AspectBase> allStates = new List<AspectBase>();

            foreach (var item in def.OtherAspects)
            {
                var result = AspectCreator.Create(this, item);

                if (result == null) continue;

                allStates.Add(result);
            }

            allStates.Add(AspectCreator.Create(this, def.DefaultAspect)!);

            AllAspects = allStates.ToArray();
            AllLights = def.GetComponentsInChildren<SignalLight>(true);

            if (def.Animator != null)
            {
                _baseAnimation = def.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
                def.Animator.enabled = false;
            }

            _hover = def.GetComponent<SignalHover>();
            _hover.Initialise(def.OffStateHUDSprite);

            def.StartCoroutine(CheckRoutine());
            junction.Switched += JunctionSwitched;
        }

        private void JunctionSwitched(Junction.SwitchMode mode, int branch)
        {
            UpdateAspect();
        }

        private System.Collections.IEnumerator CheckRoutine()
        {
            TurnOff();

            // Wait for the player to load.
            while (!StartingItemsController.Instance.itemsLoaded)
            {
                yield return null;
            }

            // Prevent cluttering log by setting states too early.
            yield return new WaitForSeconds((float)(s_random.NextDouble() + 0.1));

            while (true)
            {
                UpdateAspect();
                yield return new WaitForSeconds(GetUpdateTime());
            }
        }

        private float GetUpdateTime()
        {
            if (PlayerManager.ActiveCamera != null &&
                Vector3.SqrMagnitude(Definition.transform.position - PlayerManager.ActiveCamera.transform.position) > OptimiseDistanceSqr)
            {
                return LongUpdateTime;
            }

            return UpdateTime;
        }

        /// <summary>
        /// Updates the current aspect based on the conditions of <see cref="AllAspects"/>.
        /// </summary>
        public void UpdateAspect()
        {
            if (ForceOff)
            {
                if (IsOn)
                {
                    TurnOff();
                }

                return;
            }

            // Precompute this information so each state doesn't have to call the same functions
            // over and over again.
            TrackWalker.GetTracksAndNextSignal(this, out var tracks, out var nextSignal);

            for (int i = 0; i < AllAspects.Length; i++)
            {
                if (AllAspects[i].MeetsConditions(tracks, nextSignal))
                {
                    ChangeAspect(i);
                    return;
                }
            }

            // This shouldn't be reached but just in case.
            ChangeAspect(AllAspects.Length - 1);
        }

        /// <summary>
        /// Changes the current state to a new one. Does nothing if the state is the same.
        /// </summary>
        /// <param name="newAspect">The index of the new state. Negative values turn off the signal.</param>
        public bool ChangeAspect(int newAspect)
        {
            // Check if the state changes. All negative numbers are treated as off.
            if (newAspect == CurrentAspectIndex || (!IsOn && newAspect < 0))
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
            _hover.UpdateStateDisplay(AllAspects[newAspect].Definition.HUDSprite);
            return true;
        }

        /// <summary>
        /// Turns off the signal until the next state update.
        /// </summary>
        /// <param name="keep">If true, keeps the signal off.</param>
        public void TurnOff(bool keep = false)
        {
            CurrentAspect?.Unapply();

            foreach (var item in AllLights)
            {
                item.TurnOff();
            }

            // Try to reset the animator state if it exists.
            if (Definition.Animator != null && _baseAnimation.HasValue)
            {
                Definition.Animator.enabled = true;
                Definition.Animator.CrossFadeInFixedTime(_baseAnimation.Value, CurrentAspect != null ? CurrentAspect.Definition.AnimationTime : UpdateTime, 0);
                DisableAnimator(UpdateTime + 0.1f);
            }

            CurrentAspectIndex = OffState;
            _hover.UpdateStateDisplay(Definition.OffStateHUDSprite);
            _forceOff = keep;
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
        /// Returns the other signal at the assigned junction.
        /// </summary>
        public SignalController GetPaired()
        {
            SignalManager.Instance.TryGetSignal(AssignedJunction, !TowardsBranches, out var controller);
            return controller;
        }
    }
}
