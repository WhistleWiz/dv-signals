using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    public abstract class InfoDisplay
    {
        public InfoDisplayDefinition Definition;
        public BasicSignalController Controller;

        public string DisplayText { get => Definition.DisplayText; set => Definition.DisplayText = value; }

        protected InfoDisplay(InfoDisplayDefinition definition, BasicSignalController controller)
        {
            Definition = definition;
            Controller = controller;

            UpdateDisplay();
        }

        public abstract void UpdateDisplay();
    }
}
