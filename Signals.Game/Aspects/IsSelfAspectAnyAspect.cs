using Signals.Common.Aspects;
using System.Linq;

namespace Signals.Game.Aspects
{
    public class IsSelfAspectAnyAspect : AspectBase<IsSelfAspectAnyAspectDefinition>
    {
        public IsSelfAspectAnyAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var state = Signal.CurrentAspect;

            // Turned off signal can never meet conditions.
            return state != null && Definition.SelfIds.Contains(state.Id);
        }
    }
}
