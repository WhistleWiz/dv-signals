using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class ShuntingAllowedAspect : AspectBase<ShuntingAllowedAspectDefinition>
    {
        public ShuntingAllowedAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            return Definition.Invert ? !Signal.ShuntingAllowed : Signal.ShuntingAllowed;
        }
    }
}
