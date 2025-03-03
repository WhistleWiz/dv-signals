using UnityEngine;

namespace Signals.Common.Aspects
{
    public class TrainDetectedAspectDefinition : AspectBaseDefinition
    {
        [Space]
        [Tooltip("How tracks should be checked at diamond crossings\n" +
            " • Ignore: ignores crossings\n" +
            " • Whole Track: anything detected on any track intersecting at the crossing\n" +
            " • Intersection Only: ignores other tracks except on the intersection itself")]
        public CrossingCheckMode CrossingCheckMode;

        private void Reset()
        {
            Id = "TRAIN_DETECTED";
        }
    }
}
