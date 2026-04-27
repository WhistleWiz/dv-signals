using Signals.Common.Aspects;
using Signals.Game.Railway;

namespace Signals.Game.Aspects
{
    public class TrackReservedAspect : AspectBase
    {
        private TrackReservedAspectDefinition _fullDef;

        public TrackReservedAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal)
        {
            _fullDef = (TrackReservedAspectDefinition)definition;
        }

        public override bool MeetsConditions()
        {
            var another = TrackReserver.IsSignalReservedByAnother(Signal);

            return _fullDef.Invert ? !another && TrackReserver.HasReservation(Signal) : another;
        }
    }
}
