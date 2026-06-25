using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace Signals.Game
{
    public enum DebugMode
    {
        None,
        HoveredSignal,
        All
    }

    public enum OutsideStationPlacement
    {
        None,
        BranchOnly,
        Full
    }

    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        private static readonly string[] NoCustomPack = new[] { "None" };

        public string CustomPack = string.Empty;
        [Draw("Enable Special Matching Path", Tooltip = "Enables the optional matching path aspects in signals")]
        public bool SpecialPath = true;
        [Draw("Exit Signals on Storage Tracks", Tooltip = "Places exit signals on storage tracks rather than shunting signals")]
        public bool ExitSignalsOnStorageTracks = true;
        [Draw("Outside Station Placement", Tooltip = "Enables signals outside stations")]
        public OutsideStationPlacement OutsideStationPlacement = OutsideStationPlacement.Full;
        //[Draw("Flip Speed Sign Side", Tooltip = "Speed signs are placed on the left side of the track rather than the right")]
        //public bool FlipSpeedSigns = false;
        [Draw("Use Verbose Logging", Tooltip = "Logs a lot more information\n" +
            "Useful if you are experiencing bugs")]
        public bool UseVerboseLogging = false;
        [Draw("Show Debug Blocks", Tooltip = "Shows where each signal's tracks start and end")]
        public DebugMode DebugBlocks = DebugMode.None;

        private int _index = 0;
        private string[] _keys = NoCustomPack;
        private GUIContent _packText = new GUIContent("Custom Pack", "Use a custom signal pack from another mod");
        private GUILayoutOption _widthFull = GUILayout.MaxWidth(350);
        private GUILayoutOption? _widthLabel;

        public bool PlaceSignalsInBranches => OutsideStationPlacement != OutsideStationPlacement.None;
        public bool PlaceSignalsOutsideStations => OutsideStationPlacement == OutsideStationPlacement.Full;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            RebuildKeys();
            Save(this, modEntry);
        }

        public void OnChange() { }

        public void DrawGUI(UnityModManager.ModEntry modEntry)
        {
            if (_widthLabel == null)
            {
                _widthLabel = GUILayout.Width(GUI.skin.label.CalcSize(_packText).x + 10);
            }

            GUILayout.BeginHorizontal(_widthFull);
            GUILayout.Label(_packText, _widthLabel);

            if (UnityModManager.UI.PopupToggleGroup(ref _index, _keys, "Select Custom Pack"))
            {
                if (_index > 0)
                {
                    CustomPack = _keys[_index];
                }
                else
                {
                    CustomPack = string.Empty;
                }
            }

            GUILayout.EndHorizontal();

            this.Draw(modEntry);
        }

        private void RebuildKeys()
        {
            var entries = NoCustomPack.ToList();
            entries.AddRange(SignalManager.InstalledPacks.Keys);
            _keys = entries.ToArray();

            if (!string.IsNullOrEmpty(CustomPack))
            {
                _index = System.Array.IndexOf(_keys, CustomPack);

                if (_index < 0)
                {
                    SignalsMod.Warning($"Missing signal pack '{CustomPack}', defaulting to none.");
                    _index = 0;
                }
            }
        }
    }
}
