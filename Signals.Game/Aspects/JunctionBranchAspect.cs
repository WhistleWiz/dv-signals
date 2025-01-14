using Signals.Common.Aspects;
using Signals.Game.Controllers;

namespace Signals.Game.Aspects
{
    internal class JunctionBranchAspect : AspectBase
    {
        private JunctionBranchAspectDefinition _fullDef;

        public JunctionBranchAspect(AspectBaseDefinition def, BasicSignalController controller) : base(def, controller)
        {
            _fullDef = (JunctionBranchAspectDefinition)def;
        }

        public override bool MeetsConditions(WalkInfo _)
        {
            if (!Controller.HasJunction) return false;

            return Controller.Junction!.selectedBranch == _fullDef.ActiveOnBranch;
        }
    }
}
