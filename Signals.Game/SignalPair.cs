namespace Signals.Game
{
    internal class SignalPair
    {
        public SignalController To;
        public SignalController From;

        public SignalPair(SignalController to, SignalController from)
        {
            To = to;
            From = from;
        }

        public SignalController GetSignal(bool direction)
        {
            return direction ? To : From;
        }
    }
}
