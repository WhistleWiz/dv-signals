using MPAPI;
using MPAPI.Types;
using Signals.Game;

using static UnityModManagerNet.UnityModManager;

namespace Signals.MP
{
    public static class MultiplayerIntegration
    {
        public static void Initialise(ModEntry modEntry)
        {
            if (!MultiplayerAPI.IsMultiplayerLoaded) return;

            MultiplayerAPI.ClientStarted += ClientManager.Initialise;
        }

        public static void StartInstance(SignalManager manager)
        {
            if (MultiplayerAPI.Instance.IsHost)
            {
                var pack = SignalsMod.Settings.CustomPack;

                if (!string.IsNullOrEmpty(pack))
                {
                    MultiplayerAPI.Instance.SetModCompatibility(pack, MultiplayerCompatibility.All);
                }

                manager.gameObject.AddComponent<ServerManager>();
            }

            manager.gameObject.AddComponent<ClientManager>();

            Game.MultiplayerIntegration.SetHostStatus(MultiplayerAPI.Instance.IsHost);
        }

        public static void SendReservationRequest(int id, float duration)
        {
            SignalsMod.LogMP($"Sending reservation request to server for signal {id}");
            MultiplayerAPI.Client.SendPacketToServer(new ReservationRequestPacket() { SignalId = id, Duration = duration} );
        }

        public static void SendReservationCancelRequest(int id)
        {
            SignalsMod.LogMP($"Sending reservation cancellation request to server for signal {id}");
            MultiplayerAPI.Client.SendPacketToServer(new ReservationCancelRequestPacket() { SignalId = id });
        }
    }
}
