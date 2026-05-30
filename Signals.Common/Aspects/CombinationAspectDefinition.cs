using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Combination (Aspect)")]
    public class CombinationAspectDefinition : AspectBaseDefinition
    {
        public AspectBaseDefinition[] Conditions = new AspectBaseDefinition[0];
        [Tooltip("If true, only one of the aspects in the conditions needs to be true")]
        public bool Any = false;
    }
}
