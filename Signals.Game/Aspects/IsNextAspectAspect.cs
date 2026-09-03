using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class IsNextAspectAspect : AspectBase<IsNextAspectAspectDefinition>
    {
        public IsNextAspectAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var next = Signal.GetNextController();

            if (next == null) return false;

            var signal = next.GetControllerSignal();

            if (signal == null) return false;

            var state = signal.CurrentAspect;

            // Turned off signal can never meet conditions.
            return state != null && state.Id == Definition.NextId;
        }
    }
}
