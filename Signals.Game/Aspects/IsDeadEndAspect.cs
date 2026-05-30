using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class IsDeadEndAspect : AspectBase<IsDeadEndAspectDefinition>
    {
        public IsDeadEndAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var block = Block;

            if (block == null) return false;

            if (Definition.IncludeSelfLoops && block.IsSelfLoop) return true;

            return block.IsDeadEnd;
        }
    }
}
