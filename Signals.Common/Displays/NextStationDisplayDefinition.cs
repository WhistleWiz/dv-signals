using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Next Station (Display)")]
    public class NextStationDisplayDefinition : InfoDisplayDefinition
    {
        public enum StationDisplayMode
        {
            ID,
            FirstLetter
        }

        public StationDisplayMode DisplayMode;
        
        [Tooltip("Displayed when the station ID is not available\n" +
            "Use a space to display an empty HUD icon, or no text to hide the HUD icon")]
        public string NoValidResultValue = "-";
    }
}
