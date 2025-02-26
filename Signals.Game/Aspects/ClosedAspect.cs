using Signals.Common.Aspects;
using Signals.Game.Controllers;

namespace Signals.Game.Aspects
{
    internal class ClosedAspect : AspectBase
    {
        private ClosedAspectDefinition _fullDef;

        public ClosedAspect(AspectBaseDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (ClosedAspectDefinition)definition;
        }

        public override bool MeetsConditions(WalkInfo info)
        {
            foreach (var item in info.Tracks)
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
