using Signals.Common;

namespace Signals.Game.Controllers
{
    /// <summary>
    /// A controller for a static signal.
    /// </summary>
    public class StaticSignalController : BasicSignalController
    {
        private string _name;
        private bool _updated;

        public StaticSignalController(SignalControllerDefinition def, string name) : base(def, null)
        {
            _name = name;
        }

        protected override string GenerateName()
        {
            return _name;
        }

        public override bool ShouldUpdate()
        {
            if (_updated) return false;

            _updated = true;
            return true;
        }
    }
}
