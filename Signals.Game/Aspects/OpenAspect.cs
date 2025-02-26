using Signals.Common.Aspects;
using Signals.Game.Controllers;

namespace Signals.Game.Aspects
{
    internal class OpenAspect : AspectBase
    {
        public OpenAspect(AspectBaseDefinition definition, BasicSignalController controller) : base(definition, controller) { }

        public override bool MeetsConditions(WalkInfo _)
        {
            return true;
        }
    }
}
