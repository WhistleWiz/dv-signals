
using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Next Allows Passing (Aspect)")]
    public class NextAllowsPassingAspectDefinition : AspectBaseDefinition
    {
        public bool Invert = true;
        public bool Shunting = false;
        [Tooltip("What result to output if there is no next signal or aspect")]
        public bool NoAspectResult = false;

        private void Reset()
        {
            Id = "NEXT_DISALLOWS_PASSING";
        }
    }
}
