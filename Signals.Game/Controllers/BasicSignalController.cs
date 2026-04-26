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

        #endregion

        #region Members

        protected string InternalName = string.Empty;
        protected int UpdateRequested = 0;

        public readonly int Id;
        public SignalType Type = SignalType.NotSet;
        public PrefabType PrefabType = PrefabType.NotSet;
        public bool IsOld;
        /// <summary>
        /// Override the name of this signal.
        /// </summary>
        public string NameOverride = string.Empty;

        #endregion

        #region Properties

        protected char PlacementLetter => PlacementInfo.HasValue ? PlacementInfo.Value.Direction.IsOut() ? 'O' : 'I' : '-';

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
        public Signal? ShuntingSignal { get; private set; }
        /// <summary>
        /// Information about where this signal was placed.
        /// </summary>
        public SignalPlacementInfo? PlacementInfo { get; private set; }
        public int? RequiredJunctionBranch { get; private set; } = null;
        public Junction? GroupJunction => Group?.Junction;

        public virtual string Name => string.IsNullOrEmpty(NameOverride) ? InternalName : NameOverride;
        /// <summary>
        /// <see langword="true"/> if this signal exists in the world.
        /// </summary>
        public bool Exists => Definition != null;
        public bool HasUpdatesQueued => UpdateRequested > 0;
        /// <summary>
        /// The position in the world of this signal.
        /// </summary>
        public Vector3 Position => Definition.transform.position;

        #endregion

        #region Events

        public Action<BasicSignalController>? Destroyed;
        public Action<Signal, AspectBase?>? AnyAspectChanged;
        public Action<int?>? RequiredBranchChanged;

        #endregion

        public BasicSignalController(SignalControllerDefinition def, SignalPlacementInfo? placementInfo)
        {
            Id = GetGenId();
            Definition = def;
            PlacementInfo = placementInfo;

            Signals = def.Signals.Select(x => new Signal(this, x)).ToArray();

            if (def.ShuntingSignal != null)
            {
                ShuntingSignal = new Signal(this, def.ShuntingSignal);
            }

            TrackChecker.OnMapBuilt += FixPositionDueToCrossing;
            SignalManager.Instance.RegisterController(this);
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

        protected virtual bool ShouldMoveForwards(RailTrack track)
        {
            return true;
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
            Definition.transform.position = (Vector3)point.position + Vector3.right *
                (placement.OppositeSide ? -Definition.Offset : Definition.Offset);

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

            foreach (var signal in GetAllSignals())
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

        /// <summary>
        /// Updates the current aspect based on the conditions of <see cref="AllAspects"/>.
        /// </summary>
        /// <param name="startPropagate">Whether this signal should propagate its updates to the signals afterwards.</param>
        public void Update(bool forced, bool startPropagate)
        {
            UpdateBlocks();

            foreach (var signal in Signals)
            {
                // Update the reservation.
                if (TrackReserver.HasReservation(signal) && !TrackReserver.UpdateReservation(signal))
                {
                    SignalsMod.Warning($"Could not update reservation for signal {signal.Id}, old reservation is kept.");
                }

                signal.UpdateAspect(forced);
            }

            ShuntingSignal?.UpdateAspect(forced);

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
                RequiredJunctionBranch = null;
                RequiredBranchChanged?.Invoke(null);
                return;
            }

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

        public IEnumerable<Signal> GetAllSignals()
        {
            foreach(var signal in Signals)
            {
                yield return signal;
            }

            if (ShuntingSignal != null)
            {
                yield return ShuntingSignal;
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
    }
}
