using Signals.Common.Aspects;
using Signals.Game.Railway;
using UnityEngine;

namespace Signals.Game.Aspects
{
    public class MaxSpeedAspect : AspectBase<MaxSpeedAspectDefinition>
    {
        public MaxSpeedAspect(AspectBaseDefinition def, Signal signal) : base(def, signal) { }

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
                if (Definition.IgnoreYards && track.Track.IsPartOfStation()) continue;

                // Get the maximum speed of the track.
                max = Mathf.Min(max, SpeedCalculator.GetSpeed(track.Track, track.Direction));
                length += track.Length;

                // If we walked enough, stop looping.
                if (length >= SpeedCalculator.EndDistance) goto End;
            }

            // If somehow there wasn't a track at all, return false.
            if (!first.HasValue) return false;

            // We only reach here in case the option to skip yards was true, and there were no non-yard tracks at all.
            max = SpeedCalculator.GetSpeed(first.Value.Track, first.Value.Direction);

        End:
            if (Definition.DynamicPassingSpeed)
            {
                Definition.PassingSpeed = max;
            }

            return max <= Definition.Maximum;
        }
    }
}
