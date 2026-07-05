using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Aspect Conditional (Display)")]
    public class AspectConditionalDisplayDefinition : DisplayBaseDefinition
    {
        public DisplayBaseDefinition ActualDisplay = null!;
        public string[] AllowedAspectIds = new string[0];
        [Tooltip("Displayed when the aspect ID is not part of the allowed aspects\n" +
            "Use a space to display an empty HUD icon, or no text to hide the HUD icon")]
        public string NoValidResultValue = "-";
    }
}
