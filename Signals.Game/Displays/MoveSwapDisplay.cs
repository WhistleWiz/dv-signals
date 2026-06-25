using Signals.Common.Displays;

namespace Signals.Game.Displays
{
    public class MoveSwapDisplay : DisplayBase<MoveSwapDisplayDefinition>
    {
        private IDisplay _actualDisplay;

        public MoveSwapDisplay(DisplayBaseDefinition definition, Signal signal) : base(definition, signal)
        {
            _actualDisplay = DisplayCreator.Create(signal, Definition.ActualDisplay)!;

            if (_actualDisplay == null)
            {
                throw new System.ArgumentException(nameof(Definition.ActualDisplay));
            }

            Definition.Mover.Mover.OnReachTarget += SwapAndUnmove;
        }

        public override void UpdateDisplay()
        {
            _actualDisplay.UpdateDisplay();

            if (_actualDisplay.DisplayText != DisplayText)
            {
                Definition.Mover.Apply();
            }
        }

        private void SwapAndUnmove(bool isOriginal)
        {
            if (isOriginal) return;

            DisplayText = _actualDisplay.DisplayText;
            Signal.UpdateHoverDisplay();
            Definition.Mover.Unapply();
        }
    }
}
