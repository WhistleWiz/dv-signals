using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    public abstract class InfoDisplay
    {
        private bool _off = false;

        public InfoDisplayDefinition Definition;
        public BasicSignalController Controller;

        public string DisplayText { get => Definition.DisplayText; set => Definition.DisplayText = value; }
        public bool ShouldDisplay => !_off && !string.IsNullOrEmpty(DisplayText);
        public bool ShouldDisplayHUD => Definition.Mode != InfoDisplayDefinition.DisplayMode.WorldOnly && ShouldDisplay;

        protected InfoDisplay(InfoDisplayDefinition definition, BasicSignalController controller)
        {
            Definition = definition;
            Controller = controller;
        }

        /// <summary>
        /// Checks and updates the displays.
        /// </summary>
        /// <returns></returns>
        public bool CheckAndUpdate()
        {
            if (Definition.DisableWhenSignalIsOff && !Controller.IsOn)
            {
                if (!_off)
                {
                    Definition.gameObject.SetActive(false);
                    _off = true;
                }

                return false;
            }

            if (_off)
            {
                Definition.gameObject.SetActive(true);
                _off = false;
            }

            UpdateDisplay();
            return true;
        }

        public abstract void UpdateDisplay();
    }
}
