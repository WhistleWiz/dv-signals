using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    internal class OpenSignalAspect : SignalAspectBase
    {
        public OpenSignalAspect(SignalAspectBaseDefinition def, SignalController controller) : base(def, controller) { }

        public override bool MeetsConditions(RailTrack[] tracksToNextSignal, SignalController? nextSignal)
        {
            return true;
        }
    }
}
