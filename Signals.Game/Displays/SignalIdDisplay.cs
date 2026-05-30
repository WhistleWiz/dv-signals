using Signals.Common.Displays;

namespace Signals.Game.Displays
{
    public class SignalIdDisplay : DisplayBase<SignalIdDisplayDefinition>
    {
        public SignalIdDisplay(DisplayBaseDefinition def, Signal signal) : base(def, signal) { }

        public override void UpdateDisplay()
        {
            string text;
            var junction = Controller.GroupJunction;

            if (Definition.WithStation && junction != null)
            {
                var station = junction.GetStation();
                text = $"{(string.IsNullOrEmpty(station) ? "W" : station)}-{Signal.Id}";
            }
            else
            {
                text = Signal.Id.ToString();
            }

            DisplayText = text;
        }
    }
}
