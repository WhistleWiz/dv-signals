using Signals.Common.Aspects;
using Signals.Game.Controllers;

namespace Signals.Game.Aspects
{
    internal class ClosedAspect : AspectBase
    {
        private ClosedAspectDefinition _fullDef;

        public ClosedAspect(AspectBaseDefinition def, BasicSignalController controller) : base(def, controller)
        {
            _fullDef = (ClosedAspectDefinition)def;
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
