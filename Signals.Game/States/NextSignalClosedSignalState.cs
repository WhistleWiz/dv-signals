using Signals.Common;
using Signals.Common.States;

namespace Signals.Game.States
{
    public class NextSignalClosedSignalState : SignalStateBase
    {
        public NextSignalClosedSignalState(SignalStateBaseDefinition def) : base(def) { }

        public override bool MeetsConditions()
        {
            var controller = TrackWalker.GetNextSignal(Controller);

            if (controller == null)
            {
                return false;
            }

            var state = controller.CurrentState;

            if (state == null)
            {
                return false;
            }

            return state.Id == Constants.SignalIds.Closed;
        }
    }
}
