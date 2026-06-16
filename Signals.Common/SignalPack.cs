using System;
using System.Collections.Generic;
using UnityEngine;

namespace Signals.Common
{
    [CreateAssetMenu(menuName = "DV Signals/Signal Pack")]
    public class SignalPack : ScriptableObject
    {
        public string ModId = string.Empty;
        public string ModName = string.Empty;
        public string Author = string.Empty;
        public string Version = "1.0.0";
        public string HomePage = string.Empty;
        public string Repository = string.Empty;

        [Header("Required Signals")]
        [Tooltip("Used on all junctions in mainlines, on the through track towards the junction\n" +
            "It's also used as the fallback for all other signals if they're missing, unless specified")]
        public SignalControllerDefinition Signal = null!;
        [Tooltip("Used on junctions inside yards")]
        public SignalControllerDefinition ShuntingSignal = null!;

        [Header("Optional Signals")]
        [Tooltip("Used on all junctions on the diverging track in mainlines, facing towards the junction")]
        public SignalControllerDefinition? DivergingSignal;
        [Tooltip("Used on all junctions with a track diverging to the left in mainlines, facing the junction branches")]
        public SignalControllerDefinition? LeftJunctionSignal;
        [Tooltip("Used on all junctions with a track diverging to the right in mainlines, facing the junction branches")]
        public SignalControllerDefinition? RightJunctionSignal;
        [Tooltip("Used when entering stations")]
        public SignalControllerDefinition? EntrySignal;
        [Tooltip("Used when leaving yards")]
        public SignalControllerDefinition? ExitSignal;
        [Tooltip("Used when leaving passenger tracks\n" +
            "Falls back to exit signals")]
        public SignalControllerDefinition? ExitPassengerSignal;
        [Tooltip("Used on mainline station tracks\n" +
            "Falls back to exit signals")]
        public SignalControllerDefinition? ExitMainlineSignal;
        [Tooltip("Used on very long station tracks\n" +
            "Does not fall back if missing")]
        public SignalControllerDefinition? SpacingSignal;
        [Tooltip("Used on turntables\n" +
            "Does not fall back if missing")]
        public SignalControllerDefinition? TurntableSignal;

        [Header("Optional Distant Signals")]
        [Tooltip("Used to warn about the state of mainline signals")]
        public SignalControllerDefinition? DistantSignal;
        [Tooltip("The distance a distant signal must be from its corresponding signal"), Min(300.0f)]
        public float DistantSignalDistance = 400.0f;
        [Tooltip("The minimum length of a track to be eligible for a distant signal"), Min(100.0f)]
        public float DistantSignalMinimumDistance = 200.0f;
        [Tooltip("The tolerance in case a regular signal has distant signal function"), Min(50.0f)]
        public float DistantTolerance = 200.0f;
        [Tooltip("Used to warn about the state of mainline signals where there is low visibility")]
        public SignalControllerDefinition? RepeaterSignal;
        [Tooltip("The distance a repeater signal must be from its corresponding signal"), Min(50.0f)]
        public float RepeaterSignalDistance = 100.0f;
        [Tooltip("The minimum length of a track to be eligible for a repeater signal"), Min(100.0f)]
        public float RepeaterSignalMinimumDistance = 200.0f;

        [Header("Optional Combined Signals")]
        [Tooltip("Used on all junctions in mainlines where the main signal should also act as a distant signal, facing the joined track")]
        public SignalControllerDefinition? CombinedSignal;
        [Tooltip("Used on all junctions with a track diverging to the left in mainlines where the main signal should also act as a distant signal, " +
            "facing the junction branches")]
        public SignalControllerDefinition? CombinedLeftJunctionSignal;
        [Tooltip("Used on all junctions with a track diverging to the right in mainlines where the main signal should also act as a distant signal, " +
            "facing the junction branches")]
        public SignalControllerDefinition? CombinedRightJunctionSignal;

        [Header("Optional Old Versions")]
        [Tooltip("If false, the normal signals will be used in all spots, skipping the need to duplicate optional prefabs")]
        public bool EnableOldVersions = false;
        [Tooltip("Used on all junctions in mainlines")]
        public SignalControllerDefinition? OldSignal;
        [Tooltip("Used on junctions inside yards")]
        public SignalControllerDefinition? OldShuntingSignal;

