namespace Signals.Game
{
    public struct SignalPlacementInfo
    {
        public RailTrack Track;
        public TrackDirection Direction;
        public int PointIndex;
        public double Span;

        public SignalPlacementInfo(RailTrack track, TrackDirection direction, int pointIndex, double span)
        {
            Track = track;
            Direction = direction;
            PointIndex = pointIndex;
            Span = span;
        }
    }
}
