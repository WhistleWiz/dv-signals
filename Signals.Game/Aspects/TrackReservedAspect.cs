using Signals.Common.Aspects;
using Signals.Game.Railway;

namespace Signals.Game.Aspects
{
    public class TrackReservedAspect : AspectBase
    {
        public TrackReservedAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            return TrackReserver.IsSignalReservedByAnother(Signal);
        }
    }
}
