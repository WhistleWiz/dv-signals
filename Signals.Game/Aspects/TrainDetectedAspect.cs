using Signals.Common.Aspects;
using Signals.Game.Controllers;

namespace Signals.Game.Aspects
{
    internal class TrainDetectedAspect : AspectBase
    {
        private TrainDetectedAspectDefinition _fullDef;

        public TrainDetectedAspect(AspectBaseDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (TrainDetectedAspectDefinition)definition;
        }

        public override bool MeetsConditions()
        {
            var block = ControllerTrackBlock;

            return block != null && block.IsOccupied(_fullDef.CrossingCheckMode);
        }
    }
}
