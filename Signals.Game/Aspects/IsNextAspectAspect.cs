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
            var next = Controller.GetNextSignal();

            if (next == null) return false;

            var state = next.CurrentAspect;

            // Turned off signal can never meet conditions.
            return state != null && state.Id == _fullDef.NextId;
        }
    }
}
