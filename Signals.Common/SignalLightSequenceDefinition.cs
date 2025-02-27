using System;
using UnityEngine;

namespace Signals.Common
{
    public class SignalLightSequenceDefinition : MonoBehaviour
    {
        public SignalLightDefinition?[] Lights = new SignalLightDefinition?[0];
        public bool[] States = new bool[0];
        public float Timing = 0.2f;

        private void OnValidate()
        {
            Array.Resize(ref States, Lights.Length);
        }
    }
}
