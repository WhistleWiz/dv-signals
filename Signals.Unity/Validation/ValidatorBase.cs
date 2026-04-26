using Signals.Common;

namespace Signals.Unity.Validation
{
    internal interface IValidatorBase
    {
        public string Name { get; }
        public Result ValidateController(SignalControllerDefinition definition);
    }

    internal abstract class ControllerValidatorBase : IValidatorBase
    {
        public abstract string Name { get; }
        public abstract Result ValidateController(SignalControllerDefinition definition);

        public Result Pass()
        {
            return new Result(Name);
        }

        public Result Warning(string message)
        {
            var result = new Result(Name);
            result.AddWarning(message);
            return result;
        }

        public Result Fail(string message)
        {
            var result = new Result(Name);
            result.AddFailure(message);
            return result;
        }

        public Result Critical(string message)
        {
            var result = new Result(Name);
            result.AddCritical(message);
            return result;
        }

        public Result Skip()
        {
            return Result.Skip(Name);
        }
    }

    internal abstract class SignalValidatorBase : ControllerValidatorBase
    {
        public override Result ValidateController(SignalControllerDefinition definition)
        {
            var result = Pass();

            foreach (var signal in definition.Signals)
            {
                if (signal == null)
                {
                    return Fail("null signals in controller");
                }

                result.Merge(ValidateSignal(signal));
            }

            if (definition.ShuntingSignal != null)
            {
                result.Merge(ValidateSignal(definition.ShuntingSignal));
            }

            return result;
        }

        public abstract Result ValidateSignal(SignalDefinition definition);
    }
}
