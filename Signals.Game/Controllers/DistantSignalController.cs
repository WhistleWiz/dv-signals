using Signals.Common;
using Signals.Game.Aspects;
using Signals.Game.Railway;

namespace Signals.Game.Controllers
{
    /// <summary>
    /// A controller for a distant signal.
    /// </summary>
    public class DistantSignalController : BasicSignalController
    {
        public BasicSignalController Home { get; private set; }
        public float Distance { get; private set; }
        public bool IsRepeater { get; private set; }

        public DistantSignalController(SignalControllerDefinition def, BasicSignalController home,
            SignalPlacementInfo info, float distance, bool repeater) : base(def, info)
        {
            Home = home;
            Home.AnyAspectChanged += UpdateFromHome;

            Distance = distance;
            IsRepeater = repeater;
            Type = SignalType.Distant;

            foreach (var signal in Signals)
            {
                signal.SetBlock(TrackBlock.CreateForDistant(home, distance));
            }
        }

        protected override string GenerateName()
        {
            return string.Format(IsRepeater ?
                    SignalManager.CurrentPack.RepeaterFormat :
                    SignalManager.CurrentPack.DistantFormat,
                Home.InternalName, Id);
        }

        public override bool ShouldUpdate() => false;

        private void UpdateFromHome(Signal signal, IAspect? aspect) => Update(true, false);

        public override BasicSignalController? GetNextController()
        {
            return Home;
        }

        public static DistantSignalController? Replace(DistantSignalController original, SignalControllerDefinition def)
        {
            if (!original.PlacementInfo.HasValue) return null;

            var replacement = new DistantSignalController(def, original.Home, original.PlacementInfo.Value, original.Distance, original.IsRepeater)
            {
                Group = original.Group,
                ActingAsDistant = original.ActingAsDistant,
                ShortDistance = original.ShortDistance
            };
            original.Destroy();

            return replacement;
        }
    }
}
