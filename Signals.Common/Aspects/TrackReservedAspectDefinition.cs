using UnityEngine;

namespace Signals.Common.Aspects
{
    public class TrackReservedAspectDefinition : AspectBaseDefinition
    {
        [Space]
        [Tooltip("How tracks should be checked at diamond crossings\n" +
            " • Ignore: ignores crossings\n" +
            " • Whole Track: anything detected on any track intersecting at the crossing\n" +
            " • Intersection Only: same behaviour as Whole Track")]
        public CrossingCheckMode CrossingCheckMode;

        private void Reset()
        {
            Id = "TRACK_RESERVED";
            DisallowPassing = true;
        }
    }
}
