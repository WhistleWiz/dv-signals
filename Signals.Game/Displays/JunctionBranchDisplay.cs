using Signals.Common.Displays;
using Signals.Game.Controllers;

namespace Signals.Game.Displays
{
    public class JunctionBranchDisplay : InfoDisplay
    {
        private JunctionBranchDisplayDefinition _fullDef;
        private JunctionSignalController? _junctionController;

        public JunctionBranchDisplay(InfoDisplayDefinition definition, BasicSignalController controller) : base(definition, controller)
        {
            _fullDef = (JunctionBranchDisplayDefinition)definition;
            _junctionController = (JunctionSignalController)controller;
        }

        public override void UpdateDisplay()
        {
            if (_junctionController == null) return;

            DisplayText = GetBranchDisplay(_junctionController.Junction, _fullDef.BranchDisplay);
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
                    return IntToLetters(junction.selectedBranch + 1);
                default:
                    return (junction.selectedBranch + 1).ToString();
            }
        }

        // https://codereview.stackexchange.com/a/44094
        public static string IntToLetters(int value)
        {
            string result = string.Empty;

            while (--value >= 0)
            {
                result = (char)('A' + value % 26) + result;
                value /= 26;
            }

            return result;
        }
    }
}
