using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Max Speed (Aspect)")]
    public class MaxSpeedAspectDefinition : AspectBaseDefinition
    {
        [Tooltip("The maximum speed that sets the condition to true")]
        public float Maximum = 30;
        [Tooltip("Ignore yard tracks\n" +
            "If there are only yard tracks, then their limit will be used still")]
        public bool IgnoreYards = false;
        [Tooltip("Automatically adjusts the passing speed to match the maximum speed")]
        public bool DynamicPassingSpeed = true;

        private void Reset()
        {
            Id = "MAX_SPEED_30";
        }
    }
}
