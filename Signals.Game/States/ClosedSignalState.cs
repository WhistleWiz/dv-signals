using Signals.Common.States;
using System.Linq;

namespace Signals.Game.States
{
    internal class ClosedSignalState : SignalStateBase
    {
        public ClosedSignalState(SignalStateBaseDefinition def, SignalController controller) : base(def, controller) { }

        public override bool MeetsConditions(RailTrack[] tracksToNextSignal, SignalController? nextSignal)
        {
            foreach (var item in tracksToNextSignal)
            {
                if (item.onTrackBogies.Count() > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
