using MPAPI;
using MPAPI.Interfaces.Packets;
using Signals.Game;

using static UnityModManagerNet.UnityModManager;

namespace Signals.MP
{
    public static class MultiplayerIntegration
    {
        public static void Initialise(ModEntry modEntry)
        {
            if (!MultiplayerAPI.IsMultiplayerLoaded) return;

            MultiplayerAPI.Instance.SetModCompatibility(modEntry.Info.Id, MPAPI.Types.MultiplayerCompatibility.All);
            NetworkEventManager.Init();
        }

        public static void StartInstance(SignalManager manager)
        {
            if (MultiplayerAPI.Instance.IsHost)
            {
                var server = manager.gameObject.AddComponent<ServerManager>();
            }

            var client = manager.gameObject.AddComponent<ClientManager>();
        }

        public static void SendChangeOperationMode(int id, SignalOperationMode mode)
        {
            SendTwoWayPacket(new OperationModePacket() { SignalId = id, Mode = mode });
        }

        public static void SendChangeOverride(int id, int aspect)
        {
            SendTwoWayPacket(new OverridePacket() { SignalId = id, Aspect = aspect });
        }

        public static void SendChangeShunting(int id, bool allowed)
        {
            SendTwoWayPacket(new ShuntingAllowedPacket() { SignalId = id, Allowed = allowed });
        }

        private static void SendTwoWayPacket<T>(T packet)
            where T : class, IPacket, new()
        {
            if (MultiplayerAPI.Instance.IsHost)
            {
                MultiplayerAPI.Server.SendPacketToAll(packet);
            }
            else
            {
                MultiplayerAPI.Client.SendPacketToServer(packet);
            }
        }
    }
}
