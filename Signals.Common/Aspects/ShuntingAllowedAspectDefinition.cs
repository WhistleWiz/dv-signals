using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Shunting Allowed (Aspect)")]
    public class ShuntingAllowedAspectDefinition : AspectBaseDefinition
    {
        public bool Invert = false;

        private void Reset()
        {
            Id = "SHUNTING_ALLOWED";
        }
    }
}
