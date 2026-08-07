using MPAPI;
using MPAPI.Interfaces.Packets;
using MPAPI.Types;
using Signals.Game;
using System.Collections.Generic;
using static UnityModManagerNet.UnityModManager;

namespace Signals.MP
{
    public static class MultiplayerIntegration
    {
        private static Dictionary<string, MultiplayerCompatibility> s_originalCompat = new Dictionary<string, MultiplayerCompatibility>();

        public static void Initialise(ModEntry modEntry)
        {
            if (!MultiplayerAPI.IsMultiplayerLoaded) return;

            //Settings.OnSettingsSaved += UpdateModCompatibility;
        }

        private static void UpdateModCompatibility(Settings settings)
        {
            foreach (var item in s_originalCompat)
            {
                MultiplayerAPI.Instance.SetModCompatibility(item.Key, item.Value);
            }

            if (string.IsNullOrEmpty(settings.CustomPack)) return;

            if (!s_originalCompat.ContainsKey(settings.CustomPack))
            {
                //s_originalCompat.Add(settings.CustomPack, MultiplayerAPI.Instance.GetModCompatibility(settings.CustomPack));
                s_originalCompat.Add(settings.CustomPack, MultiplayerCompatibility.Client);
            }

            MultiplayerAPI.Instance.SetModCompatibility(settings.CustomPack, MultiplayerCompatibility.All);
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

        public static void SendChangeRequiredBranch(int id, int branch)
        {
            SendTwoWayPacket(new RequiredBranchPacket() { ControllerId = id, Branch = branch });
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

        public static void SendReservationRequest(int id, float duration)
        {
            SignalsMod.Log($"[MP] Sending reservation request to server for signal {id}");
            MultiplayerAPI.Client.SendPacketToServer(new ReservationRequestPacket() { SignalId = id, Duration = duration} );
        }

        public static void SendReservationCancelRequest(int id)
        {
            SignalsMod.Log($"[MP] Sending reservation cancellation request to server for signal {id}");
            MultiplayerAPI.Client.SendPacketToServer(new ReservationCancelRequestPacket() { SignalId = id });
        }
    }
}
