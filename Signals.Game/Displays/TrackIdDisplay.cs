using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    internal class TrackIdDisplay : InfoDisplay
    {
        private TrackIdDisplayDefinition _fullDef;

        public TrackIdDisplay(InfoDisplayDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (TrackIdDisplayDefinition)definition;
        }

        public override void UpdateDisplay()
        {
            switch (Controller)
            {
                case JunctionSignalController junction:
                    if (junction.LastWalkInfo != null)
                    {
                        switch (_fullDef.TrackIDMode)
                        {
                            case TrackIdDisplayDefinition.TrackIdDisplayMode.NumberOnly:
                                DisplayText = junction.LastWalkInfo.NextYardTrackNumber;
                                break;
                            case TrackIdDisplayDefinition.TrackIdDisplayMode.NumberAndType:
                                DisplayText = junction.LastWalkInfo.NextYardTrackSign;
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
