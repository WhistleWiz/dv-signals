using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;

namespace Signals.Game
{
    public static class SignalsMod
    {
        public const string Guid = "wiz.signals";

        public static UnityModManager.ModEntry Instance { get; private set; } = null!;
        public static Settings Settings { get; private set; } = null!;

        // Unity Mod Manager Wiki: https://wiki.nexusmods.com/index.php/Category:Unity_Mod_Manager
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            Instance = modEntry;
            Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            SignalManager.LoadSignals(modEntry);

            if (SignalManager.DefaultPack == null)
            {
                throw new System.Exception("Failed to load default pack, mod won't load!");
            }

            Instance.OnGUI += Settings.Draw;
            Instance.OnSaveGUI += Settings.Save;

            UnityModManager.toggleModsListen += HandleModToggled;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            WorldStreamingInit.LoadingStatusChanged += SignalManager.CheckStartCreation;

            return true;
        }

        private static void HandleModToggled(UnityModManager.ModEntry modEntry, bool newState)
        {
            if (newState)
            {
                SignalManager.LoadSignals(modEntry);
            }
            else
            {
                SignalManager.UnloadSignals(modEntry);
            }
        }

        public static void Log(string message)
        {
            Instance.Logger.Log(message);
        }

        public static void LogVerbose(string message)
        {
            if (Settings.UseVerboseLogging)
            {
                Instance.Logger.Log(message);
            }
        }

        public static void Warning(string message)
        {
            Instance.Logger.Warning(message);
        }

        public static void Error(string message)
        {
            Instance.Logger.Error(message);
        }
    }
}
