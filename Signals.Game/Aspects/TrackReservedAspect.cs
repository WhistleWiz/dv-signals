using Signals.Common.Aspects;
using Signals.Game.Railway;

namespace Signals.Game.Aspects
{
    public class TrackReservedAspect : AspectBase<TrackReservedAspectDefinition>
    {
        public TrackReservedAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var reserved = Definition.BySelf ? TrackReserver.HasReservation(Signal) : TrackReserver.IsSignalReservedByAnother(Signal);

            return ApplyInvert(reserved, Definition.Invert);
        }
    }
}
