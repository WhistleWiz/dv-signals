using UnityEngine;

namespace Signals.Game
{
    public interface IHudDisplayable
    {
        public bool ShouldDisplay { get; }
        public int DisplayOrder { get; }
        public string DisplayText { get; }
        public Sprite? Sprite { get; }
        public Color TextColour { get; }
    }
}
