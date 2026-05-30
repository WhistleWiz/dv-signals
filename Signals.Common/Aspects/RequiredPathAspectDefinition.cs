using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Required Path (Aspect)")]
    public class RequiredPathAspectDefinition : AspectBaseDefinition
    {
        public bool Invert = true;

        private void Reset()
        {
            Id = "REQUIRED_PATH";
        }
    }
}
