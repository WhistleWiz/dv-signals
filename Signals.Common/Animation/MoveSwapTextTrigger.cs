using System;
using UnityEngine;

namespace Signals.Common.Animation
{
    [AddComponentMenu("DV Signals/Animation/Move Swap Text Trigger")]
    public class MoveSwapTextTrigger
    {
        public Action? OnChange;

        public void ChangeText(bool value)
        {
            OnChange?.Invoke();
        }
    }
}
