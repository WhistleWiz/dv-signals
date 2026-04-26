using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Matching Branch (Aspect)")]
    public class MatchingBranchAspectDefinition : AspectBaseDefinition
    {
        [Tooltip("If true, activates when the junction's branch isn't the one for the controller")]
        public bool Invert = true;
    }
}
