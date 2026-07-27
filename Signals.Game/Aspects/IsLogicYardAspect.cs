using Signals.Common.Aspects;
using Signals.Game.Generation;
using System.Linq;

namespace Signals.Game.Aspects
{
    public class IsLogicYardAspect : AspectBase<IsLogicYardAspectDefinition>
    {
        public IsLogicYardAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var block = Block;

            if (block == null) return false;

            return ApplyInvert(block.Tracks.Any(x => SignalPlacer.IsLogicYardTrack(x.Track)), Definition.Invert);
        }
    }
}
