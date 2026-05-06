using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Junction Id (Display)")]
    public class JunctionIdDisplayDefinition : DisplayBaseDefinition
    {
        public enum IdDisplayMode
        {
            Full,
            NumbersOnly
        }

        public IdDisplayMode IdDisplay = IdDisplayMode.NumbersOnly;
    }
}
