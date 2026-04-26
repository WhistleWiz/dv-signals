using UnityEngine;

namespace Signals.Common
{
    public interface IHudDisplayable
    {
        public int DisplayOrder { get; }
        public string? DisplayText { get; }
        public Sprite? Sprite { get; }
        public Color TextColour { get; }
    }
}
