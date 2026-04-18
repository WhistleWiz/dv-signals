using Signals.Common.Displays;
using Signals.Game.Controllers;
using Signals.Game.Railway;

namespace Signals.Game.Displays
{
    public class NextStationDisplay : InfoDisplay
    {
        private NextStationDisplayDefinition _fullDef;
        private string? _startingStation;

        private string StartingStation
        {
            get
            {
                _startingStation ??= Controller.Group != null ? TrackUtils.JunctionStation(Controller.Group.Junction) : string.Empty;

                return _startingStation;
            }
        }

        public NextStationDisplay(InfoDisplayDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (NextStationDisplayDefinition)definition;
        }

        public override void UpdateDisplay()
        {
            var nextSignal = Controller.GetNextSignalCondition(x =>
            {
                if (x.Group == null) return false;

                var station = TrackUtils.JunctionStation(x.Group.Junction);

                return station != StartingStation && !string.IsNullOrEmpty(station);
            });

            if (nextSignal == null || nextSignal.Group == null)
            {
                DisplayText = _fullDef.NoValidResultValue;
                return;
            }

            var name = TrackUtils.JunctionStation(nextSignal.Group.Junction);

            DisplayText = _fullDef.DisplayMode switch
            {
                NextStationDisplayDefinition.StationDisplayMode.FirstLetter => name.Substring(0, 1),
                _ => name,
            };
        }
    }
}
