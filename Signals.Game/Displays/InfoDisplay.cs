using Signals.Common;
using Signals.Common.Displays;
using Signals.Game.Controllers;
using UnityEngine;

namespace Signals.Game.Displays
{
    public abstract class InfoDisplay : IHudDisplayable
    {
        private bool _hasUpdated = false;
        private bool _off = false;

        public Signal Signal;
        public InfoDisplayDefinition Definition;

        public string DisplayText { get => Definition.DisplayText; set => Definition.DisplayText = value; }
        public bool ShouldDisplay => !_off && !string.IsNullOrEmpty(DisplayText) && Definition.HUDSprite != null;
        public int DisplayOrder => Definition.HUDDisplayOrder;
        public Sprite? Sprite => Definition.HUDSprite;
        public Color TextColour => Definition.HUDTextColour;
        public BasicSignalController Controller => Signal.Controller;

        protected InfoDisplay(InfoDisplayDefinition definition, Signal signal)
        {
            Signal = signal;
            Definition = definition;
        }

        /// <summary>
        /// Checks and updates the displays.
        /// </summary>
        /// <param name="aspectChanged">If the aspect of the signal changed.</param>
        /// <returns></returns>
        public bool CheckAndUpdate(bool aspectChanged)
        {
            if (Definition.DisableWhenSignalIsOff && !Signal.IsOn)
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
