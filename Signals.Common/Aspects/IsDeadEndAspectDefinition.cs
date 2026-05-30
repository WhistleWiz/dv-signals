using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Is Dead End (Aspect)")]
    public class IsDeadEndAspectDefinition : AspectBaseDefinition
    {
        public bool IncludeSelfLoops = false;

        private void Reset()
        {
            Id = "IS_DEAD_END";
        }
    }
}
