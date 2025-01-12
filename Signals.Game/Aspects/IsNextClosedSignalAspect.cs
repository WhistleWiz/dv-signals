using Signals.Common;
using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class IsNextClosedSignalAspect : SignalAspectBase
    {
        public IsNextClosedSignalAspect(SignalAspectBaseDefinition def, SignalController controller) : base(def, controller) { }

        public override bool MeetsConditions(RailTrack[] tracksToNextSignal, SignalController? nextSignal)
        {
            if (nextSignal == null)
            {
                return false;
            }

            var state = nextSignal.CurrentAspect;

            if (state == null)
            {
                return false;
            }

            return state.Id == Constants.SignalIds.Closed;
        }
    }
}
