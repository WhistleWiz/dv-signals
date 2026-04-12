using Signals.Common.Aspects;
using Signals.Game.Controllers;
using Signals.Game.Railway;

namespace Signals.Game.Aspects
{
    public class TrackReservedAspect : AspectBase
    {
        private TrackReservedAspectDefinition _fullDef;

        public TrackReservedAspect(AspectBaseDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (TrackReservedAspectDefinition)definition;
        }

        public override bool MeetsConditions()
        {
            return TrackReserver.IsSignalReservedByAnother(Controller, _fullDef.CrossingCheckMode);
        }
    }
}
