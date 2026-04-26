using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class RequiredBranchAspect : AspectBase
    {
        public RequiredBranchAspect(AspectBaseDefinition def, Signal signal) : base(def, signal) { }

        public override bool MeetsConditions()
        {
            var junction = Controller.GroupJunction;

            if (junction == null || !Controller.RequiredJunctionBranch.HasValue) return false;

            return Controller.RequiredJunctionBranch.Value == junction.selectedBranch;
        }
    }
}
