using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Junction Path (Aspect)")]
    public class JunctionPathAspectDefinition : AspectBaseDefinition
    {
        public enum JunctionPathMode
        {
            AnyThrough,
            AnyDiverging,
            AllThrough,
            AllDiverging,
        }

        public JunctionPathMode PathMode = JunctionPathMode.AnyDiverging;

        private void Reset()
        {
            Id = "JUNCTION_PATH";
        }
    }
}
