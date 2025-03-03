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
            if (ControllerTrackInfo == null) return false;

            foreach (var item in ControllerTrackInfo.Tracks)
            {
                if (item.IsOccupied(_fullDef.CrossingCheckMode))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
