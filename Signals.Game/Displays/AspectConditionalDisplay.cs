using Signals.Common.Displays;
using System.Linq;

namespace Signals.Game.Displays
{
    internal class AspectConditionalDisplay : DisplayBase<AspectConditionalDisplayDefinition>
    {
        private IDisplay _actualDisplay;

        public AspectConditionalDisplay(DisplayBaseDefinition definition, Signal signal) : base(definition, signal)
        {
            _actualDisplay = DisplayCreator.Create(signal, Definition.ActualDisplay)!;

            if (_actualDisplay == null)
            {
                throw new System.ArgumentException(nameof(Definition.ActualDisplay));
            }
        }

        public override void UpdateDisplay()
        {
            if (Signal.CurrentAspect == null || !Definition.AllowedAspectIds.Contains(Signal.CurrentAspect.Id))
            {
                DisplayText = Definition.NoValidResultValue;
            }
            else
            {
                _actualDisplay.UpdateDisplay();
                DisplayText = _actualDisplay.DisplayText;
            }
        }
    }
}
