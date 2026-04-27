using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Signal ID (Display)")]
    public class SignalIdDisplayDefinition : InfoDisplayDefinition
    {
        public bool WithJunction = false;

        private void Reset()
        {
            Mode = UpdateMode.AtStart;
        }
    }
}
