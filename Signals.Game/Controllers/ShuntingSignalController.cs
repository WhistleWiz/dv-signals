using Signals.Common;

namespace Signals.Game.Controllers
{
    internal class ShuntingSignalController : BasicSignalController
    {
        public TrackDirection Direction { get; private set; }
        public RailTrack Track { get; private set; }

        public ShuntingSignalController(SignalControllerDefinition def, RailTrack track, TrackDirection direction, SignalPlacementInfo info) :
            base(def, info)
        {
            Type = SignalType.Shunting;
            PrefabType = PrefabType.Shunting;
            Operation = SignalOperationMode.FullManual;
            Direction = direction;
            Track = track;
            Block = TrackBlock.CreateForShunting(track);
            InternalName = $"{Block.Station}-{Block.Yard}{Block.TrackNumber}{PlacementLetter}";

            ChangeToLeastRestrictive(true);
        }
    }
}
