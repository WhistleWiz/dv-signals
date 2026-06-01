using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Special Matching Path (Aspect)")]
    public class SpecialMatchingPathAspectDefinition : MatchingPathAspectDefinition
    {
        private void Reset()
        {
            Id = "SPECIAL_MATCHING_PATH";
        }
    }
}
