using Signals.Common.Aspects;
using Signals.Common.Displays;
using UnityEngine;

namespace Signals.Common
{
    [AddComponentMenu("DV Signals/Signal")]
    public class SignalDefinition : MonoBehaviour
    {
        [Tooltip("Each possible state for the signal\n" +
            "Order is important, as conditions are checked from first to last\n" +
            "The signal is turned off if none of these meet their conditions")]
        public AspectBaseDefinition[] Aspects = new AspectBaseDefinition[0];
        [Tooltip("The sprite of this signal when hovered while turned off\n" +
            "The signal is not shown in the HUD when this is null")]
        public Sprite? OffStateHUDSprite;
        [Tooltip("Lower values are placed above higher values")]
        public int HUDDisplayOrder = 0;

        [Header("Optional")]
        [Tooltip("Displays that aren't part of aspects")]
        public InfoDisplayDefinition[] Displays = new InfoDisplayDefinition[0];
        [Tooltip("Extra aspects that do not interfere with the main ones")]
        public AspectBaseDefinition[] Indicators = new AspectBaseDefinition[0];
        [Tooltip("An optional distant signal\n" +
            "This is automatically enabled or disabled based on the signal distance")]
        public SignalDefinition? DistantSignal;
    }
}
