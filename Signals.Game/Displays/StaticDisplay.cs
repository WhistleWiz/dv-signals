using Signals.Common.Displays;

namespace Signals.Game.Displays
{
    public class StaticDisplay : DisplayBase<StaticDisplayDefinition>
    {
        public StaticDisplay(DisplayBaseDefinition definition, Signal signal) : base(definition, signal)
        {
            DisplayText = Definition.DisplayedText;
        }

        public override void UpdateDisplay() { }
    }
}
