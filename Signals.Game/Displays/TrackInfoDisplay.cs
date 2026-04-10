using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    internal class TrackInfoDisplay : InfoDisplay
    {
        private TrackInfoDisplayDefinition _fullDef;

        public TrackInfoDisplay(InfoDisplayDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (TrackInfoDisplayDefinition)definition;
        }

        public override void UpdateDisplay()
        {
            DisplayText = _fullDef.NoValidResultValue;

            string text = GetText(Controller.Block, _fullDef.Format);

            DisplayText = string.IsNullOrEmpty(text) ? _fullDef.NoValidResultValue : text;
        }

        private static string GetText(TrackBlock? block, string format)
        {
            return block != null ? string.Format(format, block.Station, block.Yard, block.TrackNumber, block.TrackType) : string.Empty;
        }
    }
}
