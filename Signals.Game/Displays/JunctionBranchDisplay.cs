using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    internal class JunctionBranchDisplay : InfoDisplay
    {
        private JunctionBranchDisplayDefinition _fullDef;
        private JunctionSignalController? _junctionController;

        public JunctionBranchDisplay(InfoDisplayDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (JunctionBranchDisplayDefinition)definition;
            _junctionController = (JunctionSignalController)controller;
        }

        public override void UpdateDisplay()
        {
            if (_junctionController == null) return;

            if (_fullDef.TowardsOnly && !_junctionController.Direction.IsOut())
            {
                DisplayText = string.Empty;
                return;
            }

            DisplayText = $"{_junctionController.Junction.selectedBranch + (_fullDef.OffsetByOne ? 1 : 0)}";
        }
    }
}
