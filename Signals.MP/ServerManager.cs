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
                    if (signal.Operation != SignalOperationMode.Automatic)
                    {
                        _server.SendPacketToPlayer(OperationModePacket.FromSignal(signal), player);
                    }

                    if (signal.ManualOverrideAspect != 0)
                    {
                        _server.SendPacketToPlayer(OverridePacket.FromSignal(signal), player);
                    }

                    if (signal.ShuntingAllowed)
                    {
                        _server.SendPacketToPlayer(ShuntingAllowedPacket.FromSignal(signal), player);
                    }
                }
            }
        }
    }
}
