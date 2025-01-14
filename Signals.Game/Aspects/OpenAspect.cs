using Signals.Common.Aspects;
using Signals.Game.Controllers;

namespace Signals.Game.Aspects
{
    internal class OpenAspect : AspectBase
    {
        public OpenAspect(AspectBaseDefinition def, BasicSignalController controller) : base(def, controller) { }

        public override bool MeetsConditions(WalkInfo _)
        {
            return true;
        }
    }
}
