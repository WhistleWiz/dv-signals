using Signals.Game.Controllers;
using System.Collections.Generic;

namespace Signals.Game
{
    internal class JunctionSignalGroup
    {
        public JunctionSignalController? OutBranchesSignal;
        public JunctionSignalController? InBranchSignal;

        public Junction Junction { get; private set; }

        public IEnumerable<JunctionSignalController> AllSignals
        {
            get
            {
                if (OutBranchesSignal != null) yield return OutBranchesSignal;
                if (InBranchSignal != null) yield return InBranchSignal;
            }
        }

        public IEnumerable<JunctionSignalController> AllInSignals
        {
            get
            {
                if (InBranchSignal != null) yield return InBranchSignal;
            }
        }

        public JunctionSignalGroup(Junction junction, JunctionSignalController? outSignal, JunctionSignalController? inSignal)
        {
            Junction = junction;
            OutBranchesSignal = outSignal;
            InBranchSignal = inSignal;
        }

        public JunctionSignalController? GetSignal(TrackDirection direction)
        {
            return direction.IsOut() ? OutBranchesSignal : InBranchSignal;
        }
    }
}
