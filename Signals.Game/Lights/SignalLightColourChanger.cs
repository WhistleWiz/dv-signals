using Signals.Common;

namespace Signals.Game.Lights
{
    public class SignalLightColourChanger
    {
        private SignalLight _light;

        public SignalLightColourChangerDefinition Definition { get; private set; }

        public SignalLightColourChanger(SignalLightColourChangerDefinition definition, Signal signal)
        {
            Definition = definition;
            _light = definition.Light.GetController(signal);
        }

        public void Apply()
        {
            _light.ChangeColour(Definition);
        }

        public void Unapply()
        {
            _light.ChangeColour(null);
        }
    }
}
