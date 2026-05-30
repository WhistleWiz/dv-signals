using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Is Self Aspect Any (Aspect)")]
    public class IsSelfAspectAnyAspectDefinition : AspectBaseDefinition
    {
        public string[] SelfIds = new string[0];

        private void Reset()
        {
            Id = "SELF_ANY_ASPECT";
        }
    }
}
