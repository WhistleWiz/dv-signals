using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class AlwaysActiveAspect : AspectBase
    {
        public AlwaysActiveAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            return true;
        }
    }
}
