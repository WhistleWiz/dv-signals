using UnityEngine;

namespace Signals.Common.Aspects
{
    public class ClosedSignalAspectDefinition : SignalAspectBaseDefinition
    {
        [Tooltip("How tracks should be checked at diamond crossings\n" +
            " • None: ignores crossings\n" +
            " • Whole Track: anything detected on any track intersecting at the crossing\n" +
            " • Intersection Only: ignores other tracks except on the intersection itself")]
        public CrossingCheckMode CrossingCheckMode;

        public override string Id => Constants.SignalIds.Closed;
    }
}
