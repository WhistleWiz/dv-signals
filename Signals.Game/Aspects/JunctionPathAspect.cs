using Signals.Common.Aspects;
using System.Linq;

namespace Signals.Game.Aspects
{
    public class JunctionPathAspect : AspectBase<JunctionPathAspectDefinition>
    {
        public JunctionPathAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var block = Block;

            if (block == null) return false;

            // Use Tracks, not AllTracks, since these are the ones that make up the path.
            return Definition.PathMode switch
            {
                JunctionPathAspectDefinition.JunctionPathMode.AnyThrough => block.Tracks.Any(x => CheckTrack(x.Track, true)),
                JunctionPathAspectDefinition.JunctionPathMode.AnyDiverging => block.Tracks.Any(x => CheckTrack(x.Track, false)),
                JunctionPathAspectDefinition.JunctionPathMode.AllThrough => !block.Tracks.Any(x => CheckTrack(x.Track, false)),
                JunctionPathAspectDefinition.JunctionPathMode.AllDiverging => !block.Tracks.Any(x => CheckTrack(x.Track, true)),
                _ => false,
            };

            static bool CheckTrack(RailTrack track, bool through)
            {
                if (!track.isJunctionTrack) return false;

                return through ? track.name == "[track through]" : track.name != "[track through]";
            }
        }
    }
}
