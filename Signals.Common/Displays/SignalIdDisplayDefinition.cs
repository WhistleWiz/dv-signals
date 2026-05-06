using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Signal ID (Display)")]
    public class SignalIdDisplayDefinition : DisplayBaseDefinition
    {
        public bool WithJunction = false;

        private void Reset()
        {
            Mode = UpdateMode.AtStart;
        }
    }
}
