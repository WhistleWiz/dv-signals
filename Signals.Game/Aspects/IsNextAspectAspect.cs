using Signals.Common.Aspects;
using Signals.Game.Controllers;

namespace Signals.Game.Aspects
{
    internal class IsNextAspectAspect : AspectBase
    {
        private IsNextAspectAspectDefinition _fullDef;

        public IsNextAspectAspect(AspectBaseDefinition def, BasicSignalController controller) : base(def, controller)
        {
            _fullDef = (IsNextAspectAspectDefinition)def;
        }

        public override bool MeetsConditions(WalkInfo info)
        {
            if (info.NextMainlineSignal == null)
            {
                return false;
            }

            var state = info.NextMainlineSignal.CurrentAspect;

            if (state == null)
            {
                return false;
            }

            return state.Id == _fullDef.NextId;
        }
    }
}
