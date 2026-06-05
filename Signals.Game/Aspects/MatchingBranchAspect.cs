using Signals.Common.Aspects;
using Signals.Game.Controllers;

namespace Signals.Game.Aspects
{
    public class MatchingBranchAspect : AspectBase<MatchingBranchAspectDefinition>
    {

        public MatchingBranchAspect(AspectBaseDefinition def, Signal signal) : base(def, signal) { }

        public override bool MeetsConditions()
        {
            if (!(Controller is TrackSignalController tController)) return false;

            var group = Controller.Group;

            if (group == null) return false;

            if (Controller == group.JunctionSignal || Controller == group.ReverseJunctionSignal) return false;

            return ApplyInvert(group.Junction.GetCurrentBranch().track == tController.StartingTrack, Definition.Invert);
        }
    }
}
