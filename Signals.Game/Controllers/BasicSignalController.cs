using Signals.Common;
using Signals.Game.Aspects;
using Signals.Game.Railway;
using Signals.Game.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Signals.Game.Controllers
{
    /// <summary>
    /// Base class for a signal controller.
    /// </summary>
    public class BasicSignalController
    {
        #region Static

        // Signals too far from the camera aren't updated.
        private const float SkipUpdateDistanceSqr = 1500 * 1500;

        protected const float UpdateTime = 1.0f;
        protected const float CrossingMinDistanceSqr = 7.5f * 7.5f;
        protected const int OffValue = -1;
        protected const int UpdatePropagation = 3;
        private static int s_idGen = 0;
        private static object s_lock = new object();

        private static int GetGenId()
        {
            int value;

            lock (s_lock)
            {
                value = s_idGen++;
            }

            return value;
        }

        internal static void ResetIdGeneration()
        {
            lock (s_lock)
            {
                s_idGen = 0;
            }
        }

        #endregion

        #region Members

        private string _orientationSimple = string.Empty;
        private string _orientation = string.Empty;
        private string _internalName = string.Empty;
        private bool _needName = true;

        protected int UpdateRequested = 0;

        public readonly int Id;
        public SignalType Type = SignalType.NotSet;
        public PrefabType PrefabType = PrefabType.NotSet;
        public bool IsOld;
        public bool ActingAsDistant = false;
        public bool ShortDistance = false;
        /// <summary>
        /// Override the name of this signal.
        /// </summary>
        public string NameOverride = string.Empty;

        #endregion

        #region Properties

        /// <summary>
        /// The definition and <see cref="GameObject"/> of the signal.
        /// </summary>
        public SignalControllerDefinition Definition { get; private set; }
        /// <summary>
        /// The junction group this signal belongs to.
        /// </summary>
        public JunctionSignalGroup? Group { get; internal set; }
        /// <summary>
        /// The signals belonging to this controller.
        /// </summary>
        public Signal[] Signals { get; private set; }
        /// <summary>
        /// The shunting signal belonging to this controller.
        /// </summary>
        public Signal[] ShuntingSignals { get; private set; }
        /// <summary>
        /// Information about where this signal was placed.
        /// </summary>
        public SignalPlacementInfo? PlacementInfo { get; private set; }
        public int? RequiredJunctionBranch { get; private set; } = null;
        public Junction? GroupJunction => Group?.Junction;
        public IEnumerable<Signal> AllSignals
        {
            get
            {
                foreach (var signal in Signals)
                {
                    yield return signal;
                }

                foreach (var signal in ShuntingSignals)
                {
                    yield return signal;
                }
            }
        }
        /// <summary>
        /// <see langword="true"/> if this signal exists in the world.
        /// </summary>
        public bool Exists => Definition != null;
        public bool HasUpdatesQueued => UpdateRequested > 0;
        /// <summary>
        /// The position in the world of this signal.
        /// </summary>
        public Vector3 Position => Definition.transform.position;

        public string OrientationSimple
        {
            get
            {
                if (string.IsNullOrEmpty(_orientationSimple))
                {
                    _orientationSimple = Helpers.OrientationSimple(Vector3.forward, -Definition.transform.forward);
                }

                return _orientationSimple;
            }
        }
        public string Orientation
        {
            get
            {
                if (string.IsNullOrEmpty(_orientation))
                {
                    _orientation = Helpers.Orientation(Vector3.forward, -Definition.transform.forward);
                }

                return _orientation;
            }
        }
        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(NameOverride)) return NameOverride;

                return InternalName;
            }
        }
        public string InternalName
        {
            get
            {
                if (_needName)
                {
                    _internalName = GenerateName();
                    _needName = false;
                }

                return _internalName;
            }
        }

        #endregion

        #region Events

        public Action<BasicSignalController>? Destroyed;
        public Action<BasicSignalController>? PreUpdate;
        public Action<Signal, IAspect?>? AnyAspectChanged;
        public Action<int?>? RequiredBranchChanged;

        #endregion

        public BasicSignalController(SignalControllerDefinition def, SignalPlacementInfo? placementInfo)
        {
            Id = GetGenId();
            Definition = def;
            PlacementInfo = placementInfo;

            Signals = def.Signals.Select(x => new Signal(this, x, false)).ToArray();
            ShuntingSignals = def.ShuntingSignals.Select(x => new Signal(this, x, true)).ToArray();

            TrackChecker.OnMapBuilt += FixPositionDueToCrossing;
            SignalManager.Instance.RegisterController(this);

            UpdateTracksideObjects();
        }

        private void FixPositionDueToCrossing(Dictionary<RailTrack, TrackChecker.TrackIntersectionPoints> junctionMap)
        {
            TrackChecker.OnMapBuilt -= FixPositionDueToCrossing;

            if (!Exists || PlacementInfo == null) return;

            var positions = new List<Vector3>();
            var shouldMoveForwards = false;

            foreach (var item in junctionMap)
            {
                foreach (var (Track, Position) in item.Value.IntersectionPoints)
                {
                    if (Helpers.DistanceSqr(Definition.transform.position, Position.position) < CrossingMinDistanceSqr)
                    {
                        positions.Add(Position.position);
                        shouldMoveForwards |= ShouldMoveForwards(Track);
                    }
                }
            }

            if (positions.Count == 0) return;

            var varDot = shouldMoveForwards ? float.NegativeInfinity : float.PositiveInfinity;
            var forward = -Definition.transform.forward;
            var position = Definition.transform.position;
            var targetPos = position;

            foreach (var pos in positions)
            {
                var dif = pos - position;
                var dot = Vector3.Dot(forward, dif);

                if (shouldMoveForwards ? dot > varDot : dot < varDot)
                {
                    varDot = dot;
                    targetPos = pos;
                }
            }

            var offset = (targetPos + forward * 2.5f) - position;

            OffsetOnTrack(Mathf.RoundToInt(offset.magnitude * 2));
        }

        private void UpdateTracksideObjects()
        {
            if (!PlacementInfo.HasValue) return;

            var placement = PlacementInfo.Value;

            var isOut = placement.Direction.IsOut();
            var kpSet = placement.Track.GetKinkedPointSet();

            foreach (var item in Definition.TracksideObjects)
            {
                var span = Helpers.ClampD(placement.Span + (isOut ? -item.OffsetFromController : item.OffsetFromController), 0, kpSet.span);
                var point = kpSet.points[kpSet.GetPointIndexForSpan(span)];
                var offset = placement.OppositeSide ? -item.OffsetFromTrack : item.OffsetFromTrack;

                if (item.AtRail)
                {
                    offset = offset > 0 ? TracksideObject.CurrentGauge : -TracksideObject.CurrentGauge;
                }

                item.transform.rotation = Quaternion.LookRotation(isOut ? point.forward : -point.forward);
                item.transform.position = (Vector3)point.position + item.transform.right * offset;
                item.transform.localScale = item.MirrorWhenOnOppositeSide ? new Vector3(-1, 1, 1) : Vector3.one;
            }
        }

        protected virtual bool ShouldMoveForwards(RailTrack track)
        {
            return true;
        }

        protected virtual string GenerateName()
        {
            var pack = SignalManager.CurrentPack;
            string text = string.Empty;

            if (StationControllerCache.TryGetStationInfo(this, out var station))
            {
                station.Sort();

                if (Type.IsEntry())
                {
                    // Just the index of the entry signal.
                    var index = station.EntrySignals.IndexOf(this) + 1;
                    text = ApplyFormat(pack.EntryFormat, Id, station.Station, index);
                    goto End;
                }
                else if (Type.IsAnyExit())
                {
                    var group = station.GetExitGroupFor(this, out var groupIndex);

                    if (group != null)
                    {
                        // Total entry signals + group number, so it starts after the entry signals.
                        var index1 = station.EntrySignals.Count + groupIndex + 1;
                        // Index of the exit signal in the group.
                        var index2 = group.ExitSignals.IndexOf(this) + 1;
                        text = ApplyFormat(pack.ExitFormat, Id, station.Station, index1, index2);
                        goto End;
                    }

                    // Fallback to the index after the groups.
                    var index = station.EntrySignals.Count + station.ExitGroups.Count + station.ExitNoGroupSignals.IndexOf(this) + 1;
                    text = ApplyFormat(pack.EntryFormat, Id, station.Station, index);
                    goto End;
                }
                else
                {
                    var index = station.MiscSignals.IndexOf(this) + 1;
                    text = ApplyFormat(pack.GenericStationFormat, Id, station.Station, index);
                    goto End;
                }
            }

            if (PlacementInfo.HasValue)
            {
                var index = StationControllerCache.GetIndex(this) + 1;

                if (index > 0)
                {
                    // Get the track numbers.
                    var numbers = new string(PlacementInfo.Value.Track.name.Where(char.IsDigit).ToArray());
                    text = ApplyFormat(Type == SignalType.Mainline ? pack.MainlineFormat : pack.TrackFormat, Id, numbers, index);
                    goto End;
                }
            }

            return string.Format(pack.FallbackFormat, Id);

        End:
            return string.IsNullOrEmpty(text) ? string.Format(pack.FallbackFormat, Id) : text;

            static string ApplyFormat(string format, int id, string? optionalExtra, params int[] indexes)
            {
                int extra = string.IsNullOrEmpty(optionalExtra) ? 1 : 2;
                var transformed = new object[indexes.Length * 2 + extra];

                transformed[0] = id;

                if (!string.IsNullOrEmpty(optionalExtra))
                {
                    transformed[1] = optionalExtra!;
                }

                for (int i = 0; i < indexes.Length; i++)
                {
                    transformed[i * 2 + extra] = indexes[i];
                    transformed[i * 2 + extra + 1] = Helpers.IntToLetters(indexes[i]);
                }

                return string.Format(format, transformed);
            }
        }

        /// <summary>
        /// Calculates the squared distance from the signal to the camera.
        /// </summary>
        public float GetCameraDistanceSqr()
        {
            if (PlayerManager.ActiveCamera == null) return float.PositiveInfinity;

            return Helpers.DistanceSqr(Definition.transform.position, PlayerManager.ActiveCamera.transform.position);
        }

        public bool OffsetOnTrack(int pointIndexOffset)
        {
            if (!PlacementInfo.HasValue) return false;

            var placement = PlacementInfo.Value;

            var isOut = placement.Direction.IsOut();
            var kpSet = placement.Track.GetKinkedPointSet();
            var index = Helpers.ClampBounds(placement.PointIndex + pointIndexOffset, kpSet.points);
            var point = kpSet.points[index];

            placement.PointIndex = index;
            PlacementInfo = placement;
            Definition.transform.rotation = Quaternion.LookRotation(isOut ? point.forward : -point.forward);
            Definition.transform.position = (Vector3)point.position + Definition.transform.right *
                (placement.OppositeSide ? -Definition.Offset : Definition.Offset);

            UpdateTracksideObjects();
            return true;
        }

        /// <summary>
        /// Checks if it is safe to continue using this signal instance.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if further processing can be done, otherwise <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// If <see langword="false"/> is returned, this signal should be discarded and all further processing
        /// stopped. It is likely the instanced object assigned to it has been destroyed.
        /// </remarks>
        public bool SafetyCheck()
        {
            if (Exists)
            {
                return true;
            }

            Destroy();
            return false;
        }

        /// <summary>
        /// Destroys this signal (controller and game object). If this is part of a <see cref="JunctionSignalGroup"/>,
        /// then it is also removed from it.
        /// </summary>
        public void Destroy()
        {
            if (Definition != null)
            {
                UnityEngine.Object.Destroy(Definition.gameObject);
            }

            if (Group != null)
            {
                if (Group.JunctionSignal == this)
                {
                    Group.JunctionSignal = null;
                }
                if (Group.ReverseJunctionSignal == this)
                {
                    Group.ReverseJunctionSignal = null;
                }
                Group.BranchSignals.RemoveAll(x => x == this);
            }

            foreach (var signal in AllSignals)
            {
                signal.Destroy();
            }

            TrackChecker.OnMapBuilt -= FixPositionDueToCrossing;
            SignalManager.Instance.UnregisterController(this);
            Destroyed?.Invoke(this);
        }

        public void RequestUpdate(int level)
        {
            UpdateRequested = Mathf.Max(UpdateRequested, level);
        }

        /// <summary>
        /// Whether the normal update cycle should be run or not.
        /// </summary>
        public virtual bool ShouldUpdate()
        {
            var dist = GetCameraDistanceSqr();

            // If the camera is too far from the signal, skip updating.
            return dist < SkipUpdateDistanceSqr;
        }

        /// <summary>
        /// Update the current <see cref="Block"/> before the signal is updated.
        /// </summary>
        public virtual void UpdateBlocks() { }

        public virtual void FlagAllBlocksForUpdating()
        {
            foreach (var signal in AllSignals)
            {
                if (signal.Block == null) continue;

                signal.Block.FlagForUpdating();
            }
        }

        /// <summary>
        /// Updates the current aspect based on the conditions of <see cref="AllAspects"/>.
        /// </summary>
        /// <param name="startPropagate">Whether this signal should propagate its updates to the signals afterwards.</param>
        public void Update(bool forced, bool startPropagate)
        {
            UpdateBlocks();

            foreach (var signal in AllSignals)
            {
                // Update the reservation.
                if (TrackReserver.HasReservation(signal) && !TrackReserver.UpdateReservation(signal))
                {
                    SignalsMod.Warning($"Could not update reservation for signal {signal.Id}, old reservation is kept.");
                }

                signal.UpdateAspect(forced);
            }

            // Request the next signal to be updated to propagate out of range.
            UpdateRequested = Mathf.Max(UpdateRequested - 1, 0);
            GetNextController()?.RequestUpdate(startPropagate ? UpdatePropagation : UpdateRequested);
        }

        /// <summary>
        /// Set or clear a required branch for the junction of this controller's group.
        /// </summary>
        /// <param name="index">The branch index.</param>
        /// <remarks>Use a negative number to clear the requirement.</remarks>
        public void ChangeRequiredBranch(int index)
        {
            if (index < 0)
            {
                if (!RequiredJunctionBranch.HasValue) return;

                RequiredJunctionBranch = null;
                RequiredBranchChanged?.Invoke(null);
                return;
            }

            if (RequiredJunctionBranch.HasValue && RequiredJunctionBranch.Value == index) return;

            RequiredJunctionBranch = index;
            RequiredBranchChanged?.Invoke(index);
        }

        public virtual Signal? GetActiveSignal()
        {
            return Signals.Length > 0 ? Signals[0] : null;
        }

        public Signal? GetMostRestrictiveSignal()
        {
            if (Signals.Length == 0) return null;

            var signal = Signals[0];

            for (int i = 1; i < Signals.Length; i++)
            {
                var test = Signals[i];

                if (test.IsOff) continue;

                if (test.CurrentAspectIndex < signal.CurrentAspectIndex)
                {
                    signal = test;
                }
            }

            return signal;
        }

        public Signal? GetControllerSignal() => Definition.Mode switch
        {
            ControllerMode.MostRestrictive => GetMostRestrictiveSignal(),
            _ => GetActiveSignal(),
        };

        /// <summary>
        /// Gets the next controller from the active signal.
        /// </summary>
        /// <returns>The next</returns>
        public virtual BasicSignalController? GetNextController()
        {
            var signal = GetControllerSignal();

            if (signal == null) return null;

            return signal.GetNextController();
        }

        /// <summary>
        /// Keeps iterating through signals until <paramref name="condition"/> is met.
        /// </summary>
        /// <param name="condition">The condition a signal must meet.</param>
        /// <returns></returns>
        public virtual BasicSignalController? GetNextControllerCondition(Predicate<BasicSignalController> condition)
        {
            var signal = GetControllerSignal();

            if (signal == null) return null;

            return signal.GetNextControllerCondition(condition);
        }

        public virtual IEnumerable<TrackBlock> GetPotentialBlocks()
        {
            foreach (var item in Signals)
            {
                if (item.Block != null)
                {
                    yield return item.Block;
                }
            }
        }

        public virtual IEnumerable<(Signal Signal, IEnumerable<BasicSignalController> Controllers)> GetPotentialNextControllers()
        {
            foreach (var item in Signals)
            {
                if (item.Block != null && item.Block.NextController != null)
                {
                    yield return (item, new[] { item.Block.NextController });
                }
            }
        }

        public TrackBlock? GetLongestBlock()
        {
            if (Signals.Length == 0) return null;

            var block = Signals[0].Block;

            for (int i = 1; i < Signals.Length; i++)
            {
                var signal = Signals[i];

                if (signal.Block == null) continue;

                if (block == null || block.Length < signal.Block.Length)
                {
                    block = signal.Block;
                }
            }

            return block;
        }

        public static BasicSignalController? Replace(BasicSignalController original, SignalControllerDefinition def)
        {
            if (!original.PlacementInfo.HasValue) return null;

            var replacement = new BasicSignalController(def, original.PlacementInfo.Value)
            {
                Group = original.Group,
                ActingAsDistant = original.ActingAsDistant,
                ShortDistance = original.ShortDistance
            };
            original.Destroy();

            return replacement;
        }
    }
}
