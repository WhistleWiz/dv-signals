using Signals.Common.Displays;

namespace Signals.Game.Displays
{
    public class SignalIdDisplay : DisplayBase<SignalIdDisplayDefinition>
    {
        public SignalIdDisplay(DisplayBaseDefinition def, Signal signal) : base(def, signal) { }

        public override void UpdateDisplay()
        {
            var group = Controller.Group;

            if (Definition.WithStation && group != null)
            {
                var station = group.Station;
                DisplayText = $"{(string.IsNullOrEmpty(station) ? "W" : station)}-{Signal.Id}";
            }
            else
            {
                DisplayText = Signal.Id.ToString();
            }
        }
    }
}
