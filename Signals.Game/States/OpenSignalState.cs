﻿using Signals.Common.States;

namespace Signals.Game.States
{
    internal class OpenSignalState : SignalStateBase
    {
        public OpenSignalState(SignalStateBaseDefinition def) : base(def) { }

        public override bool MeetsConditions(RailTrack[] tracksToNextSignal, SignalController? nextSignal)
        {
            return true;
        }
    }
}
