using Signals.Game;
using Signals.Game.Railway;

namespace Signals.MP
{
    public static class NetworkEventManager
    {
        internal static void Init()
        {
            TrackReserver.ReservationMade += ReservationMade;
            TrackReserver.ReservationCleared += ReservationCleared;
        }

        private static void ReservationMade(Signal signal)
        {
            // Implement the server to client sending here.
            // Networking.MakeSignalReservation(signal.Id);
        }

        private static void ReservationCleared(Signal signal)
        {
            // Implement the server to client sending here.
            // Networking.ClearSignalReservation(signal.Id);
        }

        private static void ReceiveReservationRequest(int signalId)
        {
            if (!SignalManager.Instance.TryGetSignal(signalId, out var signal))
            {
                PrintError("request reservation", signalId);
                return;
            }

            TrackReserver.ReserveForSignal(signal);
        }

        private static void ReceiveReservationClear(int signalId)
        {
            if (!SignalManager.Instance.TryGetSignal(signalId, out var signal))
            {
                PrintError("clear reservation", signalId);
                return;
            }

            TrackReserver.ClearFromSignal(signal);
        }

        private static void PrintError(string name, int signalId)
        {
            SignalsMod.Error($"[Networking] Received {name} for signal {signalId}, but it does not exist!\n" +
                $"Ensure both clients have the same signal pack active.");
        }
    }
}
