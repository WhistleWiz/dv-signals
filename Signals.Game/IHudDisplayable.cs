using UnityEngine;

namespace Signals.Game
{
    public interface IHudDisplayable
    {
        public int DisplayOrder { get; }
        public string? DisplayText { get; }
        public Sprite? Sprite { get; }
        public Color TextColour { get; }
    }
}
