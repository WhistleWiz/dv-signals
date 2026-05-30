using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Track Info (Display)")]
    public class TrackInfoDisplayDefinition : DisplayBaseDefinition
    {
        [Tooltip("How to format the display\n" +
            "Use {#} with # matching the number you want:\n" +
            "0 - Station ID\n" +
            "1 - Yard ID\n" +
            "2 - Track number\n" +
            "3 - Track number (trimmed leading 0s)\n" +
            "4 - Track type\n" +
            "5 - Simple cardinal orientation (of signal) (N/E/S/W)\n" +
            "6 - Cardinal orientation (of signal) (N/NE/E/SE/S/SW/W/NW)\n" +
            "7 - Side (A for out, B for in)")]
        public string Format = "{2}";
        [Tooltip("Displayed when the track information is not available\n" +
            "Use a space to display an empty HUD icon, or no text to hide the HUD icon")]
        public string NoValidResultValue = "-";
        [Tooltip("Use the track where the signal is placed rather than the block")]
        public bool FromPlacement = false;
    }
}
