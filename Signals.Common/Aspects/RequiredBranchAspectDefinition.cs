using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Required Branch (Aspect)")]
    public class RequiredBranchAspectDefinition : AspectBaseDefinition
    {
        private void Reset()
        {
            Id = "REQUIRED_BRANCH";
        }
    }
}
