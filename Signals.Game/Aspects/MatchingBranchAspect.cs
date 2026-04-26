using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class MatchingBranchAspect : AspectBase
    {
        private MatchingBranchAspectDefinition _fullDef;

        public MatchingBranchAspect(AspectBaseDefinition def, Signal signal) : base(def, signal)
        {
            _fullDef = (MatchingBranchAspectDefinition)def;
        }

        public override bool MeetsConditions()
        {
            var group = Controller.Group;

            if (group == null) return false;

            if (!group.TryGetControllerForTrack(group.Junction.GetCurrentBranch().track, out var branchController)) return false;

            return _fullDef.Invert ? branchController != Controller : branchController == Controller;
        }
    }
}
