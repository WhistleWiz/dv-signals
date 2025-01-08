using DV.Utils;
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

        public SignalStateBase[] AllStates { get; private set; }
        public SignalStateBase? CurrentState { get; private set; }

        public SignalController(SignalControllerDefinition def, Junction junction, bool direction)
        {
            Definition = def;
            AssignedJunction = junction;
            TowardsSplit = direction;
            List<SignalStateBase> allStates = new List<SignalStateBase>();

            foreach (var item in def.OtherStates)
            {
                var result = SignalCreator.Create(this, item);

                if (result == null)
                {
                    SignalsMod.Error($"Failed to find creator function for state '{item.Id}'");
                    continue;
                }

                allStates.Add(result);
            }

            allStates.Add(SignalCreator.Create(this, def.OpenState)!);

            AllStates = allStates.ToArray();
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

        public void UpdateState()
        {
            foreach (var item in AllStates)
            {
                if (item.MeetsConditions())
                {
                    ChangeState(item);
                    break;
                }
            }
        }

        public void ChangeState(SignalStateBase newState)
        {
            if (newState == CurrentState)
            {
                return;
            }

            SignalsMod.LogVerbose($"Setting signal '{AssignedJunction.junctionData.junctionIdLong}' to state {newState.Definition.Id}");
            CurrentState = newState;
            CurrentState.Apply();
        }

        public void TurnOff()
        {
            foreach (var item in Definition.GetComponentsInChildren<SignalLight>())
            {
                item.TurnOff();
            }
        }
    }
}
