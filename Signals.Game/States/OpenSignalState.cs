using Signals.Common.States;

namespace Signals.Game.States
{
    internal class OpenSignalState : SignalStateBase
    {
        public OpenSignalState(SignalStateBaseDefinition def, SignalController controller) : base(def, controller) { }

        public override bool MeetsConditions(RailTrack[] tracksToNextSignal, SignalController? nextSignal)
        {
            return true;
        }
    }
}
