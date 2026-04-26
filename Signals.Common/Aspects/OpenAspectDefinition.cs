using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Open (Aspect)")]
    public class OpenAspectDefinition : AspectBaseDefinition
    {
        private void Reset()
        {
            Id = "OPEN";
        }
    }
}
