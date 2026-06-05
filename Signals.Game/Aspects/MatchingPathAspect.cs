using Signals.Common.Aspects;
using Signals.Game.Railway;
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

            return block.Tracks.Any(x => CheckTrack(x, Definition.Invert));

            static bool CheckTrack(TrackInfo track, bool invert)
            {
                if (!track.IsJunctionTrack) return false;

                var junction = track.Track.inJunction;

                // If the direction of the track is pointing out, then we can't be against it.
                if (track.Direction.IsOut()) return false;

                return ApplyInvert(junction.GetCurrentBranch().track == track.Track, invert);
            }
        }
    }
}
