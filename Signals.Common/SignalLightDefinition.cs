using UnityEngine;

namespace Signals.Common
{
    [AddComponentMenu("DV Signals/Signal Light")]
    public class SignalLightDefinition : MonoBehaviour
    {
        public Renderer[] Renderers = new Renderer[0];
        public Color Colour = Color.white;
        [Tooltip("How strong the light is")]
        public float LightIntensity = 2.5f;
        [Tooltip("How long it takes for the light to turn on")]
        public float Lag = 0.2f;
        [Tooltip("If true, the lamp will only turn on at night")]
        public bool NightOnly = false;

        [Header("Optional"), Tooltip("Centre position of the light's glare\n" +
            "Positive Z points to the visible direction")]
        public Transform? Glare;
        public Light? Light;

        private void Reset()
        {
            Renderers = GetComponentsInChildren<Renderer>();
        }
    }
}
