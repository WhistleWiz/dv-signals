using Signals.Common;

namespace Signals.Game.Controllers
{
    /// <summary>
    /// A controller for a static signal.
    /// </summary>
    public class StaticSignalController : BasicSignalController
    {
        private string _name;

        public StaticSignalController(SignalControllerDefinition def, string name) : base(def, null)
        {
            _name = name;
            Update(true, false);
        }

        protected override string GenerateName()
        {
            return _name;
        }

        public override bool ShouldUpdate() => false;
    }
}
