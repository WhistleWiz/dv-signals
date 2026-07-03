using Signals.Common;
using Signals.Game.Aspects;
using Signals.Game.Controllers;
using Signals.Game.Displays;
using Signals.Game.Lights;
using Signals.Game.Misc;
using Signals.Game.Railway;
using Signals.Game.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Signals.Game
{
    public class Signal : IHudDisplayable
    {
        private class IndicatorHudWrapper : IHudDisplayable
        {
            public int DisplayOrder { get; private set; }
            public Sprite? Sprite { get; set; }

            public bool ShouldDisplay => Sprite != null;
            public string DisplayText => Space;
            public Color TextColour => Color.white;

            public IndicatorHudWrapper(IAspect indicator, Sprite? off)
            {
                Sprite = off;
                DisplayOrder = indicator.DisplayOrder;
            }
        }

        #region Static

        private const string Space = " ";

        private static int s_idGen = 0;
        private static object s_lock = new object();
        private static Renderer[] s_emptyRenderers = Array.Empty<Renderer>();

        // Get a unique ID for the signal.
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

        protected const int OffValue = -1;
        protected static readonly int AnimatorHash = Animator.StringToHash("Aspect");

        #endregion

        #region Members

        private SignalDefinitionToInstance? _comp;
        private SignalOperationMode _operation = SignalOperationMode.Automatic;
        private int _manualOverride = 0;
        private bool _shuntingAllowed = false;
        private string? _internalName;
        private IndicatorHudWrapper[] _indicatorWrapper;

        internal SignalHover? Hover;

        public readonly int Id;
        /// <summary>
        /// Override the name of this signal.
        /// </summary>
        public string NameOverride = string.Empty;
        public bool AllowReserving = true;

        #endregion

        #region Properties

        // Used for comms radio highlighting.
        internal Renderer[] HighlightRenderers => Hover != null ? ReflectionHelpers.GetRenderers(Hover) : s_emptyRenderers;

        public BasicSignalController Controller { get; private set; }
        public SignalDefinition Definition { get; private set; }
        public SignalLight[] AllLights { get; private set; }
        public IAspect[] AllAspects { get; private set; }
        public IDisplay[] AllDisplays { get; private set; }
        public IAspect[] AllIndicators { get; private set; }
        /// <summary>
        /// The block of tracks this signal works with.
        /// </summary>
        public TrackBlock? Block { get; private set; }
        public Signal? Parent { get; set; }
        public Signal? DistantSignal { get; private set; }
        public int CurrentAspectIndex { get; private set; }
        public bool IsShunting { get; private set; }

        // Getters only.
        public IAspect? CurrentAspect => IsOn ? AllAspects[CurrentAspectIndex] : null;
        public SignalOperationMode Operation => _operation;
        public bool Hovered => Hover != null && ReflectionHelpers.IsHovered(Hover);
        public bool IsOff => CurrentAspectIndex < 0;
        public bool IsOn => CurrentAspectIndex >= 0;
        public bool ShuntingAllowed => _shuntingAllowed;
        public int ManualOverrideAspect => _manualOverride;
        public string InternalName => _internalName ??= GetInternalName();
        public string Name => string.IsNullOrEmpty(NameOverride) ? InternalName : NameOverride;

        // IHudDisplayable implementation.
        public bool ShouldDisplay => true;
        public int DisplayOrder => Definition.HUDDisplayOrder;
        public string DisplayText => Space;
        public Sprite? Sprite => Definition.OffStateHUDSprite;
        public Color TextColour => Color.white;

        #endregion

        #region Events

        public Action<IAspect?>? AspectChanged;
        public Action<IDisplay[]>? DisplaysUpdated;
        public Action<SignalOperationMode>? OperationModeChanged;
        public Action<int>? OverrideChanged;
        public Action<bool>? ShuntingAllowedChanged;

        #endregion

        public Signal(BasicSignalController controller, SignalDefinition def, bool isShunting)
        {
            Id = GetGenId();
            Controller = controller;
            Definition = def;
            CurrentAspectIndex = OffValue;

            // Instantiate all aspect implementations.
            AllAspects = def.Aspects.Select(x => AspectCreator.Create(this, x)).Where(x => x != null).ToArray()!;
            // Same but for displays.
            AllDisplays = def.Displays.Select(x => DisplayCreator.Create(this, x)).Where(x => x != null).ToArray()!;
            // And finally the same for indicators.
            AllIndicators = def.Indicators.Select(x => AspectCreator.Create(this, x)).Where(x => x != null).ToArray()!;
            // Get an array of all lights after they were created by the aspects.
            AllLights = def.GetComponentsInChildren<SignalLight>(true);

            if (def.DistantSignal != null)
            {
                DistantSignal = new Signal(controller, def.DistantSignal, isShunting) { Parent = this };
            }

            _comp = SignalDefinitionToInstance.AddToDef(this);

            SignalManager.Instance.RegisterSignal(this);

            if (Definition.gameObject.TryGetComponent(out Hover))
            {
                Hover!.Initialise(Definition.OffStateHUDSprite);
            }

            if (Definition.PhysicalReserver != null)
            {
                SignalReservingObject.Create(Definition.PhysicalReserver, this);
            }

            IsShunting = isShunting;

            _indicatorWrapper = new IndicatorHudWrapper[AllIndicators.Length];

            for (int i = 0; i < _indicatorWrapper.Length; i++)
            {
                _indicatorWrapper[i] = new IndicatorHudWrapper(AllIndicators[i], def.InactiveSprites[i]);
            }
        }

        private string GetInternalName()
        {
            if (Parent != null)
            {
                return string.Format(SignalManager.CurrentPack.SubDistantFormat, Parent.Name);
            }

            var name = Controller.InternalName;
            var array = IsShunting ? Controller.ShuntingSignals : Controller.Signals;

            if (array.Length > 1)
            {
                var index = Array.IndexOf(array, this);

                if (SignalManager.CurrentPack.UseLettersForMultipleSignalsInControllers)
                {
                    name = $"{name}-{Helpers.IntToLetters(index)}";
                }
                else
                {
                    name = $"{name}-{index}";
                }
            }

            return name;
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

        public void Destroy()
        {
            DistantSignal?.Destroy();
            SignalManager.Instance.UnregisterSignal(this);

            if (Definition != null)
            {
                GameObject.Destroy(Definition.gameObject);
            }
        }

        public void DestroyDistant()
        {
            if (DistantSignal == null) return;

            Hover?.ForceRemoveRenderers(DistantSignal.Definition.GetComponentsInChildren<Renderer>(true));
            DistantSignal.Destroy();
            DistantSignal = null;
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

            CurrentAspectIndex = OffValue;
            Definition.Animator?.SetInteger(AnimatorHash, OffValue);

            AspectChanged?.Invoke(null);
            return true;
        }

        public void SetBlock(TrackBlock block)
        {
            Block = block;
            DistantSignal?.SetBlock(block);
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
            if (newAspect == CurrentAspectIndex || (IsOff && newAspect < 0))
            {
                return false;
            }

            // Out of range, ignore request. Maybe make them open the signal (least restrictive state)?
            if (newAspect >= AllAspects.Length)
            {
                SignalsMod.Error($"Failed to set aspect of signal '{Name}': {newAspect} >= {AllAspects.Length}");
                return false;
            }

            // Turn off on negative numbers.
            if (newAspect < 0)
            {
                SignalsMod.LogVerbose($"Turning off signal '{Name}'");
                return TurnOff();
            }

            CurrentAspect?.Unapply();

            var aspect = AllAspects[newAspect];
            SignalsMod.LogVerbose($"Setting signal '{Name}' to aspect '{aspect.Id}'");
            aspect.Apply();
            CurrentAspectIndex = newAspect;
            Definition.Animator?.SetInteger(AnimatorHash, newAspect);
            AspectChanged?.Invoke(aspect);
            Controller.AnyAspectChanged?.Invoke(this, aspect);
            return true;
        }

        public void UpdateAspect(bool forced)
        {
            bool changed = false;

            if (Operation.IsFullyManual())
            {
                goto Finalise;
            }

            for (int i = 0; i < AllAspects.Length; i++)
            {
                var aspect = AllAspects[i];

                if (IsAspectManuallyOverriden(i) || aspect.MeetsConditions())
                {
                    changed = ChangeAspect(i);

                    if (IsTempOverrideMatched(i))
                    {
                        ChangeOperationMode(SignalOperationMode.Automatic);
                    }

                    goto Finalise;
                }
            }

            // Turn off if no conditions are met.
            changed = TurnOff();

        Finalise:

            DistantSignal?.UpdateAspect(forced);

            // Update displays and indicators.
            UpdateDisplays(changed || forced);
            UpdateIndicators();
            UpdateHoverDisplay();
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
            if (IsOff) return;

            // Turn off all indicators first. This needs 2 loops to prevent
            // conflicts where one indicator turns another that was already
            // on, off.
            foreach (var item in AllIndicators)
            {
                item.Unapply();
            }

            for (int i = 0; i < AllIndicators.Length; i++)
            {
                var indicator = AllIndicators[i];

                // Turn on the ones that meet conditions.
                if (indicator.MeetsConditions())
                {
                    indicator.Apply();
                    _indicatorWrapper[i].Sprite = indicator.Sprite;
                }
                else
                {
                    _indicatorWrapper[i].Sprite = Definition.InactiveSprites[i];
                }
            }
        }

        public void UpdateHoverDisplay()
        {
            Hover?.UpdateStateDisplay(this);
        }

        public bool SetShuntingStatus(bool allowed)
        {
            if (ShuntingAllowed == allowed) return false;

            _shuntingAllowed = allowed;
            ShuntingAllowedChanged?.Invoke(allowed);
            return true;
        }

        public bool SetAspectOverride(int index)
        {
            if (ManualOverrideAspect == index)
            {
                return false;
            }

            _manualOverride = index;
            OverrideChanged?.Invoke(index);
            return true;
        }

        public bool ChangeOperationMode(SignalOperationMode mode)
        {
            if (_operation == mode)
            {
                return false;
            }

            _operation = mode;
            OperationModeChanged?.Invoke(mode);
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
                SetAspectOverride(0);
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
                SetAspectOverride(AllAspects.Length - 1);
            }

            var changed = ChangeAspect(AllAspects.Length - 1);
            UpdateDisplays(changed);
            UpdateIndicators();

            return changed;
        }

        /// <summary>
        /// Gets the controller at the end of the <see cref="TrackBlock"/> this signal controls.
        /// </summary>
        /// <returns>The next</returns>
        public BasicSignalController? GetNextController()
        {
            var block = Block;

            return (block != null && block.NextController != null) ? block.NextController : null;
        }

        /// <summary>
        /// Gets the first controller found that meets the condition.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public BasicSignalController? GetNextControllerCondition(Predicate<BasicSignalController> condition)
        {
            var controller = GetNextController();
            var visited = new HashSet<BasicSignalController> { Controller };
            var safety = 0;


            while (controller != null)
            {
                if (visited.Contains(controller)) return null;

                visited.Add(controller);

                if (condition(controller)) return controller;

                safety++;

                if (safety > TrackWalker.MaxDepth)
                {
                    SignalsMod.Error($"Hit safety searching for next controller meeting condition for controller {Name}");
                    return null;
                }

                controller = controller.GetNextController();
            }

            return null;
        }

        public IEnumerable<IHudDisplayable> GetAllHudElements()
        {
            var aspect = CurrentAspect;
            yield return aspect ?? (IHudDisplayable)this;

            foreach (var item in AllDisplays)
            {
                yield return item;
            }

            foreach (var item in _indicatorWrapper)
            {
                yield return item;
            }

            if (DistantSignal != null)
            {
                foreach (var item in DistantSignal.GetAllHudElements())
                {
                    yield return item;
                }
            }
        }
    }
}
