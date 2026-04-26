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
        public override string Name => string.IsNullOrEmpty(NameOverride) ? $"{Home.Name}-D" : NameOverride;

        public DistantSignalController(SignalControllerDefinition def, BasicSignalController home,
            SignalPlacementInfo info, float distance) : base(def, info)
        {
            Home = home;
            Home.AnyAspectChanged += UpdateFromHome;

            Distance = distance;
            Type = SignalType.Distant;

            foreach (var signal in Signals)
            {
                signal.Block = TrackBlock.CreateForDistant(home, distance);
            }
        }

        public override bool ShouldUpdate() => false;

        private void UpdateFromHome(Signal signal, AspectBase? aspect) => Update(true, false);

        public override BasicSignalController? GetNextController()
        {
            return Home;
        }
    }
}
