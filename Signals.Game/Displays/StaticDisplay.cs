using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    public class StaticDisplay : DisplayBase
    {
        private StaticDisplayDefinition _fullDef;

        public StaticDisplay(DisplayBaseDefinition definition, Signal signal) : base(definition, signal)
        {
            _fullDef = (StaticDisplayDefinition)definition;
            DisplayText = _fullDef.DisplayedText;
        }

        public override void UpdateDisplay() { }
    }
}
