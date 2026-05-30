using Signals.Common.Conditions;

namespace Signals.Game.Conditions
{
    public class WrongSideOfTrackCondition : ConditionBase<WrongSideOfTrackConditionDefinition>
    {
        public WrongSideOfTrackCondition(ConditionBaseDefinition definition) : base(definition) { }

        public override bool MeetsConditions(Signal signal)
        {
            var info = signal.Controller.PlacementInfo;
            return info.HasValue && info.Value.OppositeSide;
        }
    }
}
