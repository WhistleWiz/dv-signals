using Signals.Common.Conditions;

namespace Signals.Game.Conditions
{
    public interface ICondition
    {
        public bool MeetsConditions(Signal signal);
    }

    public abstract class ConditionBase<T> : ICondition
        where T : ConditionBaseDefinition
    {
        public T Definition { get; private set; }

        public ConditionBase(ConditionBaseDefinition definition)
        {
            Definition = (T)definition;
        }

        public abstract bool MeetsConditions(Signal signal);
    }
}
