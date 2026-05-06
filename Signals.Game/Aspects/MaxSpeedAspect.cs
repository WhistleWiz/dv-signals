using Signals.Common.Aspects;
using Signals.Game.Railway;
using UnityEngine;

namespace Signals.Game.Aspects
{
    public class MaxSpeedAspect : AspectBase
    {
        private MaxSpeedAspectDefinition _fullDef;

        public MaxSpeedAspect(AspectBaseDefinition def, Signal signal) : base(def, signal)
        {
            _fullDef = (MaxSpeedAspectDefinition)def;
        }

        public override bool MeetsConditions()
        {
            var block = Signal.Block;

            if (block == null) return false;

            TrackInfo? first = null;
            var length = 0.0;
            var max = float.PositiveInfinity;

            foreach (var track in block.Tracks)
            {
                // Skip junction tracks.
                if (track.IsJunctionTrack) continue;

                if (!first.HasValue)
                {
                    first = track;
                }

                // Skip track if it is in a yard and we want to ignore those.
                if (_fullDef.IgnoreYards && track.Track.IsPartOfStation()) continue;

                // Get the maximum speed of the track.
                max = Mathf.Min(max, SpeedCalculator.GetSpeed(track.Track, track.Direction));
                length += track.Length;

                // If we walked enough, compare with the lowest maximum.
                if (length >= SpeedCalculator.EndDistance)
                {
                    return _fullDef.Maximum <= max;
                }
            }

            // If somehow there wasn't a track at all, return false.
            if (!first.HasValue) return false;

            // We only reach here in case the option to skip yards was true, and there were no non-yard tracks at all.
            return _fullDef.Maximum <= SpeedCalculator.GetSpeed(first.Value.Track, first.Value.Direction);
        }
    }
}
