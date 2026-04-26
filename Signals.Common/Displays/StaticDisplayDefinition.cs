using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Static (Display)")]
    public class StaticDisplayDefinition : InfoDisplayDefinition
    {
        public string DisplayedText = string.Empty;

        private void Reset()
        {
            Mode = UpdateMode.AtStart;
        }
    }
}
