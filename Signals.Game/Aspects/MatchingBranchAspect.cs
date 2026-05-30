using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class MatchingBranchAspect : AspectBase<MatchingBranchAspectDefinition>
    {
        public MatchingBranchAspect(AspectBaseDefinition def, Signal signal) : base(def, signal) { }

        public override bool MeetsConditions()
        {
            var group = Controller.Group;

            if (group == null) return false;

            if (Controller == group.JunctionSignal || Controller == group.ReverseJunctionSignal) return false;

            if (!group.TryGetControllerForTrack(group.Junction.GetCurrentBranch().track, out var branchController)) return false;

            return Definition.Invert ? branchController != Controller : branchController == Controller;
        }
    }
}
