using UnityEngine;

namespace Signals.Common.Aspects
{
    [AddComponentMenu("DV Signals/Aspects/Block Length (Aspect)")]
    public class BlockLengthAspectDefinition : AspectBaseDefinition
    {
        public float Length = 500;
        public OperationMode OperationMode = OperationMode.LessThan;

        private void Reset()
        {
            Id = "BLOCK_LENGTH";
        }
    }
}
