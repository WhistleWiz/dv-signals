using DV.Logic.Job;

namespace Signals.Game.Railway
{
    public struct TrackInfo
    {
        public RailTrack Track;
        public TrackDirection Direction;

        public readonly double Length => Track.GetLength();
        public readonly bool IsJunctionTrack => Track.isJunctionTrack;

        public TrackInfo(RailTrack track, TrackDirection direction)
        {
            Track = track;
            Direction = direction;
        }

        public readonly TrackID GetId() => Track.GetId();
    }
}
