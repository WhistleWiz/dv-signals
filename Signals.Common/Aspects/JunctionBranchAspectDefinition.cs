using UnityEngine;

namespace Signals.Common.Aspects
{
    public class JunctionBranchAspectDefinition : AspectBaseDefinition
    {
        [Space]
        [Tooltip("Aspect is active when the junction's selected branch index matches this value\n" +
            "0 is the leftmost branch")]
        public int ActiveOnBranch = 0;
    }
}
