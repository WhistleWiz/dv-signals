using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    internal class ClosedSignalAspect : SignalAspectBase
    {
        private ClosedSignalAspectDefinition _fullDef;

        public ClosedSignalAspect(SignalAspectBaseDefinition def, SignalController controller) : base(def, controller)
        {
            _fullDef = (ClosedSignalAspectDefinition)def;
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
