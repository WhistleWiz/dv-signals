using MPAPI;
using MPAPI.Interfaces;
using Signals.Game;
using Signals.Game.Railway;
using UnityEngine;

namespace Signals.MP
{
    internal class ServerManager : MonoBehaviour
    {
        private IServer _server = null!;

        private void Awake()
        {
            _server = MultiplayerAPI.Server;

            _server.OnPlayerConnected += PlayerConnected;
            _server.OnPlayerReady += PlayerReady;

            _server.RegisterPacket<ReservationRequestPacket>(ReservationRequested);
            _server.RegisterPacket<ReservationCancelRequestPacket>(ReservationCancelled);
        }

        private void PlayerConnected(IPlayer player)
        {
            _server.SendPacketToPlayer(SettingsPacket.Get(), player);
        }

        private void PlayerReady(IPlayer player)
        {
            foreach (var id in TrackReserver.GetSignalIdsWithReservations())
            {
                _server.SendPacketToPlayer(new ReservationSuccessPacket() { SignalId = id }, player);
            }

            foreach (var controller in SignalManager.Instance.AllControllers)
            {
                foreach (var signal in controller.AllSignals)
                {
                    if (!signal.IsDefaultOperationState)
                    {
                        _server.SendPacketToPlayer(OperationModePacket.FromSignal(signal), player);
                    }

                    if (!signal.IsDefaultAspectOverride)
                    {
                        _server.SendPacketToPlayer(OverridePacket.FromSignal(signal), player);
                    }

                    if (!signal.IsDefaultShuntingAllowed)
                    {
                        _server.SendPacketToPlayer(ShuntingAllowedPacket.FromSignal(signal), player);
                    }
                }
            }
        }

        private void ReservationRequested(ReservationRequestPacket packet, IPlayer sender)
        {
            SignalsMod.Log($"[MP] Received reservation request for signal {packet.SignalId}");

            if (!SignalManager.Instance.TryGetSignal(packet.SignalId, out var signal))
            {
                PrintError("request reservation", packet.SignalId);
                return;
            }

            var result = packet.Duration > 0 ?
                TrackReserver.ReserveForSignal(signal, packet.Duration) :
                TrackReserver.ReserveForSignal(signal);

            if (result)
            {
                signal.AlignAllSwitches();
                _server.SendPacketToAll(new ReservationSuccessPacket() { SignalId = packet.SignalId });
                SignalsMod.Log($"[MP] Sent reservation success for signal {packet.SignalId}");
            }
            else
            {
                _server.SendPacketToAll(new ReservationFailurePacket() { SignalId = packet.SignalId });
                SignalsMod.Log($"[MP] Sent reservation failure for signal {packet.SignalId}");
            }
        }

        private void ReservationCancelled(ReservationCancelRequestPacket packet, IPlayer sender)
        {
            SignalsMod.Log($"[MP] Received reservation cancellation for signal {packet.SignalId}");

            if (!SignalManager.Instance.TryGetSignal(packet.SignalId, out var signal))
            {
                PrintError("cancel reservation", packet.SignalId);
                return;
            }

            TrackReserver.ClearFromSignal(signal);

            _server.SendPacketToAll(new ReservationCancelSuccessPacket() { SignalId = packet.SignalId });
        }

        private static void PrintError(string name, int signalId)
        {
            SignalsMod.Error($"[Networking] Received {name} for signal {signalId}, but it does not exist!\n" +
                $"Ensure both clients have the same signal pack active.");
        }
    }
}
