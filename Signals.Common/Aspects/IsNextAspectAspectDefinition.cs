using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Is Next Aspect")]
    public class IsNextAspectAspectDefinition : AspectBaseDefinition
    {
        public string NextId = string.Empty;

        private void Reset()
        {
            Id = "NEXT_ASPECT";
        }
    }
}
