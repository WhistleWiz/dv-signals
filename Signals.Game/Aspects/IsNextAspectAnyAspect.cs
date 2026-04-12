using Signals.Common.Aspects;
using Signals.Game.Controllers;
using System.Linq;

namespace Signals.Game.Aspects
{
    public class IsNextAspectAnyAspect : AspectBase
    {
        private IsNextAspectAnyAspectDefinition _fullDef;

        public IsNextAspectAnyAspect(AspectBaseDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (IsNextAspectAnyAspectDefinition)definition;
        }

        public override bool MeetsConditions()
        {
            var next = Controller.GetNextSignal();

            if (next == null) return false;

            var state = next.CurrentAspect;

            // Turned off signal can never meet conditions.
            return state != null && _fullDef.NextIds.Contains(state.Id);
        }
    }
}
