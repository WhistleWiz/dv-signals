using UnityEngine;

namespace Signals.Common.Displays
{
    public class JunctionBranchDisplayDefinition : InfoDisplayDefinition
    {
        public enum JunctionDisplayMode
        {
            [Tooltip("0, 1, 2, 3, 4...")]
            BranchNumberRaw,
            [Tooltip("1, 2, 3, 4, 5...")]
            BranchNumber,
            [Tooltip("\\, /")]
            Symbols,
            [Tooltip("LEFT, RIGHT")]
            Direction,
            [Tooltip("L, R")]
            DirectionLetter,
            [Tooltip("A, B, C, D, E...")]
            Letters
        }

        [Tooltip("How to display the selected branch\n" +
            "The display modes 'Symbols', 'Direction', and 'Direction Letter' only support " +
            "junctions with 2 branches, and will fall back to 'Branch Number' if the number " +
            "of branches is larger")]
        public JunctionDisplayMode BranchDisplay = JunctionDisplayMode.Symbols;
        [Tooltip("If true, only displays if the signal is facing the branches")]
        public bool TowardsOnly = true;
    }
}
