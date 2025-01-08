using Signals.Common;
using Signals.Game.States;
using System.Collections.Generic;
using UnityEngine;

namespace Signals.Game
{
    public class SignalController
    {
        private const float OptimiseDistance = 1500;
        private const float OptimiseDistanceSqr = OptimiseDistance * OptimiseDistance;
        private const float UpdateTime = 1.0f;
        private const float LongUpdateTime = 5.0f;

        public SignalControllerDefinition Definition;
        public Junction AssignedJunction;
        public bool TowardsSplit;

        private int _currentState = -1;

        public SignalStateBase[] AllStates { get; private set; }
        public SignalStateBase? CurrentState => _currentState >= 0 ? AllStates[_currentState] : null;
        public bool IsOn => CurrentState != null;
        public SignalLight[] AllLights { get; private set; }
        public string Name => $"{AssignedJunction.junctionData.junctionIdLong}-{(TowardsSplit ? "F" : "R")}";

        public SignalController(SignalControllerDefinition def, Junction junction, bool direction)
        {
            Definition = def;
            AssignedJunction = junction;
            TowardsSplit = direction;
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
            yield return new WaitForSeconds(UpdateTime);

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
            if (newState != _currentState && _currentState == -1 && newState <= -1)
            {
                return false;
            }

            // Out of range, ignore request. Maybe make them open the signal (last state)?
            if (newState >= AllStates.Length)
            {
                SignalsMod.Error($"Failed to set state on signal '{Name}': {newState} >= {AllStates.Length}");
                return false;
            }

            // Reset signal state.
            TurnOff();

            if (newState < 0)
            {
                // Object was turning off, which was already done.
                SignalsMod.LogVerbose($"Turning off signal '{Name}'");
                return true;
            }

            _currentState = newState;
            SignalsMod.LogVerbose($"Setting signal '{Name}' to state '{AllStates[newState].Definition.Id}'");
            CurrentState!.Apply();
            return true;
        }

        /// <summary>
        /// Turns off all lights.
        /// </summary>
        public void TurnOff()
        {
            foreach (var item in AllLights)
            {
                item.TurnOff();
            }

            _currentState = -1;
        }
    }
}
