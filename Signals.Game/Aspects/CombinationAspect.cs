using Signals.Common.Aspects;
using System.Linq;

namespace Signals.Game.Aspects
{
    public class CombinationAspect : AspectBase<CombinationAspectDefinition>
    {
        private IAspect[] _conditions;

        public CombinationAspect(AspectBaseDefinition def, Signal signal) : base(def, signal)
        {
            _conditions = Definition.Conditions.Select(x => AspectCreator.Create(signal, x)).Where(x => x != null).ToArray()!;
        }

        public override bool MeetsConditions()
        {
            return Definition.Any ? _conditions.Any(x => x.MeetsConditions()) : _conditions.All(x => x.MeetsConditions());
        }
    }
}
