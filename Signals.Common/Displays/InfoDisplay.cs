using UnityEngine;

namespace Signals.Common.Displays
{
    public abstract class InfoDisplay : MonoBehaviour
    {
        public enum DisplayMode
        {
            Both,
            HUDOnly,
            WorldOnly
        }

        public DisplayMode Mode;
        [Tooltip("The background of this display when hovered")]
        public Sprite HUDBackground = null!;
    }
}
