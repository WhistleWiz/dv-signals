using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Track Reserved (Aspect)")]
    public class TrackReservedAspectDefinition : AspectBaseDefinition
    {
        private void Reset()
        {
            Id = "TRACK_RESERVED";
            DisallowPassing = true;
        }
    }
}
