using Signals.Game.Controllers;
using System.Collections.Generic;

namespace Signals.Game
{
    internal class JunctionSignalPair
    {
        public JunctionSignalController? OutBranchesSignal;
        public JunctionSignalController? InBranchSignal;

        public IEnumerable<JunctionSignalController> AllSignals
        {
            get
            {
                if (OutBranchesSignal != null) yield return OutBranchesSignal;
                if (InBranchSignal != null) yield return InBranchSignal;
            }
        }

        public JunctionSignalPair(JunctionSignalController? outSignal, JunctionSignalController? inSignal)
        {
            OutBranchesSignal = outSignal;
            InBranchSignal = inSignal;
        }

        public JunctionSignalController? GetSignal(TrackDirection direction)
        {
            return direction.IsOut() ? OutBranchesSignal : InBranchSignal;
        }

        public JunctionSignalPair Flip()
        {
            return new JunctionSignalPair(InBranchSignal, OutBranchesSignal);
        }
    }
}
