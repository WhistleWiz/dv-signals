using Signals.Common;
using Signals.Game.Railway;
using System.Collections.Generic;

namespace Signals.Game.Controllers
{
    /// <summary>
    /// Controls a signal with a junction.
    /// </summary>
    /// <remarks>
    /// Signal aspect is updated every second when a player is within 2km of the signal, and slows to
    /// an update every 5 seconds when further away.
    /// </remarks>
    public class JunctionSignalController : TrackSignalController
    {
        public RailTrack? OverrideStart;

        public Junction Junction { get; protected set; }

        public JunctionSignalController(SignalControllerDefinition def, Junction junction, RailTrack? starting, SignalPlacementInfo info) :
            base(def, starting ?? junction.GetCurrentBranch().track, TrackDirection.Out, info)
        {
            OverrideStart = starting;

            Junction = junction;

            Junction.Switched += JunctionSwitched;
            Destroyed += (x) => Junction.Switched -= JunctionSwitched;

            InternalName = $"{Junction.junctionData.junctionIdLong}-{(info.Direction.IsOut() ? 'T' : 'F')}";
        }

        public JunctionSignalController(SignalControllerDefinition def, Junction junction, RailTrack? starting, TrackDirection direction, SignalPlacementInfo info) :
            base(def, starting ?? junction.GetCurrentBranch().track, TrackDirection.Out, info)
        {
            OverrideStart = starting;
            Direction = direction;

            Junction = junction;

            Junction.Switched += JunctionSwitched;
            Destroyed += (x) => Junction.Switched -= JunctionSwitched;

            InternalName = $"{Junction.junctionData.junctionIdLong}-{(info.Direction.IsOut() ? 'T' : 'F')}";
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
