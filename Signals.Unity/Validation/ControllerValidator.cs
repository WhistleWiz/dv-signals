using Signals.Common;
using System.Linq;

namespace Signals.Unity.Validation
{
    internal class ControllerValidator : ControllerValidatorBase
    {
        public override string Name => "Controller";

        public override Result ValidateController(SignalControllerDefinition definition)
        {
            if (definition.Signals.Length == 0 && definition.ShuntingSignals.Length == 0)
            {
                return Critical("No signals in controller");
            }

            for (int i = 0; i < definition.Signals.Length; i++)
            {
                var item = definition.Signals[i];

                if (item == null)
                {
                    return Critical($"Signal {i} is null");
                }
            }

            for (int i = 0; i < definition.ShuntingSignals.Length; i++)
            {
                var item = definition.ShuntingSignals[i];

                if (item == null)
                {
                    return Critical($"Shunting signal {i} is null");
                }
            }

            var result = Pass();

            if (definition.TracksideObjects.Any(x => x == null))
            {
                result.AddFailure("Trackside Objects has null entries");
            }

            return result;
        }
    }
}
