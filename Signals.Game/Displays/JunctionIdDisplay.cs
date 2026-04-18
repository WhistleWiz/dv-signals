using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    public class JunctionIdDisplay : InfoDisplay
    {
        private JunctionIdDisplayDefinition _fullDef;

        public JunctionIdDisplay(InfoDisplayDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (JunctionIdDisplayDefinition)definition;
        }

        public override void UpdateDisplay()
        {
            if (Controller.Group == null) return;

            DisplayText = _fullDef.IdDisplay switch
            {
                JunctionIdDisplayDefinition.IdDisplayMode.NumbersOnly => Controller.Group.Junction.junctionData.junctionId.ToString(),
                _ => Controller.Group.Junction.junctionData.junctionIdLong
            };
        }
    }
}
