using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class AlwaysActiveAspect : AspectBase<AlwaysActiveAspectDefinition>
    {
        public AlwaysActiveAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions() => true;
    }
}
