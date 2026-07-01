using Signals.Common;
using Signals.Game.Util;

namespace Signals.Game.Controllers
{
    public class BufferStopSignalController : BasicSignalController
    {
        private string _name;
        private bool _lastStatus;
        private bool _update;

        public BufferStop BufferStop { get; private set; }

        public BufferStopSignalController(SignalControllerDefinition def, string name, BufferStop stop, bool breakable) : base(def, null)
        {
            _name = name;
            _update = true;
            BufferStop = stop;

            if (breakable)
            {
                PreUpdate += CheckStopStatus;
            }
        }

        protected override string GenerateName()
        {
            return _name;
        }

        public override bool ShouldUpdate()
        {
            if (!_update) return false;

            _update = false;
            return true;
        }

        private void CheckStopStatus(BasicSignalController controller)
        {
            var isBroken = ReflectionHelpers.IsBroken(BufferStop);

            if (isBroken)
            {
                foreach (var signal in AllSignals)
                {
                    signal.TurnOff();
                }
            }
            else if (_lastStatus != isBroken)
            {
                _update = true;
            }

            _lastStatus = isBroken;
        }
    }
}
