using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Move Swap (Display)")]
    public class MoveSwapDisplayDefinition : DisplayBaseDefinition
    {
        public DisplayBaseDefinition ActualDisplay = null!;
        public TransformMoverTarget Mover = null!;
    }
}
