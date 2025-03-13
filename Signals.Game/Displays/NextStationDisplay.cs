using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    internal class NextStationDisplay : InfoDisplay
    {
        private NextStationDisplayDefinition _fullDef;

        public NextStationDisplay(InfoDisplayDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (NextStationDisplayDefinition)definition;
        }

        public override void UpdateDisplay()
        {
            if (Controller.TrackInfo == null)
            {
                DisplayText = _fullDef.NoValidResultValue;
            }
            else
            {
                var info = Controller.TrackInfo;
                var text = Controller.TrackInfo.NextStation;

                if (_fullDef.SearchMode == NextStationDisplayDefinition.StationSearchMode.UntilFound &&
                    string.IsNullOrEmpty(text) &&
                    info.LastDirection.HasValue)
                {
                    text = TrackUtils.NextStation(TrackWalker.Walk(info.LastTrack, info.LastDirection.Value));
                }

                DisplayText = string.IsNullOrEmpty(text) ? _fullDef.NoValidResultValue : text;
            }
        }
    }
}
