using UnityEngine;

namespace Signals.Common.Displays
{
    public class NextStationDisplayDefinition : InfoDisplayDefinition
    {
        public enum StationDisplayMode
        {
            ID,
            FirstLetter
        }

        public enum StationSearchMode
        {
            NextSignal,
            UntilFound
        }

        public StationDisplayMode DisplayMode;
        public StationSearchMode SearchMode;
        
        [Tooltip("Displayed when the track ID is not available\n" +
            "Use a space to display an empty HUD icon, or no text to hide the HUD icon")]
        public string NoValidResultValue = "-";
    }
}
