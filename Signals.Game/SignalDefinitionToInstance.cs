using UnityEngine;

namespace Signals.Game
{
    internal class SignalDefinitionToInstance : MonoBehaviour
    {
        public Signal Signal = null!;

        public static SignalDefinitionToInstance? AddToDef(Signal signal)
        {
            var def = signal.Definition;

            if (def == null)
            {
                Debug.LogError($"Definition for signal {signal.Id} is null!");
                return null;
            }

            var comp = def.gameObject.AddComponent<SignalDefinitionToInstance>();
            comp.Signal = signal;
            return comp;
        }
    }
}
