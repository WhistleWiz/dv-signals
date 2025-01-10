using Signals.Common.States;
using UnityEngine;

namespace Signals.Common
{
    public class SignalControllerDefinition : MonoBehaviour
    {
        [Tooltip("The state when no other states meet conditions")]
        public OpenSignalStateDefinition OpenState = null!;
        [Tooltip("Each possible state for the signal\n" +
            "Order is important, as conditions are checked from first to last\n" +
            "Open state is used if none of these meet their condition")]
        public SignalStateBaseDefinition[] OtherStates = new SignalStateBaseDefinition[0];

        [Header("Optional")]
        public Sprite? OffStateHUDSprite;
        [Tooltip("Used for mechanical signals")]
        public Animator? Animator;
    }
}
