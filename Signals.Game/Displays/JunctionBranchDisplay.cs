using Signals.Common.Displays;
using Signals.Game.Controllers;
using Signals.Game.Util;

namespace Signals.Game.Displays
{
    public class JunctionBranchDisplay : DisplayBase<JunctionBranchDisplayDefinition>
    {
        private Junction? _junction;

        public JunctionBranchDisplay(DisplayBaseDefinition definition, Signal signal) : base(definition, signal)
        {
            // Use the junction matched for the junction controller to still provide relevant information.
            if (signal.Controller is JunctionSignalController junctionController)
            {
                _junction = junctionController.Junction;
            }
            else
            {
                _junction = signal.Controller.Group?.Junction;
            }
        }

        public override void UpdateDisplay()
        {
            if (_junction == null) return;

            DisplayText = GetBranchDisplay(_junction, Definition.BranchDisplay);

            if (Definition.BranchDisplay == JunctionBranchDisplayDefinition.BranchDisplayMode.Sprites)
            {
                SpriteOverride = Definition.DisplaySprites[_junction.selectedBranch];
            }
        }

        private static string GetBranchDisplay(Junction junction, JunctionBranchDisplayDefinition.BranchDisplayMode mode)
        {
            // Fallback to BranchNumber for certain modes.
            if (junction.outBranches.Count > 2)
            {
                switch (mode)
                {
                    case JunctionBranchDisplayDefinition.BranchDisplayMode.Symbols:
                    case JunctionBranchDisplayDefinition.BranchDisplayMode.Direction:
                    case JunctionBranchDisplayDefinition.BranchDisplayMode.DirectionLetter:
                        mode = JunctionBranchDisplayDefinition.BranchDisplayMode.BranchNumber;
                        break;
                    default:
                        break;
                }
            }

            switch (mode)
            {
                case JunctionBranchDisplayDefinition.BranchDisplayMode.BranchNumberRaw:
                    return junction.selectedBranch.ToString();
                case JunctionBranchDisplayDefinition.BranchDisplayMode.Symbols:
                    if (junction.selectedBranch == 0) return "\\";
                    if (junction.selectedBranch == 1) return "/";
                    goto default;
                case JunctionBranchDisplayDefinition.BranchDisplayMode.Direction:
                    if (junction.selectedBranch == 0) return "LEFT";
                    if (junction.selectedBranch == 1) return "RIGHT";
                    goto default;
                case JunctionBranchDisplayDefinition.BranchDisplayMode.DirectionLetter:
                    if (junction.selectedBranch == 0) return "L";
                    if (junction.selectedBranch == 1) return "R";
                    goto default;
                case JunctionBranchDisplayDefinition.BranchDisplayMode.Letters:
                    return Helpers.IntToLetters(junction.selectedBranch + 1);
                case JunctionBranchDisplayDefinition.BranchDisplayMode.Sprites:
                    return " ";
                default:
                    return (junction.selectedBranch + 1).ToString();
            }
        }
    }
}
