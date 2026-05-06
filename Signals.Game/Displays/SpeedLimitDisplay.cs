using Signals.Common.Displays;
using Signals.Game.Railway;
using UnityEngine;

namespace Signals.Game.Displays
{
    public class SpeedLimitDisplay : DisplayBase
    {
        private SpeedLimitDisplayDefinition _fullDef;

        public SpeedLimitDisplay(DisplayBaseDefinition def, Signal signal) : base(def, signal)
        {
            _fullDef = (SpeedLimitDisplayDefinition)def;
        }

        public override void UpdateDisplay()
        {
            var block = Signal.Block;

            if (block == null)
            {
                DisplayText = _fullDef.NoValidResultValue;
                return;
            }

            TrackInfo? first = null;
            var length = 0.0;
            var max = float.PositiveInfinity;

            foreach (var track in block.Tracks)
            {
                // Skip junction tracks.
                if (track.IsJunctionTrack) continue;

                if (!first.HasValue)
                {
                    first = track;
                }

                // Skip track if it is in a yard and we want to ignore those.
                if (_fullDef.IgnoreYards && track.Track.IsPartOfStation()) continue;

                // Get the maximum speed of the track.
                max = Mathf.Min(max, SpeedCalculator.GetSpeed(track.Track, track.Direction));
                length += track.Length;

                // If we walked enough, set the value.
                if (length >= SpeedCalculator.EndDistance)
                {
                    SetValue(max);
                    return;
                }
            }

            // If somehow there wasn't a track at all, it's not a valid result.
            if (!first.HasValue)
            {
                DisplayText = _fullDef.NoValidResultValue;
                return;
            }

            // We only reach here in case the option to skip yards was true, and there were no non-yard tracks at all.
            SetValue(SpeedCalculator.GetSpeed(first.Value.Track, first.Value.Direction));

            void SetValue(float value)
            {
                DisplayText = (_fullDef.DivideBy10 ? value / 10.0f : value).ToString("F0");
            }
        }
    }
}
