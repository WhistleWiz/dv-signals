using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class OpenAspect : AspectBase
    {
        public OpenAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            return true;
        }
    }
}
