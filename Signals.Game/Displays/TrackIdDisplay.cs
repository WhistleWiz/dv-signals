using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    internal class TrackIdDisplay : InfoDisplay
    {
        private TrackIdDisplayDefinition _fullDef;

        public TrackIdDisplay(InfoDisplayDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (TrackIdDisplayDefinition)definition;
        }

        public override void UpdateDisplay()
        {
            string text = Controller switch
            {
                JunctionSignalController junction => GetJunctionText(_fullDef, junction),
                _ => string.Empty,
            };

            DisplayText = string.IsNullOrEmpty(text) ?  _fullDef.NoNumberValue : text;
        }

        private static string GetJunctionText(TrackIdDisplayDefinition def, JunctionSignalController junction)
        {
            if (junction.TrackInfo == null) return string.Empty;

            return def.TrackIDMode switch
            {
                TrackIdDisplayDefinition.TrackIdDisplayMode.NumberOnly => junction.TrackInfo.NextYardTrackNumber,
                TrackIdDisplayDefinition.TrackIdDisplayMode.NumberAndType => junction.TrackInfo.NextYardTrackSign,
                _ => string.Empty,
            };
        }
    }
}
