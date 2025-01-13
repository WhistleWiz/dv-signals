using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    internal class JunctionBranchAspect : AspectBase
    {
        private JunctionBranchAspectDefinition _fullDef;

        public JunctionBranchAspect(AspectBaseDefinition def, SignalController controller) : base(def, controller)
        {
            _fullDef = (JunctionBranchAspectDefinition)def;
        }

        public override bool MeetsConditions(RailTrack[] tracksToNextSignal, SignalController? nextSignal)
        {
            if (_fullDef.IgnoreIfFacingInBranch && !Controller.TowardsBranches) return false;

            return Controller.AssignedJunction.selectedBranch == _fullDef.ActiveOnBranch;
        }
    }
}
