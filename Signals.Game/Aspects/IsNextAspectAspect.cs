using Signals.Common.Aspects;
using Signals.Game.Controllers;

namespace Signals.Game.Aspects
{
    internal class IsNextAspectAspect : AspectBase
    {
        private IsNextAspectAspectDefinition _fullDef;

        public IsNextAspectAspect(AspectBaseDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (IsNextAspectAspectDefinition)definition;
        }

        public override bool MeetsConditions()
        {
            if (ControllerTrackInfo == null || ControllerTrackInfo.NextMainlineSignal == null) return false;

            var state = ControllerTrackInfo.NextMainlineSignal.CurrentAspect;

            if (state == null)
            {
                return false;
            }

            return state.Id == _fullDef.NextId;
        }
    }
}
