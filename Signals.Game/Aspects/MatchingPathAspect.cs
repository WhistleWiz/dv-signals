using Signals.Common.Aspects;
using Signals.Game.Railway;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game.Aspects
{
    public class MatchingPathAspect : AspectBase<MatchingPathAspectDefinition>
    {
        public MatchingPathAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var block = Block;

            if (block == null) return false;

            var set = new HashSet<Junction>();

            return block.Tracks.Any(CheckTrack);

            bool CheckTrack(TrackInfo track)
            {
                if (!track.IsJunctionTrack) return false;

                var junction = track.Track.inJunction;

                if (set.Contains(junction)) return false;

                set.Add(junction);

                // If the direction of the track is pointing out, then we can't be against it.
                if (track.Direction.IsOut()) return false;

                return ApplyInvert(junction.GetCurrentBranch().track == track.Track, Definition.Invert);
            }
        }
    }
}
