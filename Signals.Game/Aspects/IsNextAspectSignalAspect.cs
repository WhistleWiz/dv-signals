using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    internal class IsNextAspectSignalAspect : SignalAspectBase
    {
        private IsNextAspectSignalAspectDefinition _fullDef;

        public IsNextAspectSignalAspect(SignalAspectBaseDefinition def, SignalController controller) : base(def, controller)
        {
            _fullDef = (IsNextAspectSignalAspectDefinition)def;
        }

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

            return state.Id == _fullDef.NextId;
        }
    }
}
