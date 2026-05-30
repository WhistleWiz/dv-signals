using Signals.Common.Conditions;

namespace Signals.Game.Conditions
{
    public class ActsAsDistantCondition : ConditionBase<ActsAsDistantConditionDefinition>
    {
        public ActsAsDistantCondition(ConditionBaseDefinition definition) : base(definition) { }

        public override bool MeetsConditions(Signal signal)
        {
            return signal.Controller.ActingAsDistant;
        }
    }
}
