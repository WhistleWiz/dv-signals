using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class IsParentAspectAspect : AspectBase<IsParentAspectAspectDefinition>
    {
        public IsParentAspectAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var parent = Signal.Parent;

            if (parent == null) return false;

            var state = parent.CurrentAspect;

            // Turned off signal can never meet conditions.
            return state != null && state.Id == Definition.ParentId;
        }
    }
}
