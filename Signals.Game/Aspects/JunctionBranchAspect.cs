using Signals.Common.Aspects;
using Signals.Game.Controllers;

namespace Signals.Game.Aspects
{
    internal class JunctionBranchAspect : AspectBase
    {
        private JunctionBranchAspectDefinition _fullDef;
        private JunctionSignalController? _junctionController;

        public JunctionBranchAspect(AspectBaseDefinition def, BasicSignalController controller) : base(def, controller)
        {
            _fullDef = (JunctionBranchAspectDefinition)def;
            _junctionController = (JunctionSignalController)controller;
        }

        public override bool MeetsConditions(WalkInfo _)
        {
            if (_junctionController == null) return false;

            return _junctionController.Junction.selectedBranch == _fullDef.ActiveOnBranch;
        }
    }
}
