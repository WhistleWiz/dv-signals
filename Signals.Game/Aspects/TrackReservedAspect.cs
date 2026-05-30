using Signals.Common.Aspects;
using Signals.Game.Railway;

namespace Signals.Game.Aspects
{
    public class TrackReservedAspect : AspectBase<TrackReservedAspectDefinition>
    {
        public TrackReservedAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var another = TrackReserver.IsSignalReservedByAnother(Signal);

            return Definition.Invert ? !another && TrackReserver.HasReservation(Signal) : another;
        }
    }
}
