using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    internal class SignalNameDisplay : InfoDisplay
    {
        public SignalNameDisplay(InfoDisplayDefinition definition, BasicSignalController controller) : base(definition, controller) { }

        public override void UpdateDisplay()
        {
            var name = Controller.Name;

            if (name != DisplayText)
            {
                DisplayText = name;
            }
        }
    }
}
