using Signals.Common.States;
using System.Linq;

namespace Signals.Game.States
{
    public abstract class SignalStateBase
    {
        public SignalController Controller = null!;
        public SignalStateBaseDefinition Definition;

        private SignalLight[] _on = null!;
        private SignalLight[] _off = null!;
        private SignalLight[] _blink = null!;

        public string Id => Definition.Id;

        public SignalStateBase(SignalStateBaseDefinition def)
        {
            Definition = def;

            _on = def.OnLights.Select(x => x.GetController()).ToArray();
            _off = def.OffLights.Select(x => x.GetController()).ToArray();
            _blink = def.BlinkingLights.Select(x => x.GetController()).ToArray();
        }

        public abstract bool MeetsConditions();

        public void Apply()
        {
            foreach (SignalLight light in _on)
            {
                light.TurnOn(false);
            }

            foreach (SignalLight light in _off)
            {
                light.TurnOff();
            }

            foreach (SignalLight light in _blink)
            {
                light.TurnOn(true);
            }
        }
    }
}
