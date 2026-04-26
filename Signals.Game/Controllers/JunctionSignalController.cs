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

            if (ShuntingSignal != null)
            {
                ShuntingSignal.Block = TrackBlock.CreateForShunting(junction);
            }

            Junction.Switched += JunctionSwitched;
            Destroyed += (x) => Junction.Switched -= JunctionSwitched;

            InternalName = $"{GroupJunction?.junctionData.junctionIdLong}-T";
        }

        private void JunctionSwitched(Junction.SwitchMode mode, int branch)
        {
            // Force update the display because of junction branch updates even if
            // the state didn't change.
            Update(true, true);
        }

        public override void UpdateBlocks()
        {
            StartingTrack = OverrideStart ?? Junction.GetCurrentBranch().track;

            if (ShuntingSignal != null)
            {
                ShuntingSignal.Block = TrackBlock.CreateForShunting(Junction);
            }

            if (Signals.Length == 1)
            {
                base.UpdateBlocks();
                return;
            }

            var selected = Junction.selectedBranch;

            for (byte i = 0; i < Signals.Length; i++)
            {
                Junction.selectedBranch = (byte)(i % Junction.outBranches.Count);
                var track = OverrideStart ?? Junction.GetCurrentBranch().track;
                Signals[i].Block = TrackBlock.CreateUntilMainSignal(track, Direction, this);
            }

            Junction.selectedBranch = selected;
        }

        public override IEnumerable<TrackBlock> GetPotentialBlocks()
        {
            var selected = Junction.selectedBranch;

            for (byte i = 0; i < Junction.outBranches.Count; i++)
            {
                Junction.selectedBranch = i;
                var track = OverrideStart ?? Junction.GetCurrentBranch().track;
                yield return TrackBlock.CreateUntilMainSignal(track, Direction, this);
            }

            Junction.selectedBranch = selected;
        }
    }
}
