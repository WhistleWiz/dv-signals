using Signals.Common;
using Signals.Game.Railway;
using System.Collections.Generic;

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
        }

        protected override bool ShouldMoveForwards(RailTrack track)
        {
            return StartingTrack != track;
        }

        public override void UpdateBlocks()
        {
            foreach (var signal in Signals)
            {
                var block = signal.Block;

                if (block != null && !block.ShouldBeUpdated) continue;

                signal.SetBlock(Type == SignalType.Spacing ?
                    TrackBlock.CreateForSpacing(StartingTrack, Direction, this) :
                    TrackBlock.CreateUntilMainSignal(StartingTrack, Direction, this));
            }

            foreach (var signal in ShuntingSignals)
            {
                var block = signal.Block;

                if (block != null && !block.ShouldBeUpdated) continue;

                signal.SetBlock(TrackBlock.CreateForShunting(StartingTrack, Direction, this));
            }
        }

        public override IEnumerable<(Signal Signal, Dictionary<BasicSignalController, float> Controllers)> GetPotentialNextControllers()
        {
            foreach (var signal in Signals)
            {
                yield return (signal, TrackWalker.GetAllPossibleMainControllers(StartingTrack, Direction, this));
            }
        }

        public static TrackSignalController? Replace(TrackSignalController original, SignalControllerDefinition def)
        {
            if (!original.PlacementInfo.HasValue) return null;

            var replacement = new TrackSignalController(def, original.StartingTrack, original.Direction, original.PlacementInfo.Value)
            {
                ActingAsDistant = original.ActingAsDistant,
                ShortDistance = original.ShortDistance
            };
            var group = original.Group;

            if (group != null)
            {
                replacement.Group = group;

                if (group.ReverseJunctionSignal == original)
                {
                    group.ReverseJunctionSignal = replacement;
                }
                if (group.BranchSignals.Contains(original))
                {
                    group.BranchSignals.Add(replacement);
                }
            }

            original.Destroy();
            return replacement;
        }
    }
}
