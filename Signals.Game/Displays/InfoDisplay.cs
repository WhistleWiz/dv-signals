using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    public abstract class InfoDisplay
    {
        private bool _hasUpdated = false;
        private bool _off = false;

        public InfoDisplayDefinition Definition;
        public BasicSignalController Controller;

        public string DisplayText { get => Definition.DisplayText; set => Definition.DisplayText = value; }
        public bool ShouldDisplay => !_off && !string.IsNullOrEmpty(DisplayText);
        public bool ShouldDisplayHUD => Definition.HUDBackground != null && ShouldDisplay;

        protected InfoDisplay(InfoDisplayDefinition definition, BasicSignalController controller)
        {
            Definition = definition;
            Controller = controller;
        }

        /// <summary>
        /// Checks and updates the displays.
        /// </summary>
        /// <param name="aspectChanged">If the aspect of the signal changed.</param>
        /// <returns></returns>
        public bool CheckAndUpdate(bool aspectChanged)
        {
            if (Definition.DisableWhenSignalIsOff && !Controller.IsOn)
            {
                if (!_off)
                {
                    Definition.gameObject.SetActive(false);
                    _hasUpdated = false;
                    _off = true;
                }

                return false;
            }

            if (_off)
            {
                Definition.gameObject.SetActive(true);
                _off = false;
            }

            if (Definition.Mode == InfoDisplayDefinition.UpdateMode.AtStart && _hasUpdated)
            {
                return false;
            }

            if (Definition.Mode == InfoDisplayDefinition.UpdateMode.AspectChanged && !aspectChanged)
            {
                return false;
            }

            _hasUpdated = true;
            UpdateDisplay();
            return true;
        }

        public abstract void UpdateDisplay();
    }
}
