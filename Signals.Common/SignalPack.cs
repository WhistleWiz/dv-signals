using System.Collections.Generic;
using UnityEngine;

namespace Signals.Common
{
    [CreateAssetMenu(menuName = "DVSignals/Signal Pack")]
    public class SignalPack : ScriptableObject
    {
        public string ModId = string.Empty;
        public string ModName = string.Empty;
        public string Author = string.Empty;
        public string Version = "1.0.0";
        public string HomePage = string.Empty;
        public string Repository = string.Empty;

        [Space]
        public SignalControllerDefinition Signal = null!;
        public SignalControllerDefinition? IntoYardSignal = null;
        public SignalControllerDefinition? ShuntingSignal = null;

        public bool Validate()
        {
            if (Signal == null)
            {
                Debug.LogError("Controller is not set!", this);
                return false;
            }

            return true;
        }

        public IEnumerable<SignalControllerDefinition> AllSignals
        {
            get
            {
                yield return Signal;
                if (IntoYardSignal != null) yield return IntoYardSignal;
                if (ShuntingSignal != null) yield return ShuntingSignal;
            }
        }
    }
}
