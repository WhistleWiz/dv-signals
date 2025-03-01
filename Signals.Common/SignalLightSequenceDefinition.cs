using System;
using UnityEngine;

namespace Signals.Common
{
    public class SignalLightSequenceDefinition : MonoBehaviour
    {
        public SignalLightDefinition?[] Lights = new SignalLightDefinition?[0];
        public bool[] States = new bool[0];
        public float Timing = 0.2f;
        [Tooltip("Controls the amount of states moved per update"), Min(1)]
        public int Step = 1;

        private void OnValidate()
        {
            Array.Resize(ref States, Lights.Length);
        }
    }
}