        [Space]
        [Tooltip("Used on all junctions on the diverging track in mainlines, facing towards the junction")]
        public SignalControllerDefinition? OldDivergingSignal;
        [Tooltip("Used on all junctions with a track diverging to the left in mainlines, facing the junction branches")]
        public SignalControllerDefinition? OldLeftJunctionSignal;
        [Tooltip("Used on all junctions with a track diverging to the right in mainlines, facing the junction branches")]
        public SignalControllerDefinition? OldRightJunctionSignal;
        [Tooltip("Used when entering stations")]
        public SignalControllerDefinition? OldEntrySignal;
        [Tooltip("Used when leaving yards")]
        public SignalControllerDefinition? OldExitSignal;
        [Tooltip("Used when leaving passenger tracks\n" +
            "Falls back to exit signals")]
        public SignalControllerDefinition? OldExitPassengerSignal;
        [Tooltip("Used on mainline station tracks\n" +
            "Falls back to exit signals")]
        public SignalControllerDefinition? OldExitMainlineSignal;
        [Tooltip("Used on very long station tracks\n" +
            "Does not fall back if missing")]
        public SignalControllerDefinition? OldSpacingSignal;
        [Tooltip("Used on turntables\n" +
            "Does not fall back if missing")]
        public SignalControllerDefinition? OldTurntableSignal;

        [Space]
        [Tooltip("Used to warn about the state of mainline signals")]
        public SignalControllerDefinition? OldDistantSignal;
        [Tooltip("Used to warn about the state of mainline signals where there is low visibility")]
        public SignalControllerDefinition? OldRepeaterSignal;

        [Space]
        [Tooltip("Used on all junctions in mainlines where the main signal should also act as a distant signal, facing the joined track")]
        public SignalControllerDefinition? OldCombinedSignal;
        [Tooltip("Used on all junctions with a track diverging to the left in mainlines where the main signal should also act as a distant signal, " +
            "facing the junction branches")]
        public SignalControllerDefinition? OldCombinedLeftJunctionSignal;
        [Tooltip("Used on all junctions with a track diverging to the right in mainlines where the main signal should also act as a distant signal, " +
            "facing the junction branches")]
        public SignalControllerDefinition? OldCombinedRightJunctionSignal;

        [Header("Extras")]
        [Tooltip("Any additional signals included in this pack\n" +
            "These must use custom placement code")]
        public SignalControllerDefinition[] OtherSignals = Array.Empty<SignalControllerDefinition>();

        [Header("Naming Conventions")]
        public bool UseLettersForMultipleSignalsInControllers = false;
        public string EntryFormat = "{3}";
        public string ExitFormat = "{3} {4}";
        public string GenericStationFormat = "{1} {3}";
        public string TrackFormat = "{1} {3}";
        public string MainlineFormat = "{0}";
        public string DistantFormat = "Ps {0}";
        public string RepeaterFormat = "Pp {0}";
        public string SubDistantFormat = "{0}";
        public string FallbackFormat = "S{0}";

        public IEnumerable<SignalControllerDefinition> AllControllers
        {
            get
            {
                // Main.
                yield return Signal;
                yield return ShuntingSignal;

                // Optional.
                if (DivergingSignal != null) yield return DivergingSignal;
                if (LeftJunctionSignal != null) yield return LeftJunctionSignal;
                if (RightJunctionSignal != null) yield return RightJunctionSignal;
                if (EntrySignal != null) yield return EntrySignal;
                if (ExitSignal != null) yield return ExitSignal;
                if (ExitPassengerSignal != null) yield return ExitPassengerSignal;
                if (ExitMainlineSignal != null) yield return ExitMainlineSignal;
                if (SpacingSignal != null) yield return SpacingSignal;
                if (TurntableSignal != null) yield return TurntableSignal;

                // Distant.
                if (DistantSignal != null) yield return DistantSignal;
                if (RepeaterSignal != null) yield return RepeaterSignal;

                // Combined.
                if (CombinedSignal != null) yield return CombinedSignal;
                if (CombinedLeftJunctionSignal != null) yield return CombinedLeftJunctionSignal;
                if (CombinedRightJunctionSignal != null) yield return CombinedRightJunctionSignal;

                // Old main.
                if (OldSignal != null) yield return OldSignal;
                if (OldShuntingSignal != null) yield return OldShuntingSignal;
                // Old optional.
                if (OldDivergingSignal != null) yield return OldDivergingSignal;
                if (OldLeftJunctionSignal != null) yield return OldLeftJunctionSignal;
                if (OldRightJunctionSignal != null) yield return OldRightJunctionSignal;
                if (OldEntrySignal != null) yield return OldEntrySignal;
                if (OldExitSignal != null) yield return OldExitSignal;
                if (OldExitPassengerSignal != null) yield return OldExitPassengerSignal;
                if (OldExitMainlineSignal != null) yield return OldExitMainlineSignal;
                if (OldSpacingSignal != null) yield return OldSpacingSignal;
                if (OldTurntableSignal != null) yield return OldTurntableSignal;
                // Old distant.
                if (OldDistantSignal != null) yield return OldDistantSignal;
                if (OldRepeaterSignal != null) yield return OldRepeaterSignal;
                // Old combined.
                if (OldCombinedSignal != null) yield return OldCombinedSignal;
                if (OldCombinedLeftJunctionSignal != null) yield return OldCombinedLeftJunctionSignal;
                if (OldCombinedRightJunctionSignal != null) yield return OldCombinedRightJunctionSignal;

                foreach (var item in OtherSignals)
                {
                    if (item != null) yield return item;
                }
            }
        }

