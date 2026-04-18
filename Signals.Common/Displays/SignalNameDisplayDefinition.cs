using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Signal Name")]
    public class SignalNameDisplayDefinition : InfoDisplayDefinition
    {
        private void Reset()
        {
            Mode = UpdateMode.AtStart;
        }
    }
}
