using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Track Reserved (Aspect)")]
    public class TrackReservedAspectDefinition : AspectBaseDefinition
    {
        public bool Invert = false;

        private void Reset()
        {
            Id = "TRACK_RESERVED";
            DisallowPassing = true;
        }
    }
}
