using Signals.Common.Animation;
using UnityEngine;

namespace Signals.Common.Displays
{
    [AddComponentMenu("DV Signals/Displays/Move Swap (Display)")]
    public class MoveSwapDisplayDefinition : DisplayBaseDefinition
    {
        public DisplayBaseDefinition ActualDisplay = null!;
        public TransformMoverTarget? Mover;
        public Animator? Animator;
        public MoveSwapTextTrigger? AnimationTrigger;
        public bool HoldOnInvalid = false;
        public string InvalidText = string.Empty;
    }
}
