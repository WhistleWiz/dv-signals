using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Departure (Aspect)")]
    public class DepartureAspectDefinition : AspectBaseDefinition
    {
        public bool IncludeShunting = false;
        public bool Invert = false;

        private void Reset()
        {
            Id = "DEPARTURE";
        }
    }
}
