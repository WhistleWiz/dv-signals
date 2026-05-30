using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    public class JunctionIdDisplay : DisplayBase<JunctionIdDisplayDefinition>
    {
        private Junction? _junction;

        public JunctionIdDisplay(DisplayBaseDefinition definition, Signal signal) : base(definition, signal)
        {
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

            DisplayText = Definition.IdDisplay switch
            {
                JunctionIdDisplayDefinition.IdDisplayMode.NumbersOnly => _junction.junctionData.junctionId.ToString(),
                _ => _junction.junctionData.junctionIdLong
            };
        }
    }
}
