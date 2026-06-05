using Signals.Common.Aspects;
using Signals.Game.Railway;
using System.Linq;

namespace Signals.Game.Aspects
{
    public class RequiredPathAspect : AspectBase<RequiredPathAspectDefinition>
    {
        public RequiredPathAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var block = Block;

            if (block == null) return false;

            return block.Tracks.Any(x => CheckTrack(x, Definition.Invert));

            static bool CheckTrack(TrackInfo track, bool invert)
            {
                if (!track.IsJunctionTrack) return false;

                var junction = track.Track.inJunction;

                if (!SignalManager.Instance.TryGetJunctionGroup(junction, out var group)) return false;

                // If the direction of the track is pointing out, then we have to check the JunctionSignal of the group.
                if (track.Direction.IsOut())
                {
                    var jController = group.JunctionSignal;

                    if (jController == null || !jController.RequiredJunctionBranch.HasValue) return false;

                    return GetResult(jController.RequiredJunctionBranch.Value);
                }

                // Else see if the controller is in any of the branch tracks...
                if (group.TryGetControllerForTrack(track.Track, out var tController))
                {
                    if (!tController.RequiredJunctionBranch.HasValue) return false;

                    return GetResult(tController.RequiredJunctionBranch.Value);
                }

                // If it wasn't in a branch track, then it must be the reverse controller.
                var rController = group.ReverseJunctionSignal;

                if (rController == null || !rController.RequiredJunctionBranch.HasValue) return false;

                return GetResult(rController.RequiredJunctionBranch.Value);

                bool GetResult(int branch)
                {
                    return ApplyInvert(branch == junction.selectedBranch, invert);
                }
            }
        }
    }
}
