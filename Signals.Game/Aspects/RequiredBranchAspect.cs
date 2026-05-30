using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class RequiredBranchAspect : AspectBase<RequiredBranchAspectDefinition>
    {
        public RequiredBranchAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var junction = Controller.GroupJunction;

            if (junction == null || !Controller.RequiredJunctionBranch.HasValue) return false;

            return Definition.Invert ?
                Controller.RequiredJunctionBranch.Value != junction.selectedBranch :
                Controller.RequiredJunctionBranch.Value == junction.selectedBranch;
        }
    }
}
