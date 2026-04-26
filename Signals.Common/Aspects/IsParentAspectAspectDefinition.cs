using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Is Parent Aspect (Aspect)")]
    public class IsParentAspectAspectDefinition : AspectBaseDefinition
    {
        public string ParentId = string.Empty;

        private void Reset()
        {
            Id = "PARENT_ASPECT";
        }
    }
}
