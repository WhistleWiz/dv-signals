using UnityModManagerNet;

namespace Signals.Game
{
    public enum DebugMode
    {
        None,
        HoveredSignal,
        All
    }

    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Custom Pack", Tooltip = "The mod ID of a custom signals pack")]
        public string CustomPack = string.Empty;
        [Draw("Better Signals On Passenger Tracks")]
        public bool PaxSignals = true;
        [Draw("Use Verbose Logging", Tooltip = "Logs a lot more information\n" +
            "Useful if you are experiencing bugs")]
        public bool UseVerboseLogging = false;
        [Draw("Show Debug Blocks", Tooltip = "Shows where each signal's tracks start and end")]
        public DebugMode DebugBlocks = DebugMode.None;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange() { }
    }
}
