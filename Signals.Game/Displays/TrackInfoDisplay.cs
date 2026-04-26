using Signals.Common.Displays;
using Signals.Game.Railway;

namespace Signals.Game.Displays
{
    public class TrackInfoDisplay : InfoDisplay
    {
        private TrackInfoDisplayDefinition _fullDef;

        public TrackInfoDisplay(InfoDisplayDefinition definition, Signal signal) : base(definition, signal)
        {
            _fullDef = (TrackInfoDisplayDefinition)definition;
        }

        public override void UpdateDisplay()
        {
            DisplayText = _fullDef.NoValidResultValue;

            string text = GetText(Signal.Block, _fullDef.Format);

            DisplayText = string.IsNullOrEmpty(text) ? _fullDef.NoValidResultValue : text;
        }

        private static string GetText(TrackBlock? block, string format)
        {
            return block != null ? string.Format(format, block.Station, block.Yard, block.TrackNumber, block.TrackType) : string.Empty;
        }
    }
}
