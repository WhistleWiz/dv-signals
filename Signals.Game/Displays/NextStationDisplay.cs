using Signals.Common.Displays;

namespace Signals.Game.Displays
{
    public class NextStationDisplay : DisplayBase<NextStationDisplayDefinition>
    {
        private string? _startingStation;

        private string StartingStation
        {
            get
            {
                _startingStation ??= Signal.Controller.Group != null ? Signal.Controller.Group.Station : string.Empty;

                return _startingStation;
            }
        }

        public NextStationDisplay(DisplayBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override void UpdateDisplay()
        {
            var nextSignal = Signal.GetNextControllerCondition(x =>
            {
                if (x.Group == null) return false;

                var station = x.Group.Station;

                return station != StartingStation && !string.IsNullOrEmpty(station);
            });

            if (nextSignal == null || nextSignal.Group == null)
            {
                DisplayText = Definition.NoValidResultValue;
                return;
            }

            var name = nextSignal.Group.Station;

            DisplayText = Definition.DisplayMode switch
            {
                NextStationDisplayDefinition.StationDisplayMode.FirstLetter => name.Substring(0, 1),
                _ => name,
            };
        }
    }
}
