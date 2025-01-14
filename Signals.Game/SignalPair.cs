using Signals.Game.Controllers;

namespace Signals.Game
{
    internal class JunctionSignalPair
    {
        public JunctionSignalController To;
        public JunctionSignalController From;

        public JunctionSignalPair(JunctionSignalController to, JunctionSignalController from)
        {
            To = to;
            From = from;
        }

        public JunctionSignalController GetSignal(bool direction)
        {
            return direction ? To : From;
        }

        public JunctionSignalPair Flip()
        {
            return new JunctionSignalPair(From, To);
        }
    }
}
