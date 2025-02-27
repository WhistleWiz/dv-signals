using UnityModManagerNet;

namespace Signals.Game
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Custom Pack", Tooltip = "The mod ID of a custom signals pack")]
        public string CustomPack = string.Empty;
        [Draw("Use Verbose Logging", Tooltip = "Logs a lot more information\n" +
            "Useful if you are experiencing bugs")]
        public bool UseVerboseLogging = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange() { }
    }
}
