using Signals.Common;

namespace Signals.Unity.Validation
{
    internal class ControllerValidator : ControllerValidatorBase
    {
        public override string Name => "Controller";

        public override Result ValidateController(SignalControllerDefinition definition)
        {
            for (int i = 0; i < definition.Signals.Length; i++)
            {
                var item = definition.Signals[i];

                if (item == null)
                {
                    return Critical($"signal {i} is null");
                }
            }

            return Pass();
        }
    }
}
