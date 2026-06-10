using UnityEngine;

namespace Signals.Common
{
    [AddComponentMenu("DV Signals/Signal Light Colour Changer")]
    public class SignalLightColourChangerDefinition : MonoBehaviour
    {
        public SignalLightDefinition Light = null!;
        public Color Colour = Color.white;
    }
}
