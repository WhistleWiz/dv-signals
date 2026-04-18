using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    public class SignalIdDisplay : InfoDisplay
    {
        public SignalIdDisplay(InfoDisplayDefinition definition, BasicSignalController controller) : base(definition, controller) { }

        public override void UpdateDisplay()
        {
            var id = Controller.Id.ToString();

            if (id != DisplayText)
            {
                DisplayText = id;
            }
        }
    }
}
