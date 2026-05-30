using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class TrainDetectedAspect : AspectBase<TrainDetectedAspectDefinition>
    {
        public TrainDetectedAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var block = Block;

            return block != null && block.IsOccupied(Definition.CrossingCheckMode);
        }
    }
}
