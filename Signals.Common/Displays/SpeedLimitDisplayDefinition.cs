using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Speed Limit (Display)")]
    public class SpeedLimitDisplayDefinition : DisplayBaseDefinition
    {
        [Tooltip("Displays \"1, 2, 3...\" rather than \"10, 20, 30...\"")]
        public bool DivideBy10 = true;
        [Tooltip("Ignore yard tracks\n" +
            "If there are only yard tracks, then their limit will be used still")]
        public bool IgnoreYards = false;
        [Tooltip("Displayed when the track information is not available\n" +
            "Use a space to display an empty HUD icon, or no text to hide the HUD icon")]
        public string NoValidResultValue = "-";
        [Tooltip("The maximum speed displayed\n" +
            "Values above will be treated as not valid")]
        public float MaximumDisplayed = 90;
    }
}
