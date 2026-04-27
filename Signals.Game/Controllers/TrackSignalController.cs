using Signals.Common;
using Signals.Game.Railway;

namespace Signals.Game.Controllers
{
    /// <summary>
    /// A controller for a block starting on a fixed track.
    /// </summary>
    public class TrackSignalController : BasicSignalController
    {
        public RailTrack StartingTrack { get; protected set; }
        public TrackDirection Direction { get; protected set; }

        public TrackSignalController(SignalControllerDefinition def, RailTrack starting, TrackDirection startingDirection, SignalPlacementInfo info) :
            base(def, info)
        {
            StartingTrack = starting;
            Direction = startingDirection;

            if (ShuntingSignal != null)
            {
                ShuntingSignal.Block = TrackBlock.CreateForShunting(starting);
            }

            if (starting.isJunctionTrack)
            {
                var junction = starting.inJunction;
                int count = 0;

                foreach (var item in junction.outBranches)
                {
                    count++;

                    if (item.track == starting) break;
                }

                InternalName = $"{junction.junctionData.junctionIdLong}-B{count}";
            }
            else
            {
                var junction = startingDirection.IsOut() ? starting.inJunction : starting.outJunction;
                InternalName = junction != null ? $"{junction.junctionData.junctionIdLong}-F" : $"{StartingTrack.name}:{PlacementLetter}";
            }
        }

        protected override bool ShouldMoveForwards(RailTrack track)
        {
            return StartingTrack != track;
        }

        public override void UpdateBlocks()
        {
            foreach (var signal in Signals)
            {
                signal.Block = TrackBlock.CreateUntilMainSignal(StartingTrack, Direction, this);
            }
        }
    }
}
