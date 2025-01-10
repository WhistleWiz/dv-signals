using Signals.Common.States;

namespace Signals.Game.States
{
    internal class IsNextStateSignalState : SignalStateBase
    {
        private IsNextStateSignalStateDefinition _fullDef;

        public IsNextStateSignalState(SignalStateBaseDefinition def) : base(def)
        {
            _fullDef = (IsNextStateSignalStateDefinition)def;
        }

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

            return state.Id == _fullDef.NextId;
        }
    }
}
