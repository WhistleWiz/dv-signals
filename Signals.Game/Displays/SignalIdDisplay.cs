using Signals.Common.Displays;

namespace Signals.Game.Displays
{
    public class SignalIdDisplay : InfoDisplay
    {
        public SignalIdDisplay(InfoDisplayDefinition definition, Signal signal) : base(definition, signal) { }

        public override void UpdateDisplay()
        {
            var id = Signal.Id.ToString();

            if (id != DisplayText)
            {
                DisplayText = id;
            }
        }
    }
}
