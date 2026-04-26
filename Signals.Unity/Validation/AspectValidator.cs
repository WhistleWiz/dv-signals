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

            if (aspect.OnLights.Any(x => x == null))
            {
                result.AddFailure($"{name}/{aspect.Id} - On Lights has null entries");
            }

            if (aspect.BlinkingLights.Any(x => x == null))
            {
                result.AddFailure($"{name}/{aspect.Id} - Blinking Lights has null entries");
            }

            if (aspect.LightSequences.Any(x => x == null))
            {
                result.AddFailure($"{name}/{aspect.Id} - Light Sequences has null entries");
            }

            return result;
        }
    }
}
