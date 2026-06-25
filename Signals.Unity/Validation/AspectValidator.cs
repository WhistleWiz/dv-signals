using Signals.Common;
using Signals.Common.Aspects;
using System.Linq;

namespace Signals.Unity.Validation
{
    internal class AspectValidator : SignalValidatorBase
    {
        public override string Name => "Aspects";

        public override Result ValidateSignal(SignalDefinition definition)
        {
            if (definition.Aspects.Length == 0)
            {
                return Warning($"{definition.name} - signal should have at least 1 aspect");
            }

            var result = Pass();

            for (int i = 0; i < definition.Aspects.Length; i++)
            {
                var aspect = definition.Aspects[i];

                if (aspect == null)
                {
                    result.AddFailure($"{definition.name} - aspect {i} is null");
                    continue;
                }

                result.Merge(CheckAspect(aspect, definition.name));
            }

            return result;
        }

        private Result CheckAspect(AspectBaseDefinition aspect, string name)
        {
            var result = Pass();
            name = $"{name}/{aspect.Id}";

            // Check nulls.
            if (aspect.OnLights.Any(x => x == null))
            {
                result.AddFailure($"{name} - On Lights has null entries");
            }

            if (aspect.BlinkingLights.Any(x => x == null))
            {
                result.AddFailure($"{name} - Blinking Lights has null entries");
            }

            if (aspect.LightSequences.Any(x => x == null))
            {
                result.AddFailure($"{name} - Light Sequences has null entries");
            }

            if (aspect.ColourChangers.Any(x => x == null))
            {
                result.AddFailure($"{name} - Colour Changers has null entries");
            }

            if (aspect.Movers.Any(x => x == null))
            {
                result.AddFailure($"{name} - Movers has null entries");
            }

            // Check overlaps between on/blinking/sequences.
            foreach (var light in aspect.OnLights)
            {
                if (light == null) continue;

                if (aspect.BlinkingLights.Contains(light))
                {
                    result.AddWarning($"{name} - on light {light.name} overlaps with Blinking Lights");
                }

                if (aspect.LightSequences.Any(x => x != null && x.Lights.Contains(light)))
                {
                    result.AddWarning($"{name} - on light {light.name} overlaps with Light Sequences");
                }
            }

            foreach (var light in aspect.BlinkingLights)
            {
                if (light == null) continue;

                if (aspect.LightSequences.Any(x => x != null && x.Lights.Contains(light)))
                {
                    result.AddWarning($"{name} - blinking light {light.name} overlaps with Light Sequences");
                }
            }

            foreach (var cChanger in aspect.ColourChangers)
            {
                if (cChanger == null) continue;

                if (cChanger.Light == null)
                {
                    result.AddFailure($"{name}/{cChanger} - light is not assigned");
                }
            }

            foreach (var mover in aspect.Movers)
            {
                if (mover == null) continue;

                if (mover.Mover == null)
                {
                    result.AddFailure($"{name}/{mover} - mover is not assigned");
                }
            }

            if (aspect is CombinationAspectDefinition combination)
            {
                ValidateCombination(name, combination, result);
            }

            return result;
        }

        private void ValidateCombination(string name, CombinationAspectDefinition combination, Result result)
        {
            // Check child aspects for combinations.
            for (int i = 0; i < combination.Conditions.Length; i++)
            {
                var condition = combination.Conditions[i];

                if (condition == null)
                {
                    result.AddFailure($"{name} - condition {i} is null");
                    continue;
                }

                result.Merge(CheckAspect(condition, name));
            }
        }
    }
}
