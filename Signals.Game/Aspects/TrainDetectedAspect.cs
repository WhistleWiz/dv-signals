using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class TrainDetectedAspect : AspectBase
    {
        private TrainDetectedAspectDefinition _fullDef;

        public TrainDetectedAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal)
        {
            _fullDef = (TrainDetectedAspectDefinition)definition;
        }

        public override bool MeetsConditions()
        {
            var block = Block;

            return block != null && block.IsOccupied(_fullDef.CrossingCheckMode);
        }
    }
}
