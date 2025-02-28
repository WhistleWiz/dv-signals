using UnityEngine;

namespace Signals.Common.Displays
{
    public class TrackIdDisplayDefinition : InfoDisplayDefinition
    {
        public enum TrackIdDisplayMode
        {
            [Tooltip("Displays only the track number ('1', '4')")]
            NumberOnly,
            [Tooltip("Displays the track number and its type ('1L', '4S')")]
            NumberAndType
        }

        public TrackIdDisplayMode TrackIDMode = TrackIdDisplayMode.NumberOnly;
        [Tooltip("Displayed when the track ID is not available")]
        public string NoNumberValue = "-";

        public override bool AlwaysUpdate => true;
    }
}
