using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Matching Path (Aspect)")]
    public class MatchingPathAspectDefinition : AspectBaseDefinition
    {
        public bool Invert = true;

        private void Reset()
        {
            Id = "MATCHING_PATH";
        }
    }
}
