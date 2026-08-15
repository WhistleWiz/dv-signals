namespace Signals.Game
{
    public struct SignalPlacementInfo
    {
        public RailTrack Track;
        public TrackDirection Direction;
        public int PointIndex;
        public double Span;
        public bool OppositeSide;

        public SignalPlacementInfo(RailTrack track, TrackDirection direction, int pointIndex, double span, bool opposite = false)
        {
            Track = track;
            Direction = direction;
            PointIndex = pointIndex;
            Span = span;
            OppositeSide = opposite;
        }

        public readonly SignalPlacementInfo GetFlipped() => new SignalPlacementInfo(Track, Direction, PointIndex, Span, !OppositeSide);
    }
}
