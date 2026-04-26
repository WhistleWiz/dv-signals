using TMPro;
using UnityEngine;

namespace Signals.Common.Displays
{
    public abstract class InfoDisplayDefinition : MonoBehaviour
    {
        public enum UpdateMode
        {
            [Tooltip("Display is only updated once at the start\n" +
                "It is also updated again if the signal is restarted and 'Disable When Signal Is Off' is true")]
            AtStart,
            [Tooltip("Display is updated when the signal aspect changes")]
            AspectChanged,
            [Tooltip("Display is updated when the signal is updated")]
            Always
        }

        private string _displayText = string.Empty;

        [Tooltip("Disables this GO when the signal is turned off")]
        public bool DisableWhenSignalIsOff = false;
        [Tooltip("How often this display is updated")]
        public UpdateMode Mode = UpdateMode.AspectChanged;
        [Tooltip("The sprite of this display when hovered\n" +
            "The display is not shown in the HUD when this is null")]
        public Sprite HUDSprite = null!;
        [Tooltip("The colour of the text when hovered")]
        public Color HUDTextColour = Color.black;
        [Tooltip("Lower values are placed above higher values")]
        public int HUDDisplayOrder = 1;
        [Tooltip("Optional world text object to assign the value of this display")]
        public TMP_Text? Text;

        public string DisplayText
        {
            get => _displayText;
            set
            {
                if (_displayText == value) return;

                _displayText = value;

                if (Text != null)
                {
                    Text.text = value;
                }
            }
        }
    }
}
