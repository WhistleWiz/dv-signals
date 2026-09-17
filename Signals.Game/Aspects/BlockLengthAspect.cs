using Signals.Common;
using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class BlockLengthAspect : AspectBase<BlockLengthAspectDefinition>
    {
        public BlockLengthAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var block = Block;

            if (block == null) return false;

            return Definition.OperationMode switch
            {
                OperationMode.EqualTo => block.Length == Definition.Length,
                OperationMode.DifferentFrom => block.Length != Definition.Length,
                OperationMode.LessThan => block.Length < Definition.Length,
                OperationMode.LessThanOrEqualTo => block.Length <= Definition.Length,
                OperationMode.GreaterThan => block.Length > Definition.Length,
                OperationMode.GreaterThanOrEqualTo => block.Length >= Definition.Length,
                _ => false,
            };
        }
    }
}
