using DV.PointSet;
using DV.Utils;
using Signals.Common;
using Signals.Game.Controllers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace Signals.Game
{
    public class SignalManager : SingletonBehaviour<SignalManager>
    {
        private enum SignalCreationMode
        {
            None,
            Mainline,
            IntoYard,
            IntoYardReverse,
            IntoPax,
            Shunting
        }

        private const string YardNameStart = "[Y]";
        private const string PaxNameEnd = "LP]";
        private const string NoSign = "[#]";
        private const float JunctionPlacementDistance = 2.0f;
        private const float BranchPlacementDistance = 17.5f;
        private const float BranchDistanceThreshold = 4.5f * 4.5f;
        private const float DeadEndThreshold = 100.0f;
        private const float LongTrackThreshold = 300.0f;
        private const float ClosenessThreshold = 75.0f;
        private const float SmallTrackThreshold = 50.0f;
        private const float VerySmallTrackThreshold = 15.0f;
        private const float UpdateTime = 1.0f;
        private const int LaserPointerTargetLayer = 15;
        private const int MergeLoops = 3;

        private static Transform? s_holder;
        private static bool s_loaded = false;

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
        private List<ShuntingSignalController> _shuntingSignals =
            new List<ShuntingSignalController>();
        private List<DistantSignalController> _distantSignals =
            new List<DistantSignalController>();
        private List<BasicSignalController> _signalRegister =
            new List<BasicSignalController>();

        private Coroutine? _updateCoro;

        public List<BasicSignalController> AllSignals => _signalRegister;

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
            Camera.onPostRender -= DebugRender;
        }

        private void DebugRender(Camera cam)
        {
            var debugMode = SignalsMod.Settings.DebugBlocks;
            if (debugMode == DebugMode.None || PlayerManager.ActiveCamera == null) return;

            var debugHovered = debugMode == DebugMode.HoveredSignal;
            var up2 = Vector3.up * 2;

            foreach (var signal in AllSignals)
            {
                // Only draw the hovered sign if the debug mod is set to hovered.
                if (debugHovered && !signal.Hovered) continue;
                // Skip drawing signs that are too far away.
                if (signal.GetCameraDistanceSqr() > 16000000) continue;

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

                if (block.NextSignal != null)
                {
                    GLHelper.DrawLozenge(block.NextSignal.Position + up2, block.NextSignal.Definition.transform.forward, GetColour(hue, highlight));
                }

                if ((debugHovered || highlight) && signal is DistantSignalController distant)
                {
                    GLHelper.DrawLine(signal.Position + offset2, distant.Home.Position + offset2, GetColour(hue, highlight));
                }

                foreach (var track in block.ExtraTracks)
                {
                    GLHelper.DrawPointSet(track.GetKinkedPointSet().points, offset + offset2, 20, GetColour(hue, highlight));
                    hue += 0.03f;
                }

                hue += 0.1f;

                foreach (var track in block.Tracks)
                {
                    GLHelper.DrawPointSet(track.GetKinkedPointSet().points, offset + offset2, 20, GetColour(hue, highlight));
                    hue += 0.03f;
                }
            }

            static Color GetColour(float hue, bool highlight)
            {
                return Color.HSVToRGB(hue % 1.00f, highlight ? 0.15f : 0.95f, 1.00f);
            }
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
                    ProcessSignals(pack);

                    if (DefaultPack == null)
                    {
                        DefaultPack = pack;
                        SignalsMod.Log("Loaded default pack.");
                    }
                    else
                    {
                        InstalledPacks.Add(mod.Info.Id, pack);
                    }

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

        private static void ProcessSignals(SignalPack pack)
        {
            foreach (var item in pack.AllSignals)
            {
                item.gameObject.layer = LaserPointerTargetLayer;
                item.gameObject.AddComponent<SignalHover>();
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

            if (s_loaded) return;

            DisplayLoadingThingy();
            Instance.CreateSignals();
            s_loaded = true;
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

            var sw = System.Diagnostics.Stopwatch.StartNew();
            int count = 0;
            var pack = GetCurrentPack();

            foreach (var junction in RailTrackRegistryBase.Junctions)
            {
                switch (ShouldMakeSignal(junction))
                {
                    case SignalCreationMode.Mainline:
                        _junctionSignals.Add(junction, CreateMainlineSignals(pack, junction));
                        break;
                    case SignalCreationMode.IntoYard:
                        _junctionSignals.Add(junction, CreateIntoYardSignals(pack, junction));
                        break;
                    case SignalCreationMode.IntoYardReverse:
                        _junctionSignals.Add(junction, CreateIntoYardReverseSignals(pack, junction));
                        break;
                    case SignalCreationMode.IntoPax:
                        _junctionSignals.Add(junction, CreateIntoPaxSignals(pack, junction));
                        break;
                    case SignalCreationMode.Shunting:
                        _shuntingSignals.AddRange(CreateShuntingSignals(pack, junction));
                        break;
                    default:
                        continue;
                }
            }

            sw.Stop();
            SignalsMod.Log($"Finished creating signals for {_junctionSignals.Count} junction(s), " +
                $"current total is {_signalRegister.Count} ({sw.Elapsed.TotalSeconds:F4}s)");
            SignalsMod.Log($"{_shuntingSignals.Count} shunting signal(s) were also created");

            for (int i = 0; i < MergeLoops; i++)
            {
                sw.Restart();
                count = MergeSignals(pack);
                sw.Stop();
                SignalsMod.Log($"Merged {count} signal(s) (loop {i + 1}/{MergeLoops}), " +
                    $"current total is {_signalRegister.Count} ({sw.Elapsed.TotalSeconds:F4}s)");
            }

            sw.Restart();
            CreateDistantSignals(pack);
            sw.Stop();
            SignalsMod.Log($"Finished creating {_distantSignals.Count} distant signal(s), " +
                $"current total is {_signalRegister.Count} ({sw.Elapsed.TotalSeconds:F4}s)");

            TrackChecker.StartBuildingMap();

            _updateCoro = StartCoroutine(UpdateRoutine());

            Camera.onPostRender += DebugRender;
        }

        private void CreateDistantSignals(SignalPack pack)
        {
            var hasNormal = pack.DistantSignal != null;
            var hasOld = pack.OldDistantSignal != null;

            // No signal, don't even try creating any.
            if (!hasNormal && !hasOld) return;

            foreach (var junction in _junctionSignals)
            {
                foreach (var signal in junction.Value.AllSignals)
                {
                    // Prefab isn't available or signal has no placement information.
                    if (signal.IsOld && !hasOld) continue;
                    if (!signal.IsOld && !hasNormal) continue;
                    if (!signal.PlacementInfo.HasValue) continue;

                    // Don't place distant signals in tracks without signs (usually inside stations).
                    var placement = signal.PlacementInfo.Value;
                    if (IsNonSign(placement.Track)) continue;

                    // Check the minimum track length.
                    var kpSet = placement.Track.GetKinkedPointSet();
                    if (kpSet.span < pack.DistantSignalMinimumTrackLength) continue;

                    // If the placement falls outside the track bounds...
                    var tSpan = placement.Span + (placement.Direction.IsOut() ? pack.DistantSignalDistance : -pack.DistantSignalDistance);
                    if (tSpan < 0 || tSpan > kpSet.span) continue;

                    // Actually place one.
                    _distantSignals.Add(CreateDistantForSignal(signal, signal.IsOld ? pack.OldDistantSignal! : pack.DistantSignal!, pack.DistantSignalDistance));
                }
            }
        }

        private int MergeSignals(SignalPack pack)
        {
            var toDelete = new Dictionary<TrackSignalController, HashSet<TrackSignalController>>();
            var toMerge = new Dictionary<TrackSignalController, HashSet<TrackSignalController>>();

            // Check for possible merge targets.
            foreach (var group in _junctionSignals)
            {
                foreach (var signal in group.Value.AllSignals)
                {
                    signal.UpdateBlock();

                    foreach (var block in signal.GetPotentialBlocks())
                    {
                        var next = block.NextSignal;

                        if (next is TrackSignalController track)
                        {
                            var reverse = IsSignalReverse(signal) && IsSignalReverse(track);
                            var source = reverse ? track : signal;
                            var target = reverse ? signal : track;

                            // Close enough to merge, signal types allow merging, and the signal that will be
                            // removed does not have a block that is too long.
                            if (block.Length <= ClosenessThreshold && CanBeMerged(source, target))
                            {
                                if (!reverse)
                                {
                                    next.UpdateBlock();
                                    var nextBlock = next.Block;

                                    if (nextBlock != null && nextBlock.Length < LongTrackThreshold)
                                    {
                                        AddToMap(target, source);
                                    }
                                }
                                else
                                {
                                    AddToMap(target, source);
                                }
                            }
                        }
                    }
                }
            }

            // Remove any target for which any signal leading to it does not meet conditions.
            foreach (var group in _junctionSignals)
            {
                foreach (var signal in group.Value.AllSignals)
                {
                    bool atLeastOneNext = false;

                    foreach (var block in signal.GetPotentialBlocks())
                    {
                        var next = block.NextSignal;
                        atLeastOneNext |= next != null;

                        if (next is TrackSignalController track)
                        {
                            var reverse = IsSignalReverse(signal) && IsSignalReverse(track);
                            var source = reverse ? track : signal;
                            var target = reverse ? signal : track;

                            // Fail if the target cannot be merged or the signal will be the result of a merge.
                            if (!CanBeMerged(source, target) || toMerge.ContainsKey(target))
                            {
                                // If reverse, don't check for the length, else fail for long tracks too.
                                if (reverse || (next.Block != null && next.Block.Length > LongTrackThreshold))
                                {
                                    RemoveFromMap(target);
                                }
                            }
                        }
                    }

                    // End of track signals are blocked from being merged.
                    if (!atLeastOneNext)
                    {
                        RemoveFromMap(signal);
                    }
                }
            }

            int count = toMerge.Count;

            foreach (var merge in toMerge)
            {
                var remain = merge.Key;
                var result = Merge(pack, remain, merge.Value);

                if (result != null)
                {
                    var group = remain.Group;
                    result.Group = group;

                    if (group != null)
                    {
                        // Check which signal the remaining one was...
                        if (group.JunctionSignal == remain && result is JunctionSignalController junction)
                        {
                            group.JunctionSignal = junction;
                        }
                        else if (group.ReverseJunctionSignal == remain)
                        {
                            group.ReverseJunctionSignal = result;
                        }
                        else
                        {
                            group.BranchSignals.Add(result);
                        }
                    }
                    else
                    {
                        Debug.LogError($"Remain signal {remain.Name} has no group, this shouldn't happen!");
                    }

                    remain.Destroy();

                    foreach (var item in merge.Value)
                    {
                        item.Destroy();
                    }
                }
            }

            return count;

            void AddToMap(TrackSignalController delete, TrackSignalController remain)
            {
                if (!toDelete.TryGetValue(delete, out var set) || set == null)
                {
                    set = new HashSet<TrackSignalController>();
                    toDelete[delete] = set;
                }

                set.Add(remain);

                if (!toMerge.TryGetValue(remain, out set) || set == null)
                {
                    set = new HashSet<TrackSignalController>();
                    toMerge[remain] = set;
                }

                set.Add(delete);
            }

            void RemoveFromMap(TrackSignalController signal)
            {
                if (!toDelete.TryGetValue(signal, out var set)) return;

                foreach (var item in set)
                {
                    if (toMerge.TryGetValue(item, out var set2))
                    {
                        set2.Remove(signal);
                    }
                }

                toDelete.Remove(signal);
            }

            static bool IsSignalReverse(TrackSignalController signal)
            {
                return signal.Group != null && signal.Group.ReverseJunctionSignal == signal;
            }
        }

        #region Creation testing

        private SignalCreationMode ShouldMakeSignal(Junction junction)
        {
            var inName = junction.inBranch.track.name;
            SignalsMod.LogVerbose($"Testing track '{inName}' for signals...");

            // Don't duplicate junctions and don't make signals at short dead ends.
            if (_junctionSignals.ContainsKey(junction) || InIsShortDeadEnd(junction))
            {
                return SignalCreationMode.None;
            }

            // If the in track belongs to a yard...
            if (inName.StartsWith(YardNameStart))
            {
                // Check all out branches.
                foreach (var branch in junction.outBranches)
                {
                    // Track ends after a switch for some reason, plop signal.
                    if (!branch.track.outIsConnected)
                    {
                        return SignalCreationMode.IntoYard;
                    }

                    // Get the track after the switch track.
                    var outName = branch.track.outBranch.track.name;

                    SignalsMod.LogVerbose($"Testing branch '{outName}' for signals...");

                    // If this yard track goes to a non yard track...
                    if (!outName.StartsWith(YardNameStart))
                    {
                        return SignalCreationMode.IntoYardReverse;
                    }
                }

                // Switch branches stay in the same yard, no signal needed.
                return SignalCreationMode.Shunting;
            }

            // Count very small branches as if they were a yard.
            // There's no regular mainline tracks fulfilling this condition.
            if (AreAllBranchesSmallNonSign(junction))
            {
                return SignalCreationMode.IntoYard;
            }

            // In case we are in a mainline, check all out branches for yards.
            foreach (var branch in junction.outBranches)
            {
                // Track ends after a switch for some reason, plop signal.
                if (!branch.track.outIsConnected)
                {
                    return SignalCreationMode.IntoYard;
                }

                // Get the track after the switch track.
                var outName = branch.track.outBranch.track.name;

                SignalsMod.LogVerbose($"Testing branch '{outName}' for signals...");

                // If this non yard track goes to a yard track...
                if (outName.StartsWith(YardNameStart))
                {
                    // If we want to make special signals for passenger tracks.
                    if (SignalsMod.Settings.PaxSignals && junction.outBranches.Any(x => IsPaxTrack(x.track.outBranch.track)))
                    {
                        return SignalCreationMode.IntoPax;
                    }

                    return SignalCreationMode.IntoYard;
                }
            }

            return SignalCreationMode.Mainline;
        }

        private static bool InIsShortDeadEnd(Junction junction)
        {
            // If the track is null here something is very wrong with the game, so don't check.
            var track = junction.inBranch.track;

            if (track.GetLength() > DeadEndThreshold)
            {
                return false;
            }

            var branch = track.inJunction == junction ?
                track.GetOutBranch() :
                track.GetInBranch();

            return branch == null;
        }

        private static bool IsPartOfYard(RailTrack track)
        {
            return track.name.StartsWith(YardNameStart);
        }

        private static bool IsSmallTrack(RailTrack track)
        {
            return track.GetLength() < SmallTrackThreshold;
        }

        private static bool IsPaxTrack(RailTrack track)
        {
            return track.name.EndsWith(PaxNameEnd);
        }

        private static bool IsNonSign(RailTrack track)
        {
            return track.name.StartsWith(NoSign);
        }

        private static bool IsLogicYardTrack(RailTrack track)
        {
            return !track.GetID().IsGeneric();
        }

        private static bool AreAllBranchesSmallNonSign(Junction junction)
        {
            foreach (var item in junction.outBranches)
            {
                var track = item.track.outBranch.track;

                if (track == null || track.GetLength() > VerySmallTrackThreshold || !IsNonSign(track))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsOld(Junction junction)
        {
            return OldAreaCalculator.IsWithinOldArea(junction.position);
        }

        #endregion

        #region Main creation methods

        private static JunctionSignalGroup CreateMainlineSignals(SignalPack pack, Junction junction)
        {
            SignalsMod.LogVerbose($"Making mainline signals for junction '{junction.junctionData.junctionIdLong}'");

            var old = IsOld(junction);
            var group = new JunctionSignalGroup(junction,
                CreateSignalAtJunction(junction, pack.GetJunctionSignal(old)),
                CreateBranchSignals(junction, pack.GetMainlineSignal(old)));

            foreach (var signal in group.AllSignals)
            {
                signal.IsOld = old;
                signal.Type = SignalType.Mainline;
                signal.PrefabType = PrefabType.Mainline;
            }

            if (group.JunctionSignal != null)
            {
                group.JunctionSignal.PrefabType = PrefabType.MainlineJunction;
            }

            return group;
        }

        private static JunctionSignalGroup CreateIntoYardSignals(SignalPack pack, Junction junction)
        {
            var smallBranches = AreAllBranchesSmallNonSign(junction);

            // Create more regular signals rather than the full into yard group.
            if (!smallBranches && junction.outBranches.Any(x => !IsPartOfYard(x.track.outBranch.track)))
            {
                return CreateIntoYardMainlineSignals(pack, junction);
            }

            SignalsMod.LogVerbose($"Making into {(smallBranches ? "small branches" : string.Empty)} " +
                $"yard signals for junction '{junction.junctionData.junctionIdLong}'");

            var old = IsOld(junction);
            var group = new JunctionSignalGroup(junction,
                CreateSignalAtJunction(junction, smallBranches ? pack.GetMainlineSignal(old) : pack.GetIntoYardSignal(old)),
                CreateSignalFromJunction(junction, pack.GetMainlineSignal(old)));

            if (group.JunctionSignal != null)
            {
                group.JunctionSignal.IsOld = old;
                group.JunctionSignal.Type = smallBranches ? SignalType.Mainline : SignalType.IntoYard;
                group.JunctionSignal.PrefabType = smallBranches ? PrefabType.Mainline : PrefabType.IntoYard;
            }

            if (group.ReverseJunctionSignal != null)
            {
                group.ReverseJunctionSignal.IsOld = old;
                group.ReverseJunctionSignal.Type = SignalType.Mainline;
                group.ReverseJunctionSignal.PrefabType = PrefabType.Mainline;
            }

            return group;
        }

        private static JunctionSignalGroup CreateIntoYardMainlineSignals(SignalPack pack, Junction junction)
        {
            SignalsMod.LogVerbose($"Making into yard mainline signals for junction '{junction.junctionData.junctionIdLong}'");

            var old = IsOld(junction);
            var group = new JunctionSignalGroup(junction,
                CreateSignalAtJunction(junction, pack.GetIntoYardSignal(old)),
                CreateBranchSignals(junction, pack.GetMainlineSignal(old)));

            foreach (var signal in group.AllSignals)
            {
                signal.IsOld = old;
                signal.Type = SignalType.Mainline;
                signal.PrefabType = PrefabType.Mainline;
            }

            if (group.JunctionSignal != null)
            {
                group.JunctionSignal.Type = SignalType.IntoYard;
                group.JunctionSignal.PrefabType= PrefabType.IntoYard;
            }

            return group;
        }

        private static JunctionSignalGroup CreateIntoYardReverseSignals(SignalPack pack, Junction junction)
        {
            SignalsMod.LogVerbose($"Making into yard reverse signals for junction '{junction.junctionData.junctionIdLong}'");

            var old = IsOld(junction);
            var inPax = IsPaxTrack(junction.inBranch.track) && (old ? pack.OldPassengerSignal : pack.PassengerSignal) != null;
            var group = new JunctionSignalGroup(junction,
                CreateSignalAtJunction(junction, inPax ? pack.GetPassengerSignal(old) : pack.GetJunctionSignal(old)),
                CreateBranchSignals(junction, pack.GetMainlineSignal(old)));

            foreach (var signal in group.AllSignals)
            {
                signal.IsOld = old;
                signal.Type = SignalType.Mainline;
                signal.PrefabType = PrefabType.Mainline;
            }

            if (group.JunctionSignal != null)
            {
                group.JunctionSignal.Type = inPax ? SignalType.OutPax : SignalType.Mainline;
                group.JunctionSignal.PrefabType = inPax ? PrefabType.OutPax : PrefabType.MainlineJunction;
            }

            return group;
        }

        private static JunctionSignalGroup CreateIntoPaxSignals(SignalPack pack, Junction junction)
        {
            SignalsMod.LogVerbose($"Making into pax signals for junction '{junction.junctionData.junctionIdLong}'");

            var old = IsOld(junction);
            var group = new JunctionSignalGroup(junction,
                CreateSignalAtJunction(junction, pack.GetIntoYardSignal(old)),
                CreateBranchSignalsPax(junction, pack.GetPassengerSignal(old), pack.GetMainlineSignal(old)));

            foreach (var signal in group.AllSignals)
            {
                signal.IsOld = old;
            }

            if (group.JunctionSignal != null)
            {
                group.JunctionSignal.Type = SignalType.IntoYard;
                group.JunctionSignal.PrefabType = PrefabType.IntoYard;
            }

            return group;
        }

        private static List<ShuntingSignalController> CreateShuntingSignals(SignalPack pack, Junction junction)
        {
            var old = IsOld(junction);
            var def = pack.GetShuntingSignal(old);
            var list = new List<ShuntingSignalController>();

            if (def == null) return list;

            //CreateJunctionSignal();
            CreateBranchSignals();

            return list;

            void CreateJunctionSignal()
            {
                var track = junction.inBranch.track;
                var kpSet = track.GetKinkedPointSet();
                var tDirJ = TrackUtils.TrackDirectionFromJunction(track, junction);
                var tSpan = kpSet.GetSpan(JunctionPlacementDistance / 4.0f, tDirJ);
                var index = kpSet.GetPointIndexForSpan(tSpan);
                var point = kpSet.points[index];

                var placement = new SignalPlacementInfo(track, tDirJ, index, tSpan);
                var signal = InstantiateFromDef(def, point.position, tDirJ.IsOut() ? point.forward : -point.forward, track);

                list.Add(new ShuntingSignalController(signal, junction.inBranch.track, tDirJ, placement));
            }

            void CreateBranchSignals()
            {
                EquiPointSet.Point? prev = null;
                float baseSpan = BranchPlacementDistance;

                // Check potential placements to see if they're too close to eachother.
                foreach (var branch in junction.outBranches)
                {
                    var track = branch.track.outBranch.track;
                    var kpSet = track.GetKinkedPointSet();
                    var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
                    var index = kpSet.GetPointIndexForSpan(kpSet.GetSpan(BranchPlacementDistance, tDirT));
                    var point = kpSet.points[index];

                    if (prev != null)
                    {
                        var distance = (point.position - prev.Value.position).sqrMagnitude;

                        // Just 1 confirmation is needed, so all of them are moved.
                        if (distance < BranchDistanceThreshold)
                        {
                            baseSpan = BranchPlacementDistance + 10;
                            break;
                        }
                    }

                    prev = point;
                }

                foreach (var branch in junction.outBranches)
                {
                    var track = branch.track.outBranch.track;

                    // Don't spam switches with them, only the actual numbered tracks and big ones.
                    if (!IsLogicYardTrack(track) && track.GetLength() <= SmallTrackThreshold) continue;

                    var kpSet = track.GetKinkedPointSet();
                    var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
                    var tSpan = kpSet.GetSpan(baseSpan, tDirT);
                    var index = kpSet.GetPointIndexForSpan(tSpan);
                    var point = kpSet.points[index];

                    var placement = new SignalPlacementInfo(track, tDirT, index, tSpan);
                    var signal = InstantiateFromDef(def, point.position, tDirT.IsOut() ? point.forward : -point.forward, track);

                    list.Add(new ShuntingSignalController(signal, track, tDirT, placement));
                }
            }
        }

        #endregion

        #region Internal creation methods

        private static SignalControllerDefinition GetForType(SignalPack pack, PrefabType prefabType, bool old) => prefabType switch
        {
            PrefabType.MainlineJunction => pack.GetJunctionSignal(old),
            PrefabType.IntoYard => pack.GetIntoYardSignal(old),
            PrefabType.OutPax => pack.GetPassengerSignal(old),
            _ => pack.GetMainlineSignal(old),
        };

        private static SignalControllerDefinition InstantiateFromDef(SignalControllerDefinition definition,
            Vector3d position, Vector3 direction, RailTrack track) =>
                Instantiate(definition, (Vector3)position, Helpers.FlattenLook(direction), track.transform);

        private static JunctionSignalController? CreateSignalAtJunction(Junction junction, SignalControllerDefinition definition)
        {
            var track = junction.inBranch.track;
            var kpSet = track.GetKinkedPointSet();
            var tDirJ = TrackUtils.TrackDirectionFromJunction(track, junction);
            var tSpan = kpSet.GetSpan(JunctionPlacementDistance, tDirJ);
            var index = kpSet.GetPointIndexForSpan(tSpan);
            var point = kpSet.points[index];

            var placement = new SignalPlacementInfo(track, tDirJ, index, tSpan);
            var signal = InstantiateFromDef(definition, point.position, tDirJ.IsOut() ? point.forward : -point.forward, track);

            return new JunctionSignalController(signal, junction, null, placement);
        }

        private static TrackSignalController? CreateSignalFromJunction(Junction junction, SignalControllerDefinition definition)
        {
            var track = junction.inBranch.track;
            var kpSet = track.GetKinkedPointSet();
            var tDirJ = TrackUtils.TrackDirectionFromJunction(track, junction);
            var tSpan = kpSet.GetSpan(JunctionPlacementDistance, tDirJ);
            var index = kpSet.GetPointIndexForSpan(tSpan);
            var point = kpSet.points[index];

            tDirJ = tDirJ.Flipped();
            var placement = new SignalPlacementInfo(track, tDirJ, index, tSpan);
            var signal = InstantiateFromDef(definition, point.position, tDirJ.IsOut() ? point.forward : -point.forward, track);

            return new TrackSignalController(signal, track, tDirJ.Flipped(), placement);
        }

        private static List<TrackSignalController> CreateBranchSignals(Junction junction, SignalControllerDefinition definition)
        {
            var signals = new List<TrackSignalController>();

            EquiPointSet.Point? prev = null;
            float baseSpan = BranchPlacementDistance;

            // Check potential placements to see if they're too close to eachother.
            foreach (var branch in junction.outBranches)
            {
                var track = branch.track.outBranch.track;
                var kpSet = track.GetKinkedPointSet();
                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
                var index = kpSet.GetPointIndexForSpan(kpSet.GetSpan(BranchPlacementDistance, tDirT));
                var point = kpSet.points[index];

                if (prev != null)
                {
                    var distance = (point.position - prev.Value.position).sqrMagnitude;

                    // Just 1 confirmation is needed, so all of them are moved.
                    if (distance < BranchDistanceThreshold)
                    {
                        baseSpan = BranchPlacementDistance + 10;
                        break;
                    }
                }

                prev = point;
            }

            foreach (var branch in junction.outBranches)
            {
                var track = branch.track.outBranch.track;

                // Skip very small branch tracks to avoid placement issues.
                if (IsSmallTrack(track)) continue;

                var kpSet = track.GetKinkedPointSet();
                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
                var tSpan = kpSet.GetSpan(baseSpan, tDirT);
                var index = kpSet.GetPointIndexForSpan(tSpan);
                var point = kpSet.points[index];

                var placement = new SignalPlacementInfo(track, tDirT, index, tSpan);
                var signal = InstantiateFromDef(definition, point.position, tDirT.IsOut() ? point.forward : -point.forward, track);

                signals.Add(new TrackSignalController(signal, branch.track, TrackDirection.In, placement));
            }

            return signals;
        }

        private static List<TrackSignalController> CreateBranchSignalsPax(Junction junction, SignalControllerDefinition pax, SignalControllerDefinition nonPax)
        {
            var signals = new List<TrackSignalController>();

            EquiPointSet.Point? prev = null;
            float baseSpan = BranchPlacementDistance;

            // Check potential placements to see if they're too close to eachother.
            foreach (var branch in junction.outBranches)
            {
                var track = branch.track.outBranch.track;
                var kpSet = track.GetKinkedPointSet();
                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
                var index = kpSet.GetPointIndexForSpan(kpSet.GetSpan(BranchPlacementDistance, tDirT));
                var point = kpSet.points[index];

                if (prev != null)
                {
                    var distance = (point.position - prev.Value.position).sqrMagnitude;

                    // Just 1 confirmation is needed, so all of them are moved.
                    if (distance < BranchDistanceThreshold)
                    {
                        baseSpan = BranchPlacementDistance + 10;
                        break;
                    }
                }

                prev = point;
            }

            foreach (var branch in junction.outBranches)
            {
                var track = branch.track.outBranch.track;
                var kpSet = track.GetKinkedPointSet();
                var tDirT = TrackUtils.TrackDirectionFromTrack(track, branch.track);
                var tSpan = kpSet.GetSpan(baseSpan, tDirT);
                var index = kpSet.GetPointIndexForSpan(tSpan);
                var point = kpSet.points[index];

                var paxEnd = IsPaxTrack(track);
                var placement = new SignalPlacementInfo(track, tDirT, index, tSpan);
                var signal = InstantiateFromDef(paxEnd ? pax : nonPax,
                    point.position, tDirT.IsOut() ? point.forward : -point.forward, track);

                var controller = new TrackSignalController(signal, branch.track, TrackDirection.In, placement)
                {
                    Type = paxEnd ? SignalType.OutPax : SignalType.Mainline,
                    PrefabType = paxEnd ? PrefabType.OutPax : PrefabType.Mainline
                };

                signals.Add(controller);
            }

            return signals;
        }

        private static DistantSignalController CreateDistantForSignal(BasicSignalController home, SignalControllerDefinition definition, float distance)
        {
            // This is checked before calling the method.
            var placement = home.PlacementInfo!.Value;

            var isOut = placement.Direction.IsOut();
            var kpSet = placement.Track.GetKinkedPointSet();
            var point = kpSet.points[placement.PointIndex];
            var tSpan = point.span + (isOut ? distance : -distance);
            var index = kpSet.GetPointIndexForSpan(tSpan);

            point = kpSet.points[index];
            placement.PointIndex = index;
            placement.Span = tSpan;
            var signal = InstantiateFromDef(definition, point.position, isOut ? point.forward : -point.forward, placement.Track);

            return new DistantSignalController(signal, home, placement, distance);
        }

        #endregion

        #region Post processing

        private static bool CanBeMerged(BasicSignalController source, BasicSignalController target)
        {
            return source.Type switch
            {
                SignalType.Shunting => false,
                SignalType.Distant => false,
                _ => target.Type switch
                {
                    SignalType.Mainline => true,
                    SignalType.IntoYard => true,
                    SignalType.Shunting => false,
                    SignalType.Distant => false,
                    SignalType.OutPax => false,
                    SignalType.Other => false,
                    _ => false,
                }
            };
        }

        private static SignalType GetHigher(SignalType a, SignalType b)
        {
            if (Check(SignalType.IntoYard)) return SignalType.IntoYard;
            if (Check(SignalType.OutPax)) return SignalType.OutPax;
            if (Check(SignalType.Mainline)) return SignalType.Mainline;

            return a;

            bool Check(SignalType type)
            {
                return a == type || b == type;
            }
        }

        private static PrefabType GetHigher(PrefabType a, PrefabType b)
        {
            if (Check(PrefabType.IntoYard)) return PrefabType.IntoYard;
            if (Check(PrefabType.OutPax)) return PrefabType.OutPax;
            if (Check(PrefabType.MainlineJunction)) return PrefabType.MainlineJunction;
            if (Check(PrefabType.Mainline)) return PrefabType.Mainline;

            return a;

            bool Check(PrefabType type)
            {
                return a == type || b == type;
            }
        }

        private static TrackSignalController? Merge(SignalPack pack, TrackSignalController remain, HashSet<TrackSignalController> remove)
        {
            if (!remain.PlacementInfo.HasValue) return null;

            var signalType = remain.Type;
            var prefabType = remain.PrefabType;

            foreach (var item in remove)
            {
                signalType = GetHigher(signalType, item.Type);
                prefabType = GetHigher(prefabType, item.PrefabType);
            }

            var info = remain.PlacementInfo.Value;
            var point = info.Track.GetKinkedPointSet().points[info.PointIndex];
            var signal = InstantiateFromDef(GetForType(pack, prefabType, remain.IsOld),
                point.position, remain.Definition.transform.forward, info.Track);

            TrackSignalController controller;

            if (remain is JunctionSignalController junctionRemain)
            {
                controller = new JunctionSignalController(signal, junctionRemain.Junction, null, info);
            }
            else
            {
                controller = new TrackSignalController(signal, remain.StartingTrack, remain.Direction, info);
            }

            controller.IsOld = remain.IsOld;
            controller.Type = signalType;
            controller.PrefabType = prefabType;
            return controller;
        }

        #endregion

        #endregion

        // Update all signals from here.
        private System.Collections.IEnumerator UpdateRoutine()
        {
            while (PlayerManager.ActiveCamera == null) yield return null;

            yield return WaitFor.Seconds(UpdateTime);

            while (true)
            {
                // Check how many signals to update per frame so they all update roughly once a second.
                int count = Mathf.CeilToInt(_signalRegister.Count * Time.fixedDeltaTime);

                // Loop through all registered signals.
                for (int start = 0; start < _signalRegister.Count; start += count)
                {
                    // Loop through a batch of signals. Updates are distributed so they don't all update
                    // at once, but batched so timing is consistent.
                    for (int current = start; current < start + count && current < _signalRegister.Count; current++)
                    {
                        // Stop updating if camera is gone.
                        while (PlayerManager.ActiveCamera == null) yield return WaitFor.Seconds(UpdateTime);

                        var signal = _signalRegister[current];

                        // Check if it can be updated.
                        if (!signal.SafetyCheck())
                        {
                            current--;
                            continue;
                        }

                        // Distance optimisation.
                        signal.Optimise();

                        // Skip updating if not needed.
                        var update = signal.ShouldUpdate();
                        if (!update && !signal.HasUpdatesQueued) continue;

                        // Actually update.
                        signal.UpdateAspect(update);
                    }

                    yield return WaitFor.FixedUpdate;
                }
            }
        }

        /// <summary>
        /// Register a <see cref="BasicSignalController"/> to be automatically updated.
        /// </summary>
        public void RegisterSignal(BasicSignalController signal)
        {
            _signalRegister.Add(signal);
        }

        /// <summary>
        /// Unregister a <see cref="BasicSignalController"/> from being automatically updated.
        /// </summary>
        /// <returns><see langword="true"/> if the signal was successfully removed, and <see langword="false"/> otherwise.</returns>
        public bool UnregisterSignal(BasicSignalController signal)
        {
            return _signalRegister.Remove(signal);
        }

        #region Utility

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

        // For RUE debug.
        private JunctionSignalGroup? GetGroup(int id)
        {
            TryGetJunctionGroup(id, out var group);
            return group;
        }

        #endregion
    }
}
