using UnityEngine;

namespace Signals.Common.Displays
{
    public class JunctionBranchDisplayDefinition : InfoDisplayDefinition
    {
        [Tooltip("If true, numbers start at 1 instead of 0")]
        public bool OffsetByOne = true;
        [Tooltip("If true, only displays if the signal is facing the branches")]
        public bool TowardsOnly = true;
    }
}
