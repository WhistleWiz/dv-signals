using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Signal ID (Display)")]
    public class SignalIdDisplayDefinition : DisplayBaseDefinition
    {
        [Tooltip("If true, includes the station where the signal is placed ('W' if outside a station)")]
        public bool WithStation = false;

        private void Reset()
        {
            Mode = UpdateMode.AtStart;
        }
    }
}
