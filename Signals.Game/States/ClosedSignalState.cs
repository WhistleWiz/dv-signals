﻿using Signals.Common.States;
using System.Linq;

namespace Signals.Game.States
{
    internal class ClosedSignalState : SignalStateBase
    {
        public ClosedSignalState(SignalStateBaseDefinition def) : base(def) { }

        public override bool MeetsConditions()
        {
            foreach (var item in TrackWalker.WalkUntilNextSignal(Controller))
            {
                if (item.onTrackBogies.Count() > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}