using Signals.Common;
using System.Linq;
using UnityEngine;

namespace Signals.Unity.Validation
{
    internal class SignalValidator : SignalValidatorBase
    {
        public override string Name => "Signal";

        public override Result ValidateSignal(SignalDefinition definition)
        {
            var result = Pass();

            var colliders = definition.GetComponents<Collider>();

            if (colliders.Any(x => !x.isTrigger))
            {
                result.AddWarning($"{definition.name} - colliders for the signal are not set to trigger");
            }

            return result;
        }
    }
}
