using Signals.Common;
using Signals.Game.Aspects;
using Signals.Game.Displays;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Signals.Game.Controllers
{
    public class BasicSignalController
    {
        protected const float UpdateTime = 1.0f;
        protected const float OptimiseDistanceSqr = 2500 * 2500;
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

        private int? _baseAnimation;
        private SignalHover _hover;
        private DebugComp? _debugComp;

        protected Coroutine? AnimatorDisabler;
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
        /// <summary>
        /// How the signal should be updated. Optimisation routines still execute no matter this state.
        /// </summary>
        public SignalOperationMode Operation = SignalOperationMode.Automatic;
        public int ManualOverrideAspect;

        protected char PlacementLetter => PlacementInfo.HasValue ? PlacementInfo.Value.Direction.IsOut() ? 'O' : 'I' : '-';

        /// <summary>
        /// The definition and <see cref="GameObject"/> of the signal.
        /// </summary>
        public SignalControllerDefinition Definition { get; private set; }
        /// <summary>
        /// The index of the current aspect. Use <see cref="ChangeAspect(int)"/> to change this value.
        /// </summary>
        public int CurrentAspectIndex { get; private set; }
        public AspectBase[] AllAspects { get; private set; }
        public SignalLight[] AllLights { get; private set; }
        public InfoDisplay[] AllDisplays { get; private set; }
        public AspectBase[] AllIndicators { get; private set; }
        /// <summary>
        /// Information about where this signal was placed.
        /// </summary>
        public SignalPlacementInfo? PlacementInfo { get; private set; }
        public TrackBlock? Block { get; set; }
        /// <summary>
        /// The junction group this signal belongs to.
        /// </summary>
        public JunctionSignalGroup? Group { get; internal set; }

        public virtual string Name => string.IsNullOrEmpty(NameOverride) ? InternalName : NameOverride;
        /// <summary>
        /// Is <see langword="true"/> if this signal exists in the world.
        /// </summary>
        public bool Exists => Definition != null;
        public bool IsOn => CurrentAspectIndex >= 0;
        public bool HasUpdatesQueued => UpdateRequested > 0;
        /// <summary>
        /// Is <see langword="null"/> if the signal is off.
        /// </summary>
        public AspectBase? CurrentAspect => IsOn ? AllAspects[CurrentAspectIndex] : null;
        /// <summary>
        /// The position in the world of this signal.
        /// </summary>
        public Vector3 Position => Definition.transform.position;
        public bool Hovered => ReflectionHelpers.IsHovered(_hover);

        public Action<AspectBase?>? AspectChanged;
        public Action<InfoDisplay[]>? DisplaysUpdated;
        public Action<BasicSignalController>? Destroyed;

        public BasicSignalController(SignalControllerDefinition def, SignalPlacementInfo? placementInfo)
        {
            Id = GetGenId();
            Definition = def;
            CurrentAspectIndex = OffValue;
            PlacementInfo = placementInfo;

            if (def.Aspects.Any(x => x == null)) SignalsMod.Error($"Null aspect in {def.name}");
            if (def.Displays.Any(x => x == null)) SignalsMod.Error($"Null display in {def.name}");
            if (def.Indicators.Any(x => x == null)) SignalsMod.Error($"Null indicator in {def.name}");

            // Instantiate all aspect implementations.
            AllAspects = def.Aspects.Select(x => AspectCreator.Create(this, x)).Where(x => x != null).ToArray()!;

            // Get an array of all lights.
            AllLights = def.GetComponentsInChildren<SignalLight>(true);

            // Same as aspects but for displays.
            AllDisplays = def.Displays.Select(x => DisplayCreator.Create(this, x)).Where(x => x != null).ToArray()!;

            // And finally the same for indicators.
            AllIndicators = def.Indicators.Select(x => AspectCreator.Create(this, x)).Where(x => x != null).ToArray()!;

            // If there's an animator, set up the default state
            if (def.Animator != null)
            {
                _baseAnimation = def.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
                def.Animator.enabled = false;
            }

            _hover = def.GetComponent<SignalHover>();
            _hover.Initialise(def.OffStateHUDSprite);

            TrackChecker.OnMapBuilt += FixPositionDueToCrossing;
            SignalManager.Instance.RegisterSignal(this);
            _debugComp = DebugComp.AddToDef(this);
        }

        private void FixPositionDueToCrossing(Dictionary<RailTrack, TrackChecker.TrackIntersectionPoints> junctionMap)
        {
            TrackChecker.OnMapBuilt -= FixPositionDueToCrossing;

            if (!Exists || PlacementInfo == null) return;

            var positions = new List<Vector3>();
            var shouldMoveForwards = false;

            foreach (var item in junctionMap)
            {
                foreach (var (_, Position) in item.Value.IntersectionPoints)
                {
                    if (Helpers.DistanceSqr(Definition.transform.position, Position.position) < CrossingMinDistanceSqr)
                    {
                        positions.Add(Position.position);
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

        // Used to disable child objects of the signal to increase performance.
        // The distance set is 2.5km, which is far enough that signals can realistically be seen,
        // but still not too far that it is useless.
        internal void Optimise()
        {
            if (PlayerManager.ActiveCamera == null) return;

            bool active = GetCameraDistanceSqr() <= OptimiseDistanceSqr;

            foreach (Transform t in Definition.transform)
            {
                t.gameObject.SetActive(active);
            }
        }

        internal void DisableAnimator(float time)
        {
            if (Definition.Animator == null) return;

            if (AnimatorDisabler != null)
            {
                Definition.StopCoroutine(AnimatorDisabler);
            }

            // Disable the animator after some time. Since the animations are instant
            AnimatorDisabler = Definition.StartCoroutine(Helpers.DisableBehaviour(Definition.Animator, time));
        }

        /// <summary>
        /// Calculates the squared distance from the signal to the camera.
        /// </summary>
        public float GetCameraDistanceSqr()
        {
            return Helpers.DistanceSqr(Definition.transform.position, PlayerManager.ActiveCamera.transform.position);
        }

        protected bool IsAspectManuallyOverriden(int index)
        {
            if (Operation == SignalOperationMode.Automatic)
            {
                return false;
            }

            return index == ManualOverrideAspect;
        }

        protected bool IsTempOverrideMatched(int index)
        {
            if (Operation != SignalOperationMode.TempOverride)
            {
                return false;
            }

            return index <= ManualOverrideAspect;
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
            Definition.transform.position = (Vector3)point.position;
            Definition.transform.rotation = Quaternion.LookRotation(isOut ? point.forward : -point.forward);

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

            TrackChecker.OnMapBuilt -= FixPositionDueToCrossing;
            SignalManager.Instance.UnregisterSignal(this);
            Destroyed?.Invoke(this);
        }

        /// <summary>
        /// Turns off the signal until the next state update.
        /// </summary>
        /// <param name="keep">If true, keeps the signal off.</param>
        /// <returns><see langword="true"/> if the signal was turned off successfuly, <see langword="false"/> otherwise.</returns>
        /// <remarks>In case the aspect was turned off successfully, <see cref="AspectChanged"/> will be called.</remarks>
        public bool TurnOff()
        {
            if (!IsOn) return false;

            CurrentAspect?.Unapply();

            foreach (var item in AllIndicators)
            {
                item.Unapply();
            }

            foreach (var item in AllLights)
            {
                item.TurnOff();
            }

            // Try to reset the animator state if it exists.
            if (Definition.Animator != null && _baseAnimation.HasValue)
            {
                var time = CurrentAspect != null ? CurrentAspect.Definition.AnimationTime : 1;
                Definition.Animator.enabled = true;
                Definition.Animator.CrossFadeInFixedTime(_baseAnimation.Value, time, 0);
                DisableAnimator(time + 0.1f);
            }

            CurrentAspectIndex = OffValue;

            AspectChanged?.Invoke(null);
            return true;
        }

        /// <summary>
        /// Changes the current aspect to a new one.
        /// </summary>
        /// <param name="newAspect">The index of the new aspect. Negative values turn off the signal.</param>
        /// <returns><see langword="true"/> if the aspect changed, <see langword="false"/> otherwise.</returns>
        /// <remarks>In case the aspect is successfully changed, <see cref="AspectChanged"/> will be called.</remarks>
        public bool ChangeAspect(int newAspect)
        {
            // Check if the state changes. All negative numbers are treated as off.
            if (newAspect == CurrentAspectIndex || !IsOn && newAspect < 0)
            {
                return false;
            }

            // Out of range, ignore request. Maybe make them open the signal (last state)?
            if (newAspect >= AllAspects.Length)
            {
                SignalsMod.Error($"Failed to set state on signal '{Name}': {newAspect} >= {AllAspects.Length}");
                return false;
            }

            // Turn off on negative numbers.
            if (newAspect < 0)
            {
                SignalsMod.LogVerbose($"Turning off signal '{Name}'");
                return TurnOff();
            }

            CurrentAspect?.Unapply();

            SignalsMod.LogVerbose($"Setting signal '{Name}' to state '{AllAspects[newAspect].Definition.Id}'");
            CurrentAspectIndex = newAspect;
            AllAspects[newAspect].Apply();

            AspectChanged?.Invoke(AllAspects[newAspect]);
            return true;
        }

        /// <summary>
        /// Change the current aspect to the most restrictive one.
        /// </summary>
        /// <param name="withOverride">If <see langword="true"/>, also sets the manual override.</param>
        /// <returns><see langword="true"/> if the aspect changed, <see langword="false"/> otherwise.</returns>
        public bool ChangeToMostRestrictive(bool withOverride)
        {
            if (withOverride)
            {
                ManualOverrideAspect = 0;
            }

            var changed = ChangeAspect(0);
            UpdateDisplays(changed);
            UpdateIndicators();

            return changed;
        }

        /// <summary>
        /// Change the current aspect to the least restrictive one.
        /// </summary>
        /// <param name="withOverride">If <see langword="true"/>, also sets the manual override.</param>
        /// <returns><see langword="true"/> if the aspect changed, <see langword="false"/> otherwise.</returns>
        public bool ChangeToLeastRestrictive(bool withOverride)
        {
            if (withOverride)
            {
                ManualOverrideAspect = AllAspects.Length - 1;
            }

            var changed = ChangeAspect(AllAspects.Length - 1);
            UpdateDisplays(changed);
            UpdateIndicators();

            return changed;
        }

        public void UpdateHoverDisplay()
        {
            if (IsOn)
            {
                // Current aspect is not null when the signal is on.
                _hover.UpdateStateDisplay(this, CurrentAspect!.Definition.HUDSprite);
            }
            else
            {
                _hover.UpdateStateDisplay(this, Definition.OffStateHUDSprite);
            }
        }

        public void UpdateDisplays(bool aspectChanged)
        {
            foreach (var item in AllDisplays)
            {
                item.CheckAndUpdate(aspectChanged);
            }

            UpdateHoverDisplay();

            DisplaysUpdated?.Invoke(AllDisplays);
        }

        public void UpdateIndicators()
        {
            if (!IsOn) return;

            // Turn off all indicators first. This needs 2 loops to prevent
            // conflicts where one indicator turns another that was already
            // on, off.
            foreach (var item in AllIndicators)
            {
                item.Unapply();
            }

            foreach (var item in AllIndicators)
            {
                // Turn on the ones that meet conditions.
                if (item.MeetsConditions())
                {
                    item.Apply();
                }
            }
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
            return !Operation.IsFullyManual();
        }

        /// <summary>
        /// Update the current <see cref="Block"/> before the signal is updated.
        /// </summary>
        public virtual void UpdateBlock() { }

        /// <summary>
        /// Updates the current aspect based on the conditions of <see cref="AllAspects"/>.
        /// </summary>
        /// <param name="startPropagate">Whether this signal should propagate its updates to the signals afterwards.</param>
        public void UpdateAspect(bool startPropagate)
        {
            UpdateBlock();

            bool changed;

            for (int i = 0; i < AllAspects.Length; i++)
            {
                if (IsAspectManuallyOverriden(i) || AllAspects[i].MeetsConditions())
                {
                    changed = ChangeAspect(i);

                    if (IsTempOverrideMatched(i))
                    {
                        Operation = SignalOperationMode.Automatic;
                    }

                    goto Finalise;
                }
            }

            // Turn off if no conditions are met.
            changed = TurnOff();

            Finalise:

            // Update displays and indicators.
            UpdateDisplays(changed);
            UpdateIndicators();

            // Request the next signal to be updated to propagate out of range.
            UpdateRequested = Mathf.Max(UpdateRequested - 1, 0);
            GetNextSignal()?.RequestUpdate(startPropagate ? UpdatePropagation : UpdateRequested);
        }

        /// <summary>
        /// Gets the next signal within the <see cref="TrackBlock"/> this signal controls.
        /// </summary>
        /// <returns>The next</returns>
        public BasicSignalController? GetNextSignal()
        {
            var block = Block;

            return (block != null && block.NextSignal != null) ? block.NextSignal : null;
        }

        /// <summary>
        /// Keeps iterating through signals until <paramref name="condition"/> is met.
        /// </summary>
        /// <param name="condition">The condition a signal must meet.</param>
        /// <returns></returns>
        public BasicSignalController? GetNextSignalCondition(Predicate<BasicSignalController> condition)
        {
            var signal = this;
            var visited = new HashSet<BasicSignalController>();
            var safety = 0;

            while (signal != null)
            {
                if (visited.Contains(signal)) return null;

                visited.Add(signal);

                if (condition(signal)) return signal;

                safety++;

                if (safety > TrackWalker.MaxDepth)
                {
                    SignalsMod.Error($"Hit safety searching for next signal meeting condition of signal {Name}");
                    return null;
                }

                signal = signal.GetNextSignal();
            }

            return null;
        }

        public virtual List<TrackBlock> GetPotentialBlocks()
        {
            var blocks = new List<TrackBlock>();

            if (Block != null)
            {
                blocks.Add(Block);
            }

            return blocks;
        }
    }
}
