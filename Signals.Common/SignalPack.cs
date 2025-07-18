﻿using System;
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
        [Tooltip("Used on all junctions in mainlines")]
        public SignalControllerDefinition Signal = null!;
        [Header("Optional Signals")]
        [Tooltip("Used on junctions that enter/change yards")]
        public SignalControllerDefinition? IntoYardSignal;
        [Tooltip("Used on junctions inside yards")]
        public SignalControllerDefinition? ShuntingSignal;
        [Tooltip("Used on mainline tracks that meet certain conditions")]
        public SignalControllerDefinition? DistantSignal;
        [Tooltip("The distance a Distant Signal must be from its corresponding signal"), Min(100.0f)]
        public float DistantSignalDistance = 300.0f;
        [Tooltip("The minimum length of a track to be eligible for a distant signal\nShould be at least double the signal distance"), Min(200.0f)]
        public float DistantSignalMinimumTrackLength = 600.0f;

        [Header("Optional Alternate Versions")]
        [Tooltip("Used on all junctions in mainlines")]
        public SignalControllerDefinition? OldSignal;
        [Tooltip("Used on junctions that enter/change yards")]
        public SignalControllerDefinition? OldIntoYardSignal;
        [Tooltip("Used on junctions inside yards")]
        public SignalControllerDefinition? OldShuntingSignal;
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
                if (DistantSignal != null) yield return DistantSignal;

                foreach (var item in OtherSignals)
                {
                    if (item != null) yield return item;
                }

                if (OldSignal != null) yield return OldSignal;
                if (OldIntoYardSignal != null) yield return OldIntoYardSignal;
                if (OldShuntingSignal != null) yield return OldShuntingSignal;
                if (OldDistantSignal != null) yield return OldDistantSignal;
            }
        }
    }
}
