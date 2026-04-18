using Signals.Common.Aspects;
using Signals.Common.Displays;
using UnityEngine;

namespace Signals.Common
{
    [AddComponentMenu("DV Signals/Signal Subcontroller")]
    public class SubsignalControllerDefinition : MonoBehaviour
    {
        [Tooltip("Each possible state for the signal\n" +
            "Order is important, as conditions are checked from first to last\n" +
            "The signal is turned off if none of these meet their conditions")]
        public AspectBaseDefinition[] Aspects = new AspectBaseDefinition[0];
        public Sprite? OffStateHUDSprite;

        [Header("Optional")]
        [Tooltip("Displays that aren't part of aspects")]
        public InfoDisplayDefinition[] Displays = new InfoDisplayDefinition[0];
    }
}
