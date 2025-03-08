using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    internal class StaticDisplay : InfoDisplay
    {
        private StaticDisplayDefinition _fullDef;

        public StaticDisplay(InfoDisplayDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (StaticDisplayDefinition)definition;
            DisplayText = _fullDef.DisplayedText;
        }

        public override void UpdateDisplay() { }
    }
}
