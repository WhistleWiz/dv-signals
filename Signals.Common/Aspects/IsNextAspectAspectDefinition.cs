using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Is Next Aspect (Aspect)")]
    public class IsNextAspectAspectDefinition : AspectBaseDefinition
    {
        public string NextId = string.Empty;
        public bool Shunting = false;

        private void Reset()
        {
            Id = "NEXT_ASPECT";
        }
    }
}
