using Signals.Common.Displays;
using Signals.Game.Controllers;
using System;

namespace Signals.Game.Displays
{
    internal class DistanceToNextDisplay : InfoDisplay
    {
        private DistantSignalController? _distant;

        public DistanceToNextDisplay(InfoDisplayDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _distant = (DistantSignalController)controller;
        }

        public override void UpdateDisplay()
        {
            var distance = _distant != null ? _distant.Distance : (Controller.TrackInfo != null ? Controller.TrackInfo.DistanceWalked : 0);
            var rounded = Math.Round(distance * 0.001, 1);
            var text = rounded > 0 ? $"{rounded}" : string.Empty;

            if (text != DisplayText)
            {
                DisplayText = text;
            }
        }
    }
}
