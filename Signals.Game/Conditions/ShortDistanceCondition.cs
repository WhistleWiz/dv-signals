using Signals.Common.Conditions;

namespace Signals.Game.Conditions
{
    public class ShortDistanceCondition : ConditionBase<ShortDistanceConditionDefinition>
    {
        public ShortDistanceCondition(ConditionBaseDefinition definition) : base(definition) { }

        public override bool MeetsConditions(Signal signal)
        {
            return signal.Controller.ShortDistance;
        }
    }
}
