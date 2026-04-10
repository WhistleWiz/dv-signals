using System;
using System.Collections.Generic;
using UnityEngine;

namespace Signals.Common
{
    [CreateAssetMenu(menuName = "DVSignals/Signal Pack")]
    public class SignalPack : ScriptableObject
    {
        public string ModId = string.Empty;
        public string ModName = string.Empty;
        public string Author = string.Empty;
        public string Version = "1.0.0";
        public string HomePage = string.Empty;
        public string Repository = string.Empty;

        [Header("Main Signal")]
        [Tooltip("Used on all junctions in mainlines, facing the joined track")]
        public SignalControllerDefinition Signal = null!;
        [Tooltip("Used on all junctions in mainlines, facing the junction branches")]
        public SignalControllerDefinition? JunctionSignal;

        [Header("Optional Signals")]
        [Tooltip("Used on junctions that enter/change yards")]
        public SignalControllerDefinition? IntoYardSignal;
        [Tooltip("Used on junctions inside yards")]
        public SignalControllerDefinition? ShuntingSignal;
        [Tooltip("Used on passenger track")]
        public SignalControllerDefinition? PassengerSignal;
        [Tooltip("Used before mainline signals warn about their state")]
        public SignalControllerDefinition? DistantSignal;
        [Tooltip("The distance a Distant Signal must be from its corresponding signal"), Min(100.0f)]
        public float DistantSignalDistance = 200.0f;
        [Tooltip("The minimum length of a track to be eligible for a distant signal\nShould be at least double the signal distance"), Min(200.0f)]
        public float DistantSignalMinimumTrackLength = 600.0f;

        [Header("Optional Alternate Versions")]
        [Tooltip("Used on all junctions in mainlines")]
        public SignalControllerDefinition? OldSignal;
        [Tooltip("Used on all junctions in mainlines, facing the junction branches")]
        public SignalControllerDefinition? OldJunctionSignal;
        [Tooltip("Used on junctions that enter/change yards")]
        public SignalControllerDefinition? OldIntoYardSignal;
        [Tooltip("Used on junctions inside yards")]
        public SignalControllerDefinition? OldShuntingSignal;
        [Tooltip("Used on passenger track")]
        public SignalControllerDefinition? OldPassengerSignal;
        [Tooltip("Used on mainline tracks that meet certain conditions")]
        public SignalControllerDefinition? OldDistantSignal;

        [Header("Extras")]
        [Tooltip("Any additional signals included in this pack")]
        public SignalControllerDefinition[] OtherSignals = Array.Empty<SignalControllerDefinition>();

        public bool Validate()
        {
            if (Signal == null)
            {
                Debug.LogError("Controller is not set!", this);
                return false;
            }

            return true;
        }

        public IEnumerable<SignalControllerDefinition> AllSignals
        {
            get
            {
                yield return Signal;

                if (IntoYardSignal != null) yield return IntoYardSignal;
                if (ShuntingSignal != null) yield return ShuntingSignal;
                if (PassengerSignal != null) yield return PassengerSignal;
                if (DistantSignal != null) yield return DistantSignal;

                foreach (var item in OtherSignals)
                {
                    if (item != null) yield return item;
                }

                if (OldSignal != null) yield return OldSignal;
                if (OldIntoYardSignal != null) yield return OldIntoYardSignal;
                if (OldShuntingSignal != null) yield return OldShuntingSignal;
                if (OldPassengerSignal != null) yield return OldPassengerSignal;
                if (OldDistantSignal != null) yield return OldDistantSignal;
            }
        }

        public SignalControllerDefinition GetMainlineSignal(bool old)
        {
            if (old && OldSignal != null) return OldSignal;

            return Signal;
        }

        public SignalControllerDefinition GetJunctionSignal(bool old)
        {
            if (old && OldJunctionSignal != null) return OldJunctionSignal;

            if (JunctionSignal != null) return JunctionSignal;

            return GetMainlineSignal(old);
        }

        public SignalControllerDefinition GetIntoYardSignal(bool old)
        {
            if (old && OldIntoYardSignal != null) return OldIntoYardSignal;

            if (IntoYardSignal != null) return IntoYardSignal;

            return GetJunctionSignal(old);
        }

        public SignalControllerDefinition GetPassengerSignal(bool old)
        {
            if (old && OldPassengerSignal != null) return OldPassengerSignal;

            if (PassengerSignal != null) return PassengerSignal;

            return GetMainlineSignal(old);
        }

        public SignalControllerDefinition? GetShuntingSignal(bool old)
        {
            return old ? OldShuntingSignal : ShuntingSignal;
        }
    }
}
