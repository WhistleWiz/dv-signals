using Signals.Common;
using Signals.Game.States;
using System.Collections.Generic;
using UnityEngine;

namespace Signals.Game
{
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

        internal Coroutine? AnimatorDisabler;

        public SignalControllerDefinition Definition { get; private set; }
        public Junction AssignedJunction { get; private set; }
        /// <summary>
        /// Whether the signal refers to the junction's branches or the inbound track.
        /// </summary>
        public bool TowardsBranches { get; private set; }
        public int CurrentStateIndex { get; private set; }
        public bool IsOn => CurrentStateIndex >= 0;
        public SignalStateBase[] AllStates { get; private set; }
        /// <summary>
        /// Returns <see langword="null"/> if the signal is off.
        /// </summary>
        public SignalStateBase? CurrentState => IsOn ? AllStates[CurrentStateIndex] : null;
        public SignalLight[] AllLights { get; private set; }
        public string Name => $"{AssignedJunction.junctionData.junctionIdLong}-{(TowardsBranches ? "F" : "R")}";
        /// <summary>
        /// Forces this signal to stay off.
        /// </summary>
        public bool ForceOff
        {
            get => _forceOff;
            set
            {
                _forceOff = value;
                UpdateState();
            }
        }

        public SignalController(SignalControllerDefinition def, Junction junction, bool direction)
        {
            Definition = def;
            AssignedJunction = junction;
            TowardsBranches = direction;
            ForceOff = false;
            CurrentStateIndex = OffState;

            List<SignalStateBase> allStates = new List<SignalStateBase>();

            foreach (var item in def.OtherStates)
            {
                var result = SignalCreator.Create(this, item);

                if (result == null) continue;

                allStates.Add(result);
            }

            allStates.Add(SignalCreator.Create(this, def.OpenState)!);

            AllStates = allStates.ToArray();
            AllLights = def.GetComponentsInChildren<SignalLight>(true);

            if (def.Animator != null)
            {
                _baseAnimation = def.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
                def.Animator.enabled = false;
            }

            def.StartCoroutine(CheckRoutine());
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
                UpdateState();
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
        /// Updates the current state.
        /// </summary>
        public void UpdateState()
        {
            if (ForceOff)
            {
                if (IsOn)
                {
                    TurnOff();
                }

                return;
            }

            for (int i = 0; i < AllStates.Length; i++)
            {
                if (AllStates[i].MeetsConditions())
                {
                    ChangeState(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Changes the current state to a new one. Does nothing if the state is the same.
        /// </summary>
        /// <param name="newState">The index of the new state. Negative values turn off the signal.</param>
        public bool ChangeState(int newState)
        {
            // Check if the state changes. All negative numbers are treated as off.
            if (newState == CurrentStateIndex || (!IsOn && newState < 0))
            {
                return false;
            }

            // Out of range, ignore request. Maybe make them open the signal (last state)?
            if (newState >= AllStates.Length)
            {
                SignalsMod.Error($"Failed to set state on signal '{Name}': {newState} >= {AllStates.Length}");
                return false;
            }

            // Turn off on negative numbers.
            if (newState < 0)
            {
                SignalsMod.LogVerbose($"Turning off signal '{Name}'");
                TurnOff();
                return true;
            }

            CurrentState?.Unapply();

            SignalsMod.LogVerbose($"Setting signal '{Name}' to state '{AllStates[newState].Definition.Id}'");
            CurrentStateIndex = newState;
            AllStates[newState].Apply();
            return true;
        }

        /// <summary>
        /// Turns off the signal until the next state update.
        /// </summary>
        public void TurnOff()
        {
            foreach (var item in AllLights)
            {
                item.TurnOff();
            }

            // Try to reset the animator state if it exists.
            if (Definition.Animator != null && _baseAnimation.HasValue)
            {
                Definition.Animator.enabled = true;
                Definition.Animator.CrossFade(_baseAnimation.Value, UpdateTime / 2, 0);
                DisableAnimator(UpdateTime);
            }

            CurrentStateIndex = OffState;
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
    }
}
