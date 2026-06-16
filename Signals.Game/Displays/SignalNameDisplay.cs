using Signals.Common.Displays;

namespace Signals.Game.Displays
{
    public class SignalNameDisplay : DisplayBase<SignalNameDisplayDefinition>
    {
        public SignalNameDisplay(DisplayBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override void UpdateDisplay()
        {
            DisplayText = Signal.Name;
        }
    }
}
