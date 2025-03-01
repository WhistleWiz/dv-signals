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
            string text = GetText(_fullDef, Controller.TrackInfo);

            DisplayText = string.IsNullOrEmpty(text) ? _fullDef.NoValidResultValue : text;
        }

        private static string GetText(TrackIdDisplayDefinition def, TrackInfo? info)
        {
            if (info == null) return string.Empty;

            return def.TrackIDMode switch
            {
                TrackIdDisplayDefinition.TrackIdDisplayMode.NumberOnly => info.NextTrackNumber,
                TrackIdDisplayDefinition.TrackIdDisplayMode.NumberAndType => info.NextTrackNumberType,
                TrackIdDisplayDefinition.TrackIdDisplayMode.YardAndNumberAndType => info.NextTrackYardNumberType,
                TrackIdDisplayDefinition.TrackIdDisplayMode.YardAndNumber => info.NextTrackYardNumber,
                TrackIdDisplayDefinition.TrackIdDisplayMode.YardOnly => info.NextTrackYard,
                _ => string.Empty,
            };
        }
    }
}
