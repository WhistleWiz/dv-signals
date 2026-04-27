using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Always Active (Aspect)")]
    public class AlwaysActiveAspectDefinition : AspectBaseDefinition
    {
        private void Reset()
        {
            Id = "OPEN";
        }
    }
}
