using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Is Next Aspect Any (Aspect)")]
    public class IsNextAspectAnyAspectDefinition : AspectBaseDefinition
    {
        public string[] NextIds = new string[0];

        private void Reset()
        {
            Id = "NEXT_ANY_ASPECT";
        }
    }
}
