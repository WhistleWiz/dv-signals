using Signals.Common;
using Signals.Common.Displays;

namespace Signals.Unity.Validation
{
    internal class DisplayValidator : SignalValidatorBase
    {
        public override string Name => "Displays";

        public override Result ValidateSignal(SignalDefinition definition)
        {
            if (definition.Displays.Length == 0)
            {
                return Skip();
            }

            var result = Pass();

            for (int i = 0; i < definition.Displays.Length; i++)
            {
                var display = definition.Displays[i];

                if (display == null)
                {
                    result.AddFailure($"{definition.name} - display {i} is null");
                    continue;
                }

                result.Merge(ValidateDisplay(display, definition.name));
            }

            return result;
        }

        private Result ValidateDisplay(DisplayBaseDefinition display, string name)
        {
            if (display.Conditions.Length == 0)
            {
                return Skip();
            }

            var result = Pass();

            for (int i = 0; i < display.Conditions.Length; i++)
            {
                var condition = display.Conditions[i];

                if (condition == null)
                {
                    result.AddFailure($"{name}/{display.name} - condition {i} is null");
                    continue;
                }
            }

            return result;
        }
    }
}
