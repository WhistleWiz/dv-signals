using Signals.Common.Displays;
using Signals.Game.Conditions;
using Signals.Game.Controllers;
using System.Linq;
using UnityEngine;

namespace Signals.Game.Displays
{
    public interface IDisplay : IHudDisplayable
    {
        public bool CheckAndUpdate(bool aspectChanged);

        public void UpdateDisplay();
    }

    public abstract class DisplayBase<T> : IDisplay
        where T : DisplayBaseDefinition
    {
        private bool _conditionsChecked = false;
        private bool _hasUpdated = false;
        private bool _disabled = false;
        private bool _off = false;
        private ICondition[] _conditions;

        public Sprite? SpriteOverride;

        public Signal Signal { get; private set; }
        public T Definition { get; private set; }

        public string DisplayText { get => Definition.DisplayText; set => Definition.DisplayText = value; }
        public bool ShouldDisplay => !_disabled && !_off;
        public int DisplayOrder => Definition.HUDDisplayOrder;
        public Sprite? Sprite => SpriteOverride ?? Definition.HUDSprite;
        public Color TextColour => Definition.HUDTextColour;
        public BasicSignalController Controller => Signal.Controller;

        protected DisplayBase(DisplayBaseDefinition definition, Signal signal)
        {
            Signal = signal;
            Definition = (T)definition;

            if (Definition == null) throw new System.ArgumentException($"Type mismatch between definition and expected {typeof(T)}", nameof(definition));

            _conditions = definition.Conditions.Select(x => ConditionCreator.Create(x)).Where(x => x != null).ToArray()!;
        }

        /// <summary>
        /// Checks and updates the displays.
        /// </summary>
        /// <param name="aspectChanged">If the aspect of the signal changed.</param>
        /// <returns></returns>
        public bool CheckAndUpdate(bool aspectChanged)
        {
            // If it has been disabled by conditions, it's never active or updated.
            if (_disabled) return false;

            if (!_conditionsChecked && _conditions.Any(x => !x.MeetsConditions(Signal)))
            {
                // Remove the child renderers from the signal hover, or they'll be highlighted while invisible.
                Signal.Hover?.ForceRemoveRenderers(Definition.GetComponentsInChildren<Renderer>(true));
                Definition.gameObject.SetActive(false);
                _disabled = true;
                return false;
            }

            _conditionsChecked = true;

            if (Definition.DisableWhenSignalIsOff && Signal.IsOff)
            {
                if (!_off)
                {
                    Definition.gameObject.SetActive(false);
                    _hasUpdated = false;
                    _off = true;
                }

                return false;
            }

            if (_off)
            {
                Definition.gameObject.SetActive(true);
                _off = false;
            }

            if (Definition.Mode == DisplayBaseDefinition.UpdateMode.AtStart && _hasUpdated)
            {
                return false;
            }

            if (Definition.Mode == DisplayBaseDefinition.UpdateMode.AspectChanged && !aspectChanged)
            {
                return false;
            }

            _hasUpdated = true;
            UpdateDisplay();
            return true;
        }

        public abstract void UpdateDisplay();
    }
}
