using Signals.Common.Aspects;
using UnityEngine;

namespace Signals.Common
{
    public class SignalControllerDefinition : MonoBehaviour
    {
        [Tooltip("The state when no other states meet conditions")]
        public AspectBaseDefinition DefaultAspect = null!;
        [Tooltip("Each possible state for the signal\n" +
            "Order is important, as conditions are checked from first to last\n" +
            "Open state is used if none of these meet their condition")]
        public AspectBaseDefinition[] OtherAspects = new AspectBaseDefinition[0];

        [Header("Optional")]
        public Sprite? OffStateHUDSprite;
        [Tooltip("Used for mechanical signals")]
        public Animator? Animator;
    }
}
