using Signals.Common.Displays;
using UnityEngine;

namespace Signals.Game.Displays
{
    public class MoveSwapDisplay : DisplayBase<MoveSwapDisplayDefinition>
    {
        private static readonly int AnimatorHash = Animator.StringToHash("Change");

        private IDisplay _actualDisplay;
        private bool _inPosition;

        public MoveSwapDisplay(DisplayBaseDefinition definition, Signal signal) : base(definition, signal)
        {
            _actualDisplay = DisplayCreator.Create(signal, Definition.ActualDisplay)!;

            if (_actualDisplay == null)
            {
                throw new System.ArgumentException(nameof(Definition.ActualDisplay));
            }

            if (Definition.Mover != null)
            {
                Definition.Mover.Mover.OnReachTarget += SwapAndUnmove;
            }

            if (Definition.AnimationTrigger != null)
            {
                Definition.AnimationTrigger.OnChange += UpdateTextAndReturn;
            }
        }

        public override void UpdateDisplay()
        {
            _actualDisplay.UpdateDisplay();

            if (_actualDisplay.DisplayText != DisplayText)
            {
                Definition.Mover?.Apply();
                Definition.Animator?.SetBool(AnimatorHash, true);

                if (_inPosition && Definition.HoldOnInvalid && _actualDisplay.DisplayText != Definition.InvalidText)
                {
                    UpdateTextAndReturn();
                }
            }
        }

        private void SwapAndUnmove(bool isOriginal)
        {
            if (isOriginal) return;

            UpdateTextAndReturn();
        }

        private void UpdateTextAndReturn()
        {
            _inPosition = true;
            DisplayText = _actualDisplay.DisplayText;

            if (Definition.HoldOnInvalid && _actualDisplay.DisplayText == Definition.InvalidText) return;

            Signal.UpdateHoverDisplay();
            Definition.Mover?.Unapply();
            Definition.Animator?.SetBool(AnimatorHash, false);
            _inPosition = false;
        }
    }
}
