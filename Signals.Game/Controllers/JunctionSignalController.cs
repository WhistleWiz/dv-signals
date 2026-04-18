using Signals.Common;
using Signals.Game.Railway;
using System.Collections.Generic;

namespace Signals.Game.Controllers
{
    /// <summary>
    /// A controller for a signal placed at a junction.
    /// </summary>
    /// <remarks>
    /// Switching the junction automatically updates the signal.
    /// </remarks>
    public class JunctionSignalController : TrackSignalController
    {
        /// <summary>
        /// If not <see langword="null"/>, the block will start at the specified track rather than the junction branches.
        /// </summary>
        public RailTrack? OverrideStart;

        public bool Left { get; protected set; }
        public Junction Junction { get; protected set; }

        public JunctionSignalController(SignalControllerDefinition def, Junction junction, RailTrack? starting, SignalPlacementInfo info) :
            base(def, starting ?? junction.GetCurrentBranch().track, TrackDirection.Out, info)
        {
            OverrideStart = starting;

            Junction = junction;
            Left = junction.IsLeft();

            Junction.Switched += JunctionSwitched;
            Destroyed += (x) => Junction.Switched -= JunctionSwitched;

            InternalName = $"{Junction.junctionData.junctionIdLong}-T";
        }

        private void JunctionSwitched(Junction.SwitchMode mode, int branch)
        {
            // Force update the display because of junction branch updates even if
            // the state didn't change.
            UpdateAspect(true);
            UpdateDisplays(true);
        }

        public override void UpdateBlock()
        {
            StartingTrack = OverrideStart ?? Junction.GetCurrentBranch().track;

            base.UpdateBlock();
        }

        public override List<TrackBlock> GetPotentialBlocks()
        {
            var list = new List<TrackBlock>();

            if (OverrideStart != null)
            {
                list.Add(TrackBlock.CreateUntilSignal(OverrideStart, Direction, Type == SignalType.Shunting, this));
                return list;
            }

            foreach (var branch in Junction.outBranches)
            {
                if (branch == null || branch.track == null) continue;

                list.Add(TrackBlock.CreateUntilSignal(branch.track, Direction, Type == SignalType.Shunting, this));
            }

            return list;
        }
    }
}
