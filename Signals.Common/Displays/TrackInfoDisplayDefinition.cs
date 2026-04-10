using UnityEngine;

namespace Signals.Common.Displays
{
    public class TrackInfoDisplayDefinition : InfoDisplayDefinition
    {
        [Tooltip("How to format the display\n" +
            "Use {0} for station ID, {1} for yard ID, {2} for track number, and {3} for track type")]
        public string Format = "{2}";
        [Tooltip("Displayed when the track information is not available\n" +
            "Use a space to display an empty HUD icon, or no text to hide the HUD icon")]
        public string NoValidResultValue = "-";
    }
}
