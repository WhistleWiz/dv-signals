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
        [Tooltip("The background of this display when hovered")]
        public Sprite HUDBackground = null!;
        [Tooltip("The colour of the text when hovered")]
        public Color HUDTextColour = Color.black;
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
