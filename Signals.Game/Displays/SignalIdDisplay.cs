using Signals.Common.Displays;

namespace Signals.Game.Displays
{
    public class SignalIdDisplay : InfoDisplay
    {
        private SignalIdDisplayDefinition _fullDef;

        public SignalIdDisplay(InfoDisplayDefinition def, Signal signal) : base(def, signal)
        {
            _fullDef = (SignalIdDisplayDefinition)def;
        }

        public override void UpdateDisplay()
        {
            string text;
            var junction = Controller.GroupJunction;

            if (_fullDef.WithJunction && junction != null)
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
