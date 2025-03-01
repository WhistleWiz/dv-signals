using UnityEngine;

namespace Signals.Common.Displays
{
    public class TrackIdDisplayDefinition : InfoDisplayDefinition
    {
        public enum TrackIdDisplayMode
        {
            [Tooltip("Displays only the track number ('1', '4')")]
            NumberOnly,
            [Tooltip("Displays the track number and type ('1L', '4S')")]
            NumberAndType,
            [Tooltip("Displays the track yard, number and type ('A1L', 'C4S')")]
            YardAndNumberAndType,
            [Tooltip("Displays the track yard and number type ('A1', 'C4')")]
            YardAndNumber,
            [Tooltip("Displays only the track yard ('A', 'C')")]
            YardOnly
        }

        public TrackIdDisplayMode TrackIDMode = TrackIdDisplayMode.NumberOnly;
        [Tooltip("Displayed when the track ID is not available\n" +
            "Use a space to display an empty HUD icon, or no text to hide the HUD icon")]
        public string NoValidResultValue = "-";
    }
}
