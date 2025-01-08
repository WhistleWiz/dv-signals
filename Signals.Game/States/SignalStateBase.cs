using Signals.Common.States;
using System.Linq;

namespace Signals.Game.States
{
    public abstract class SignalStateBase
    {
        public SignalController Controller = null!;
        public SignalStateBaseDefinition Definition;

        private SignalLight[] _on = null!;
        private SignalLight[] _blink = null!;

        public string Id => Definition.Id;

        public SignalStateBase(SignalStateBaseDefinition def)
        {
            Definition = def;

            _on = def.OnLights.Select(x => x.GetController()).ToArray();
            _blink = def.BlinkingLights.Select(x => x.GetController()).ToArray();
        }

        /// <summary>
        /// Checks if the conditions for this state to be used are true.
        /// </summary>
        public abstract bool MeetsConditions();

        public void Apply()
        {
            foreach (SignalLight light in _on)
            {
                light.TurnOn(false);
            }

            foreach (SignalLight light in _blink)
            {
                light.TurnOn(true);
            }
        }
    }
}
