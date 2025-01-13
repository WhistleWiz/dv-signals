using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    internal class IsNextAspectAspect : AspectBase
    {
        private IsNextAspectAspectDefinition _fullDef;

        public IsNextAspectAspect(AspectBaseDefinition def, SignalController controller) : base(def, controller)
        {
            _fullDef = (IsNextAspectAspectDefinition)def;
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
