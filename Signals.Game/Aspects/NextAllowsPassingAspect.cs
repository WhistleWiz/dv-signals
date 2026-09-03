using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class NextAllowsPassingAspect : AspectBase<NextAllowsPassingAspectDefinition>
    {
        public NextAllowsPassingAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var next = Signal.GetNextController();

            if (next == null) return Definition.NoAspectResult;

            var signal = Definition.Shunting ? next.GetControllerShuntingSignal() : next.GetControllerSignal();

            if (signal == null) return Definition.NoAspectResult;

            var state = signal.CurrentAspect;

            if (state == null) return Definition.NoAspectResult;

            return ApplyInvert(!state.DisallowPassing, Definition.Invert);
        }
    }
}
