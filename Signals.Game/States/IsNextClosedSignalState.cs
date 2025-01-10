using Signals.Common;
using Signals.Common.States;

namespace Signals.Game.States
{
    public class IsNextClosedSignalState : SignalStateBase
    {
        public IsNextClosedSignalState(SignalStateBaseDefinition def) : base(def) { }

        public override bool MeetsConditions(RailTrack[] tracksToNextSignal, SignalController? nextSignal)
        {
            if (nextSignal == null)
            {
                return false;
            }

            var state = nextSignal.CurrentState;

            if (state == null)
            {
                return false;
            }

            return state.Id == Constants.SignalIds.Closed;
        }
    }
}
