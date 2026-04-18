using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Junction Id")]
    public class JunctionIdDisplayDefinition : InfoDisplayDefinition
    {
        public enum IdDisplayMode
        {
            Full,
            NumbersOnly
        }

        public IdDisplayMode IdDisplay = IdDisplayMode.NumbersOnly;
    }
}
