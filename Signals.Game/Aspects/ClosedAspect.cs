using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    internal class ClosedAspect : AspectBase
    {
        private ClosedAspectDefinition _fullDef;

        public ClosedAspect(AspectBaseDefinition def, SignalController controller) : base(def, controller)
        {
            _fullDef = (ClosedAspectDefinition)def;
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
