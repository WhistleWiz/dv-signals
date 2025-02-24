using Signals.Game.Controllers;

namespace Signals.Game
{
    internal class JunctionSignalPair
    {
        public JunctionSignalController? OutBranchesSignal;
        public JunctionSignalController? InBranchesSignal;

        public JunctionSignalPair(JunctionSignalController? outSignal, JunctionSignalController? inSignal)
        {
            OutBranchesSignal = outSignal;
            InBranchesSignal = inSignal;
        }

        public JunctionSignalController? GetSignal(bool direction)
        {
            return direction ? OutBranchesSignal : InBranchesSignal;
        }

        public JunctionSignalPair Flip()
        {
            return new JunctionSignalPair(InBranchesSignal, OutBranchesSignal);
        }
    }
}
