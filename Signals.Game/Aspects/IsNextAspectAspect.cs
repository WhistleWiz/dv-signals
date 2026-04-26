using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class IsNextAspectAspect : AspectBase
    {
        private IsNextAspectAspectDefinition _fullDef;

        public IsNextAspectAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal)
        {
            _fullDef = (IsNextAspectAspectDefinition)definition;
        }

        public override bool MeetsConditions()
        {
            var next = Signal.GetNextController();

            if (next == null) return false;

            var signal = next.GetControllerSignal();

            if (signal == null) return false;

            var state = signal.CurrentAspect;

            // Turned off signal can never meet conditions.
            return state != null && state.Id == _fullDef.NextId;
        }
    }
}
