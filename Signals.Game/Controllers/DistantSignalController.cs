using Signals.Common;
using Signals.Game.Aspects;
using Signals.Game.Displays;

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
                _home.OnAspectChanged -= UpdateFromHome;
                _home.OnDisplaysUpdated -= UpdateDisplaysFromHome;
                value.OnAspectChanged += UpdateFromHome;
                value.OnDisplaysUpdated += UpdateDisplaysFromHome;
                _home = value;
            }
        }

        public DistantSignalController(BasicSignalController home, SignalControllerDefinition def) : base(def)
        {
            _home = home;

            _home.OnAspectChanged += UpdateFromHome;
            _home.OnDisplaysUpdated += UpdateDisplaysFromHome;
        }

        public override void UpdateAspect()
        {
            if (Home is JunctionSignalController junctionSignal)
            {
                TrackInfo = TrackInfo.NextSignalTrackInfo(Home, junctionSignal.Junction);
            }
            else
            {
                TrackInfo = TrackInfo.NextSignalTrackInfo(Home);
            }

            base.UpdateAspect();
        }

        private void UpdateFromHome(AspectBase? aspect)
        {
            UpdateAspect();
        }

        private void UpdateDisplaysFromHome(InfoDisplay[] obj)
        {
            UpdateDisplays();
        }
    }
}
