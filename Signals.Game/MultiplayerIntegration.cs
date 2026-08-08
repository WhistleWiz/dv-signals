using System;
using System.Reflection;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace Signals.Game
{
    public static class MultiplayerIntegration
    {
        private static bool s_findDone = false;
        private static bool s_host = false;
        private static bool s_running = false;

        private static MethodInfo? s_init;
        private static MethodInfo? s_instance;
        private static MethodInfo? s_reserve;
        private static MethodInfo? s_cancelReserve;

        private static ModEntry? s_mp;
        private static ModEntry? MpMod
        {
            get
            {
                if (s_mp == null && !s_findDone)
                {
                    s_mp = UnityModManager.modEntries.Find(x => x.Info.Id == "Multiplayer");
                    s_findDone = true;
                }

                return s_mp;
            }
        }

        public static bool MpPresent => MpMod != null;
        public static bool IsMpActive => MpMod != null && MpMod.Active;
        public static bool IsMpRunning => IsMpActive && s_running;
        public static bool IsHost => s_host;

        public static Action<int, bool>? OnReservationRequestResultReceived;
        public static Action<int>? OnReservationClearResultReceived;

        public static void Initialise(ModEntry modEntry)
        {
            if (!MpPresent) return;

            try
            {
                var path = System.IO.Path.Combine(modEntry.Path, "Signals.MP.dll");
                var assembly = Assembly.LoadFile(path);
                var type = assembly.GetType("Signals.MP.MultiplayerIntegration");
                s_init = type.GetMethod("Initialise");
                s_instance = type.GetMethod("StartInstance");
                s_reserve = type.GetMethod("SendReservationRequest");
                s_cancelReserve = type.GetMethod("SendReservationCancelRequest");

                s_init.Invoke(null, new object[] { modEntry });
            }
            catch (Exception e)
            {
                SignalsMod.Error($"Failure loading Multiplayer integration: {e.Message}");
                s_mp = null;
                return;
            }

            SignalsMod.Log("Multiplayer integration loaded successfully.");
        }

        public static void StartInstance(SignalManager manager)
        {
            if (!IsMpActive || s_instance == null) return;

            s_instance.Invoke(null, new object[] { manager });
        }

        public static void SendReservationRequest(Signal signal, float duration)
        {
            if (!IsMpActive || s_reserve == null) return;

            s_reserve.Invoke(null, new object[] { signal.Id, duration });
        }

        public static void SendReservationCancelRequest(Signal signal)
        {
            if (!IsMpActive || s_cancelReserve == null) return;

            s_cancelReserve.Invoke(null, new object[] { signal.Id });
        }

        public static void SetHostStatus(bool isHost)
        {
            s_host = isHost;
        }

        public static void SetRunningStatus(bool isRunning)
        {
            s_running = isRunning;
        }
    }
}
