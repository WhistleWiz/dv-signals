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
        public float OffsetFromTrackCentre = 2.0f;

        public bool Validate()
        {
            if (Signal == null)
            {
                Debug.LogError("Controller is not set!", this);
                return false;
            }

            if (Signal.DefaultAspect == null)
            {
                Debug.LogError("Default state is not set!", this);
                return false;
            }

            return true;
        }

        public IEnumerable<SignalControllerDefinition> AllSignals
        {
            get
            {
                yield return Signal;
            }
        }
    }
}
