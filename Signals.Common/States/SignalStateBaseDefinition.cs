using UnityEngine;

namespace Signals.Common.States
{
    public abstract class SignalStateBaseDefinition : MonoBehaviour
    {
        public abstract string Id { get; }

        public SignalLightDefinition[] OnLights = new SignalLightDefinition[0];
        public SignalLightDefinition[] OffLights = new SignalLightDefinition[0];
        public SignalLightDefinition[] BlinkingLights = new SignalLightDefinition[0];
    }
}
