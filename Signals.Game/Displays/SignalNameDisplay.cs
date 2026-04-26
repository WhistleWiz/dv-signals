using Signals.Common.Displays;

namespace Signals.Game.Displays
{
    public class SignalNameDisplay : InfoDisplay
    {
        public SignalNameDisplay(InfoDisplayDefinition definition, Signal signal) : base(definition, signal) { }

        public override void UpdateDisplay()
        {
            var name = Signal.Name;

            if (name != DisplayText)
            {
                DisplayText = name;
            }
        }
    }
}
