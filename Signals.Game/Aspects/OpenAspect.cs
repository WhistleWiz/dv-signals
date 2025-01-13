using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    internal class OpenAspect : AspectBase
    {
        public OpenAspect(AspectBaseDefinition def, SignalController controller) : base(def, controller) { }

        public override bool MeetsConditions(RailTrack[] tracksToNextSignal, SignalController? nextSignal)
        {
            return true;
        }
    }
}
