using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Is Logic Yard (Aspect)")]
    public class IsLogicYardAspectDefinition : AspectBaseDefinition
    {
        public bool Invert;

        private void Reset()
        {
            Id = "LOGIC_YARD";
        }
    }
}
