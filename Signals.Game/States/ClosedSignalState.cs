using Signals.Common.States;

namespace Signals.Game.States
{
    internal class ClosedSignalState : SignalStateBase
    {
        private ClosedSignalStateDefinition _fullDef;

        public ClosedSignalState(SignalStateBaseDefinition def, SignalController controller) : base(def, controller)
        {
            _fullDef = (ClosedSignalStateDefinition)def;
        }

        public override bool MeetsConditions(RailTrack[] tracksToNextSignal, SignalController? nextSignal)
        {
            foreach (var item in tracksToNextSignal)
            {
                if (item.IsOccupied(_fullDef.CrossingCheckMode))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
