using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    public class JunctionIdDisplay : InfoDisplay
    {
        private JunctionIdDisplayDefinition _fullDef;
        private Junction? _junction;

        public JunctionIdDisplay(InfoDisplayDefinition definition, Signal signal) : base(definition, signal)
        {
            _fullDef = (JunctionIdDisplayDefinition)definition;

            if (signal.Controller is JunctionSignalController junctionController)
            {
                _junction = junctionController.GroupJunction;
            }
            else
            {
                _junction = signal.Controller.Group?.Junction;
            }
        }

        public override void UpdateDisplay()
        {
            if (_junction == null) return;

            DisplayText = _fullDef.IdDisplay switch
            {
                JunctionIdDisplayDefinition.IdDisplayMode.NumbersOnly => _junction.junctionData.junctionId.ToString(),
                _ => _junction.junctionData.junctionIdLong
            };
        }
    }
}
