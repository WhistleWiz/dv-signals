using Signals.Common;
using Signals.Common.Animation;

namespace Signals.Unity.Validation
{
    internal class ComponentsValidator : ControllerValidatorBase
    {
        public override string Name => "Components";

        public override Result ValidateController(SignalControllerDefinition definition)
        {
            var tested = false;
            var result = Pass();

            foreach (var item in definition.GetComponentsInChildren<PlaySoundFromAnimationEvents>())
            {
                if (item.Source == null)
                {
                    result.AddFailure($"{item.name} - Source is null");
                }

                tested = true;
            }

            return tested ? result : Skip();
        }
    }
}
