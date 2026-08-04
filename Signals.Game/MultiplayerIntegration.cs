using System.Reflection;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace Signals.Game
{
    internal static class MultiplayerIntegration
    {
        private static bool s_findDone = false;

        private static MethodInfo? s_init;
        private static MethodInfo? s_instance;

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

                s_init.Invoke(null, new object[] { modEntry });
            }
            catch (System.Exception e)
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
    }
}
