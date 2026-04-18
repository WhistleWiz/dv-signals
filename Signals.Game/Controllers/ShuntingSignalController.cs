using Signals.Common;
using Signals.Game.Railway;

namespace Signals.Game.Controllers
{
    /// <summary>
    /// A controller for shunting signals.
    /// </summary>
    public class ShuntingSignalController : BasicSignalController
    {
        public TrackDirection Direction { get; private set; }
        public RailTrack? Track { get; private set; }
        public Junction? Junction { get; private set; }

        private ShuntingSignalController(SignalControllerDefinition def, SignalPlacementInfo info) : base(def, info)
        {
            Type = SignalType.Shunting;
            PrefabType = PrefabType.Shunting;
            Operation = SignalOperationMode.SemiManual;
        }

        public ShuntingSignalController(SignalControllerDefinition def, RailTrack track, TrackDirection direction, SignalPlacementInfo info) :
            this(def, info)
        {
            Direction = direction;
            Track = track;
            Block = TrackBlock.CreateForShunting(track);
            InternalName = $"{Block.Station}-{Block.Yard}{Block.TrackNumber}{PlacementLetter}";
        }

        public ShuntingSignalController(SignalControllerDefinition def, Junction junction, SignalPlacementInfo info) :
            this(def, info)
        {
            Junction = junction;
            Block = TrackBlock.CreateForShunting(junction);
            InternalName = $"{junction.GetStation()}-{junction.junctionData.junctionId}S";
        }

        public override void UpdateBlock()
        {
            if (Junction == null) return;

            Block = TrackBlock.CreateForShunting(Junction);
        }
    }
}
