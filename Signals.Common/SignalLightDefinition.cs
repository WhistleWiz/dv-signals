using UnityEngine;

namespace Signals.Common
{
    [RequireComponent(typeof(MeshRenderer))]
    public class SignalLightDefinition : MonoBehaviour
    {
        public MeshRenderer Renderer = null!;
        public Color Color = Color.white;

        [Header("Optional"), Tooltip("Centre position of the light's glare\n" +
            "Positive Z points to the visible direction")]
        public Transform? Glare;

        private void Reset()
        {
            Renderer = GetComponent<MeshRenderer>();
        }
    }
}
