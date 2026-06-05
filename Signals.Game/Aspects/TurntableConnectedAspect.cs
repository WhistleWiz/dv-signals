using Signals.Common.Aspects;
using Signals.Game.Controllers;

namespace Signals.Game.Aspects
{
    public class TurntableConnectedAspect : AspectBase<TurntableConnectedAspectDefinition>
    {
        private TurntableSignalController _turntable;

        public TurntableConnectedAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal)
        {
            _turntable = (TurntableSignalController)Controller;
        }

        public override bool MeetsConditions()
        {
            if (_turntable == null) return false;

            return ApplyInvert(_turntable.IsConnected, Definition.Invert);
        }
    }
}
