using DV.WeatherSystem;
using Signals.Common;
using Signals.Game.Controllers;

namespace Signals.Game.Lights
{
    public class SignalNightLight : SignalLight
    {
        // From AutomaticHeadlightController.
        private const float DAYTIME_START = 7f / 24f;
        private const float DAYTIME_END = 20f / 24f;

        public override void Initialize(SignalLightDefinition def, Signal signal)
        {
            base.Initialize(def, signal);

            signal.Controller.PreUpdate += CheckStateNight;
        }

        private void OnDestroy()
        {
            Signal.Controller.PreUpdate -= CheckStateNight;
        }

        public override void TurnOn(bool blink = false)
        {
            InternalState = blink ? LampControl.LampState.Blinking : LampControl.LampState.On;

            if (!Definition.NightOnly || MeetsNightConditions())
            {
                UpdateFromState();
            }
            else
            {
                ForceOff();
            }
        }

        private bool MeetsNightConditions()
        {
            var weather = WeatherDriver.Instance;

            if (weather == null) return false;

            float timeOfDay = weather.manager.timeOfDay;

            if (timeOfDay <= DAYTIME_START || timeOfDay >= DAYTIME_END) return true;

            if (weather.GetFogDensity(Signal.Controller.Definition.transform.position) > 0.5f) return true;

            return false;
        }

        private void CheckStateNight(BasicSignalController controller)
        {
            if (MeetsNightConditions())
            {
                UpdateFromState();
            }
            else
            {
                ForceOff();
            }
        }
    }
}
