using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Turntable Connected (Aspect)")]
    public class TurntableConnectedAspectDefinition : AspectBaseDefinition
    {
        public bool Invert;

        private void Reset()
        {
            Id = "TURNTABLE_CONNECTED";
        }
    }
}