        private bool OldAndEnabled(bool old) => EnableOldVersions && old;

        public SignalControllerDefinition GetMainlineSignal(bool old)
        {
            if (OldAndEnabled(old) && OldSignal != null) return OldSignal;

            return Signal;
        }

        public SignalControllerDefinition GetDivergingSignal(bool old)
        {
            if (OldAndEnabled(old) && OldDivergingSignal != null) return OldDivergingSignal;

            if (DivergingSignal != null) return DivergingSignal;

            return GetMainlineSignal(old);
        }

        public SignalControllerDefinition GetLeftJunctionSignal(bool old)
        {
            if (OldAndEnabled(old) && OldLeftJunctionSignal != null) return OldLeftJunctionSignal;

            if (LeftJunctionSignal != null) return LeftJunctionSignal;

            return GetMainlineSignal(old);
        }

        public SignalControllerDefinition GetRightJunctionSignal(bool old)
        {
            if (OldAndEnabled(old) && OldRightJunctionSignal != null) return OldRightJunctionSignal;

            if (RightJunctionSignal != null) return RightJunctionSignal;

            return GetMainlineSignal(old);
        }

        public SignalControllerDefinition GetJunctionSignal(bool old, bool left)
        {
            return left ? GetLeftJunctionSignal(old) : GetRightJunctionSignal(old);
        }

        public SignalControllerDefinition GetEntrySignal(bool old)
        {
            if (OldAndEnabled(old) && OldEntrySignal != null) return OldEntrySignal;

            if (EntrySignal != null) return EntrySignal;

            return GetMainlineSignal(old);
        }

        public SignalControllerDefinition GetExitSignal(bool old)
        {
            if (OldAndEnabled(old) && OldExitSignal != null) return OldExitSignal;

            if (ExitSignal != null) return ExitSignal;

            return GetMainlineSignal(old);
        }

        public SignalControllerDefinition GetExitPassengerSignal(bool old)
        {
            if (OldAndEnabled(old) && OldExitPassengerSignal != null) return OldExitPassengerSignal;

            if (ExitPassengerSignal != null) return ExitPassengerSignal;

            return GetExitSignal(old);
        }

        public SignalControllerDefinition GetExitMainlineSignal(bool old)
        {
            if (OldAndEnabled(old) && OldExitMainlineSignal != null) return OldExitMainlineSignal;

            if (ExitMainlineSignal != null) return ExitMainlineSignal;

            return GetExitSignal(old);
        }

        public SignalControllerDefinition GetShuntingSignal(bool old)
        {
            if (OldAndEnabled(old) && OldShuntingSignal != null) return OldShuntingSignal;

            return ShuntingSignal;
        }

        public SignalControllerDefinition? GetDistantSignal(bool old)
        {
            return OldAndEnabled(old) ? OldDistantSignal : DistantSignal;
        }

        public SignalControllerDefinition? GetRepeaterSignal(bool old)
        {
            return OldAndEnabled(old) ? OldRepeaterSignal : RepeaterSignal;
        }

        public SignalControllerDefinition? GetSpacingSignal(bool old)
        {
            return OldAndEnabled(old) ? OldSpacingSignal : SpacingSignal;
        }

        public SignalControllerDefinition? GetTurntableSignal(bool old)
        {
            return OldAndEnabled(old) ? OldTurntableSignal : TurntableSignal;
        }

        public bool HasAnyDistantSignal => DistantSignal != null || OldDistantSignal != null;
        public bool HasAnyRepeaterSignal => RepeaterSignal != null || OldRepeaterSignal != null;
        public bool HasAnySpacingSignal => SpacingSignal != null || OldSpacingSignal != null;
        public bool HasAnyTurntableSignal => TurntableSignal != null || OldTurntableSignal != null;
    }
}
