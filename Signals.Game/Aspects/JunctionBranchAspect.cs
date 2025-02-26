using Signals.Common.Aspects;
using Signals.Game.Controllers;

namespace Signals.Game.Aspects
{
    internal class JunctionBranchAspect : AspectBase
    {
        private JunctionBranchAspectDefinition _fullDef;
        private JunctionSignalController? _junctionController;

        public JunctionBranchAspect(AspectBaseDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (JunctionBranchAspectDefinition)definition;
            _junctionController = (JunctionSignalController)controller;
        }

        public override bool MeetsConditions()
        {
            if (_junctionController == null) return false;

            return _junctionController.Junction.selectedBranch == _fullDef.ActiveOnBranch;
        }
    }
}
