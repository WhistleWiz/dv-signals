using Signals.Common;
using Signals.Game.Aspects;
using Signals.Game.Displays;
using Signals.Game.Railway;

namespace Signals.Game.Controllers
{
    internal class DistantSignalController : BasicSignalController
    {
        private BasicSignalController _home;

        public BasicSignalController Home
        {
            get => _home;
            protected set
            {
                _home.AspectChanged -= UpdateFromHome;
                _home.DisplaysUpdated -= UpdateDisplaysFromHome;
                _home = value;
                _home.AspectChanged += UpdateFromHome;
                _home.DisplaysUpdated += UpdateDisplaysFromHome;
            }
        }
        public float Distance { get; private set; }
        public override string Name => string.IsNullOrEmpty(NameOverride) ? $"{Home.Name}-D" : NameOverride;

        public DistantSignalController(SignalControllerDefinition def, BasicSignalController home,
            SignalPlacementInfo info, float distance) : base(def, info)
        {
            _home = home;

            _home.AspectChanged += UpdateFromHome;
            _home.DisplaysUpdated += UpdateDisplaysFromHome;

            Distance = distance;
            Type = SignalType.Distant;
            Block = TrackBlock.CreateForDistant(home, distance);
        }

        public override bool ShouldUpdate() => false;

        private void UpdateFromHome(AspectBase? aspect) => UpdateAspect(false);

        private void UpdateDisplaysFromHome(InfoDisplay[] obj) => UpdateDisplays(true);
    }
}
