using DV.Logic.Job;
using System;

namespace Signals.Game.Railway
{
    public readonly struct TrackInfo : IEquatable<TrackInfo>
    {
        public readonly RailTrack Track;
        public readonly TrackDirection Direction;

        public readonly double Length => Track.GetLength();
        public readonly bool IsJunctionTrack => Track.isJunctionTrack;

        public TrackInfo(RailTrack track, TrackDirection direction)
        {
            Track = track;
            Direction = direction;
        }

        public readonly TrackID GetId() => Track.GetId();

        public override bool Equals(object obj) => obj is TrackInfo other && Equals(other);

        public bool Equals(TrackInfo other) => Direction == other.Direction && Track == other.Track;

        public static bool operator ==(TrackInfo a, TrackInfo b) => a.Equals(b);

        public static bool operator !=(TrackInfo a, TrackInfo b) => !(a == b);

        public override int GetHashCode() => (Track, Direction).GetHashCode();
    }
}
