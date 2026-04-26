using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Signal ID (Display)")]
    public class SignalIdDisplayDefinition : InfoDisplayDefinition
    {
        private void Reset()
        {
            Mode = UpdateMode.AtStart;
        }
    }
}
