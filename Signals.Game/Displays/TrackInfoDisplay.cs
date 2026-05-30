using Signals.Common.Displays;
using Signals.Game.Railway;
using Signals.Game.Util;

namespace Signals.Game.Displays
{
    public class TrackInfoDisplay : DisplayBase<TrackInfoDisplayDefinition>
    {
        public TrackInfoDisplay(DisplayBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override void UpdateDisplay()
        {
            string text = GetText(Signal.Block, Definition.Format);

            DisplayText = string.IsNullOrEmpty(text) ? Definition.NoValidResultValue : text;
        }

        private string GetText(TrackBlock? block, string format)
        {
            var isOut = !Controller.PlacementInfo.HasValue || Controller.PlacementInfo.Value.Direction.IsOut();

            if (Definition.FromPlacement)
            {
                if (!Controller.PlacementInfo.HasValue) goto Empty;

                var id = Controller.PlacementInfo.Value.Track.GetId();

                if (id.IsGeneric()) goto Empty;

                return string.Format(format, id.yardId, id.SignIDSubYardPart,
                    ReflectionHelpers.GetOrderNumber(id), ReflectionHelpers.GetTrimmedOrderNumber(id), ReflectionHelpers.GetTrackType(id),
                    Controller.OrientationSimple, Controller.Orientation, isOut ? "A" : "B");
            }

            if (block != null)
            {
                return string.Format(format, block.Station, block.Yard, block.TrackNumber, block.TrackTrimmerNumber, block.TrackType,
                    Controller.OrientationSimple, Controller.Orientation, isOut ? "A" : "B");
            }

        Empty:

            return string.Format(format, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                Controller.OrientationSimple, Controller.Orientation, isOut ? "A" : "B");
        }
    }
}
