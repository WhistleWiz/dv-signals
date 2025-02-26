namespace Signals.Common.Displays
{
    public class TrackIdDisplayDefinition : InfoDisplayDefinition
    {
        public enum TrackIdDisplayMode
        {
            NumberOnly,
            NumberAndType
        }

        public TrackIdDisplayMode TrackIDMode = TrackIdDisplayMode.NumberOnly;
    }
}
