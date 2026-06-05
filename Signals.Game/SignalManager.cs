using DV.Utils;
using Signals.Common;
using Signals.Game.Controllers;
using Signals.Game.Generation;
using Signals.Game.Railway;
using Signals.Game.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace Signals.Game
{
    public class SignalManager : SingletonBehaviour<SignalManager>
    {
        private const float UpdateTime = 1.0f;
        private const int MergeLoops = 3;
        private const int LaserPointerTargetLayer = 15;

        private static Transform? s_holder;
        private static bool s_loaded = false;
        private static bool s_jobs = false;

        internal static SignalPack DefaultPack = null!;
        internal static Dictionary<string, SignalPack> InstalledPacks = new Dictionary<string, SignalPack>();

        internal static Transform Holder
        {
            get
            {
                if (s_holder == null)
                {
                    var go = new GameObject("[Signal Holder]");
                    go.SetActive(false);
                    DontDestroyOnLoad(go);
                    s_holder = go.transform;
                }

                return s_holder;
            }
        }

        public static bool Running => s_loaded;

        private Dictionary<Junction, JunctionSignalGroup> _junctionSignals =
            new Dictionary<Junction, JunctionSignalGroup>();
        private List<DistantSignalController> _distantSignals =
            new List<DistantSignalController>();
        private List<TurntableSignalController> _turntableSignals =
            new List<TurntableSignalController>();
        private List<BasicSignalController> _controllerRegistry =
            new List<BasicSignalController>();
        private Dictionary<int, Signal> _signalRegistry =
            new Dictionary<int, Signal>();

        private Coroutine? _updateCoro;

        public static SignalPlacer? Placer = new RealisticSignalPlacer();

        public List<BasicSignalController> AllControllers => _controllerRegistry;

        public new static string AllowAutoCreate()
        {
            return "[SignalManager]";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _junctionSignals.Clear();
            _distantSignals.Clear();

            StopCoroutine(_updateCoro);
            JobHelper.ManagerInstanced = false;
            Camera.onPostRender -= DebugRender;
            TrackReserver.ClearAll();
        }

        #region Mod Loading

        internal static SignalPack GetCurrentPack()
        {
            if (TryGetPack(SignalsMod.Settings.CustomPack, out var pack))
            {
                return pack;
            }

            if (!string.IsNullOrEmpty(SignalsMod.Settings.CustomPack))
            {
                SignalsMod.Error($"Could not find pack '{SignalsMod.Settings.CustomPack}', using default signals.");
            }

            return DefaultPack;
        }

        internal static void LoadSignals(UnityModManager.ModEntry mod)
        {
            AssetBundle bundle;
            var files = Directory.EnumerateFiles(mod.Path, Constants.Bundle, SearchOption.AllDirectories);

            foreach (var file in files)
            {
                bundle = AssetBundle.LoadFromFile(file);

                // Somehow failed to load the bundle.
                if (bundle == null)
                {
                    SignalsMod.Error("Failed to load bundle!");
                    continue;
                }

                var pack = bundle.LoadAllAssets<SignalPack>().FirstOrDefault();

                if (pack != null)
                {
                    if (DefaultPack == null)
                    {
                        DefaultPack = pack;
                        SignalsMod.Log("Loaded default pack.");
                    }
                    else
                    {
                        InstalledPacks.Add(mod.Info.Id, pack);
                    }

                    ProcessControllers(pack);
                    bundle.Unload(false);
                    break;
                }

                bundle.Unload(false);
            }
        }

        internal static void UnloadSignals(UnityModManager.ModEntry mod)
        {
            if (InstalledPacks.ContainsKey(mod.Info.Id))
            {
                InstalledPacks.Remove(mod.Info.Id);
            }
        }

        private static void ProcessControllers(SignalPack pack)
        {
            foreach (var controller in pack.AllControllers)
            {
                foreach (var signal in controller.Signals)
                {
                    ProcessSignal(signal);
                }

                foreach (var signal in controller.ShuntingSignals)
                {
                    ProcessSignal(signal);
                }
            }

            static void ProcessSignal(SignalDefinition signal)
            {
                if (signal.TryGetComponent<SignalHover>(out var hover)) return;

                hover = signal.gameObject.AddComponent<SignalHover>();
                hover.gameObject.layer = LaserPointerTargetLayer;
            }
        }

        #endregion

        #region Signal Creation

        internal static void CheckStartCreation(string msg, bool isError, float percent)
        {
            if (!SignalsMod.Instance.Active) return;

            // Reset for reloads.
            if (percent < 60)
            {
                s_loaded = false;
                return;
            }

            if (!s_loaded)
            {
                DisplayLoadingThingy();
                Instance.CreateSignals();
                s_loaded = true;
            }

            if (percent < 90)
            {
                s_jobs = false;
                return;
            }

            if (!s_jobs)
            {
                JobHelper.ManagerInstanced = true;
                s_jobs = true;
            }
        }

        private static void DisplayLoadingThingy()
        {
            // Thanks Skin Manager.
            var info = FindObjectOfType<DisplayLoadingInfo>();

            if (info != null)
            {
                info.loadProgressTMP.richText = true;
                info.loadProgressTMP.text += "\ncreating <color=#FF3333>si</color><color=#FFCC00>gna</color><color=#33FF77>ls</color>";
            }
        }

        private void CreateSignals()
        {
            SignalsMod.Log("Started creating signals...");
            //OldAreaCalculator.DebugCreateDummies();

            SpeedCalculator.ClearCache();
            BasicSignalController.ResetIdGeneration();
            Signal.ResetIdGeneration();

            Placer ??= new RealisticSignalPlacer();

            // Initial placement.
            var sw = System.Diagnostics.Stopwatch.StartNew();
            int count = 0;
            var pack = GetCurrentPack();

            Placer.CreateSignals(pack, _junctionSignals);

            sw.Stop();
            SignalsMod.Log($"Finished creating signals for {_junctionSignals.Count} junction(s), " +
                $"current total is {_controllerRegistry.Count} ({sw.Elapsed.TotalSeconds:F4}s)");

            // Merging loops.
            for (int i = 0; i < MergeLoops; i++)
            {
                sw.Restart();
                count = Placer.MergeSignals(pack, _junctionSignals);
                sw.Stop();
                SignalsMod.Log($"Merged {count} signal(s) (loop {i + 1}/{MergeLoops}), " +
                    $"current total is {_controllerRegistry.Count} ({sw.Elapsed.TotalSeconds:F4}s)");

                if (count == 0)
                {
                    SignalsMod.Log($"Interrupted merging loop (no more merges could be made)");
                    break;
                }
            }

            // Distant signals.
            sw.Restart();
            Placer.CreateDistantSignals(pack, _junctionSignals, _distantSignals);
            sw.Stop();
            SignalsMod.Log($"Finished creating {_distantSignals.Count} distant signal(s), " +
                $"current total is {_controllerRegistry.Count} ({sw.Elapsed.TotalSeconds:F4}s)");

            count = _distantSignals.Count;

            sw.Restart();
            Placer.CreateRepeaterSignals(pack, _junctionSignals, _distantSignals);
            sw.Stop();
            SignalsMod.Log($"Finished creating {_distantSignals.Count - count} repeater signal(s), " +
                $"current total is {_controllerRegistry.Count} ({sw.Elapsed.TotalSeconds:F4}s)");

            sw.Restart();
            Placer.CreateTurntableSignals(pack, _turntableSignals);
            sw.Stop();
            SignalsMod.Log($"Finished creating {_turntableSignals.Count} turntable signal(s), " +
                $"current total is {_controllerRegistry.Count} ({sw.Elapsed.TotalSeconds:F4}s)");

            // Track intersections.
            TrackChecker.StartBuildingMap();

            _updateCoro = StartCoroutine(UpdateRoutine());

            Camera.onPostRender += DebugRender;
        }

        #endregion

        // Update all signals from here.
        private System.Collections.IEnumerator UpdateRoutine()
        {
            while (PlayerManager.ActiveCamera == null) yield return null;

            yield return WaitFor.Seconds(UpdateTime);

            while (true)
            {
                // Check how many signals to update per frame so they all update roughly once a second.
                int count = Mathf.CeilToInt(_controllerRegistry.Count * Time.fixedDeltaTime);

                // Loop through all registered signals.
                for (int start = 0; start < _controllerRegistry.Count; start += count)
                {
                    // Loop through a batch of signals. Updates are distributed so they don't all update
                    // at once, but batched so timing is consistent.
                    for (int current = start; current < start + count && current < _controllerRegistry.Count; current++)
                    {
                        // Stop updating if camera is gone.
                        while (PlayerManager.ActiveCamera == null) yield return WaitFor.Seconds(UpdateTime);

                        var controller = _controllerRegistry[current];

                        // Check if it can be updated.
                        if (!controller.SafetyCheck())
                        {
                            current--;
                            continue;
                        }

                        // Call this for any pre-update functions.
                        controller.PreUpdate?.Invoke(controller);

                        // Skip updating if not needed.
                        var update = controller.ShouldUpdate();
                        if (!update && !controller.HasUpdatesQueued) continue;

                        // Actually update.
                        controller.Update(false, update);
                    }

                    yield return WaitFor.FixedUpdate;
                }
            }
        }

        /// <summary>
        /// Register a <see cref="BasicSignalController"/> to be automatically updated.
        /// </summary>
        public void RegisterController(BasicSignalController controller)
        {
            _controllerRegistry.Add(controller);
        }

        /// <summary>
        /// Unregister a <see cref="BasicSignalController"/> from being automatically updated.
        /// </summary>
        /// <returns><see langword="true"/> if the signal was successfully removed, and <see langword="false"/> otherwise.</returns>
        public bool UnregisterController(BasicSignalController controller)
        {

            return _controllerRegistry.Remove(controller);
        }

        /// <summary>
        /// Register a <see cref="Signal"/> to be accessible by various methods.
        /// </summary>
        public void RegisterSignal(Signal signal)
        {
            _signalRegistry.Add(signal.Id, signal);
        }

        /// <summary>
        /// Unregister a <see cref="Signal"/> from being accessible by various methods.
        /// </summary>
        public void UnregisterSignal(Signal signal)
        {
            _signalRegistry.Remove(signal.Id);
        }

        #region Utility

        private void DebugRender(Camera cam)
        {
            var debugMode = SignalsMod.Settings.DebugBlocks;
            if (debugMode == DebugMode.None || PlayerManager.ActiveCamera == null) return;

            var debugHovered = debugMode == DebugMode.HoveredSignal;
            var up2 = Vector3.up * 2;

            foreach (var controller in AllControllers)
            {
                // Skip drawing signals that are too far away.
                if (controller.GetCameraDistanceSqr() > 16000000) continue;

                foreach (var signal in controller.AllSignals)
                {
                    // Only draw the hovered sign if the debug mod is set to hovered.
                    if (debugHovered && !signal.Hovered) continue;

                    var block = signal.Block;
                    if (block == null) continue;

                    var offset = WorldMover.currentMove + Vector3.up;
                    var offset2 = signal.Definition.transform.right * -0.1f;
                    var hue = signal.Id * 0.31f;
                    var highlight = !debugHovered && signal.Hovered;

                    if (highlight)
                    {
                        offset2 += new Vector3(0, 0.1f, 0);
                    }

                    if (block.NextController != null)
                    {
                        GLHelper.DrawLozenge(block.NextController.Position + up2, block.NextController.Definition.transform.forward, GetColour(hue, highlight));
                    }

                    if ((debugHovered || highlight) && controller is DistantSignalController distant)
                    {
                        GLHelper.DrawLine(controller.Position + offset2, distant.Home.Position + offset2, GetColour(hue, highlight));
                    }

                    foreach (var track in block.ExtraTracks)
                    {
                        GLHelper.DrawPointSet(track.GetKinkedPointSet().points, offset + offset2, 20, GetColour(hue, highlight));
                        hue += 0.03f;
                    }

                    hue += 0.1f;

                    foreach (var track in block.Tracks)
                    {
                        GLHelper.DrawPointSet(track.Track.GetKinkedPointSet().points, offset + offset2, 20, GetColour(hue, highlight));
                        hue += 0.03f;
                    }
                }
            }

            static Color GetColour(float hue, bool highlight)
            {
                return Color.HSVToRGB(hue % 1.00f, highlight ? 0.25f : 0.95f, 1.00f);
            }
        }

        /// <summary>
        /// Tries to find a pack from the mod ID.
        /// </summary>
        /// <param name="id">The mod ID that added the pack.</param>
        /// <param name="pack">The returned pack if it exists.</param>
        /// <returns></returns>
        public static bool TryGetPack(string id, out SignalPack pack)
        {
            return InstalledPacks.TryGetValue(id, out pack);
        }

        /// <summary>
        /// Tries to find a <see cref="JunctionSignalGroup"/> that belongs to the specified <see cref="Junction"/>.
        /// </summary>
        /// <param name="junction">The <see cref="Junction"/> with a group.</param>
        /// <param name="group">The returned group, if found. Otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a group was found, otherwise <see langword="false"/>.</returns>
        public bool TryGetJunctionGroup(Junction junction, out JunctionSignalGroup group)
        {
            return _junctionSignals.TryGetValue(junction, out group);
        }

        /// <summary>
        /// Tries to find a <see cref="Junction"/> with the specified ID, then a <see cref="JunctionSignalGroup"/> that belongs to it.
        /// </summary>
        /// <param name="junctionId">The ID of a junction.</param>
        /// <param name="group">The returned group, if found. Otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a group was found, otherwise <see langword="false"/>.</returns>
        public bool TryGetJunctionGroup(string junctionId, out JunctionSignalGroup group)
        {
            var junction = System.Array.Find(RailTrackRegistry.Junctions, x => x.junctionData.junctionIdLong == junctionId);

            if (junction != null)
            {
                return TryGetJunctionGroup(junction, out group);
            }

            group = null!;
            return false;
        }

        /// <summary>
        /// Tries to find a <see cref="Junction"/> with the specified ID, then a <see cref="JunctionSignalGroup"/> that belongs to it.
        /// </summary>
        /// <param name="junctionId">The numerical ID of a junction.</param>
        /// <param name="group">The returned group, if found. Otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a group was found, otherwise <see langword="false"/>.</returns>
        public bool TryGetJunctionGroup(int junctionIdInt, out JunctionSignalGroup group)
        {
            var junction = System.Array.Find(RailTrackRegistry.Junctions, x => x.junctionData.junctionId == junctionIdInt);

            if (junction != null)
            {
                return TryGetJunctionGroup(junction, out group);
            }

            group = null!;
            return false;
        }

        /// <summary>
        /// Tries to find a <see cref="BasicSignalController"/> with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the controller.</param>
        /// <param name="signal">The signal, if found. Otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a controller was found, otherwise <see langword="false"/>.</returns>
        public bool TryGetController(int id, out BasicSignalController signal)
        {
            signal = AllControllers.First(x => x.Id == id);
            return signal != null;
        }

        /// <summary>
        /// Tries to find a <see cref="Signal"/> with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the signal.</param>
        /// <param name="signal">The signal, if found. Otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a signal was found, otherwise <see langword="false"/>.</returns>
        public bool TryGetSignal(int id, out Signal signal)
        {
            return _signalRegistry.TryGetValue(id, out signal);
        }

        #endregion
    }
}
