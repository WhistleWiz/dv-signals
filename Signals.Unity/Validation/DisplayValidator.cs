using Signals.Common;
using Signals.Common.Displays;
using System.Xml.Linq;
using UnityEngine;

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
            var result = Pass();
            name = $"{name}/{display.name}";

            for (int i = 0; i < display.Conditions.Length; i++)
            {
                var condition = display.Conditions[i];

                if (condition == null)
                {
                    result.AddFailure($"{name} - condition {i} is null");
                    continue;
                }
            }

            if (display is MoveSwapDisplayDefinition moveSwap)
            {
                ValidateMoveSwap(name, moveSwap, result);
            }

            return result;
        }

        private void ValidateMoveSwap(string name, MoveSwapDisplayDefinition moveSwap, Result result)
        {
            if (moveSwap.ActualDisplay == null)
            {
                result.AddFailure($"{name} - Actual Display is null");
            }
            else
            {
                result.Merge(ValidateDisplay(moveSwap.ActualDisplay, name));
            }

            if (moveSwap.Mover == null)
            {
                result.AddFailure($"{name} - Mover is null");
            }
        }
    }
}
