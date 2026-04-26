using Signals.Common.Aspects;
using System.Linq;

namespace Signals.Game.Aspects
{
    public class IsNextAspectAnyAspect : AspectBase
    {
        private IsNextAspectAnyAspectDefinition _fullDef;

        public IsNextAspectAnyAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal)
        {
            _fullDef = (IsNextAspectAnyAspectDefinition)definition;
        }

        public override bool MeetsConditions()
        {
            var next = Signal.GetNextController();

            if (next == null) return false;

            var signal = next.GetControllerSignal();

            if (signal == null) return false;

            var state = signal.CurrentAspect;

            // Turned off signal can never meet conditions.
            return state != null && _fullDef.NextIds.Contains(state.Id);
        }
    }
}
