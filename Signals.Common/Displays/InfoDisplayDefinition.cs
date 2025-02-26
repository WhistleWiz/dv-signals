using TMPro;
using UnityEngine;

namespace Signals.Common.Displays
{
    public abstract class InfoDisplayDefinition : MonoBehaviour
    {
        public enum DisplayMode
        {
            Both,
            HUDOnly,
            WorldOnly
        }

        private string _displayText = string.Empty;

        public DisplayMode Mode;
        [Tooltip("Disables this GO when the signal is turned off")]
        public bool DisableWhenSignalIsOff = false;
        [Tooltip("The background of this display when hovered")]
        public Sprite HUDBackground = null!;
        [Tooltip("Optional world text object to assign the value of this display")]
        public TMP_Text? Text;

        public string DisplayText
        {
            get => _displayText;
            set
            {
                _displayText = value;

                if (Text != null)
                {
                    Text.text = value;
                }
            }
        }
    }
}
