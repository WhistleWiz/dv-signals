using Signals.Common;
using Signals.Common.States;
using Signals.Game.States;
using System;
using System.Collections.Generic;

namespace Signals.Game
{
    internal static class SignalCreator
    {
        public static Dictionary<string, Func<SignalStateBaseDefinition, SignalStateBase>> CreatorFunctions;

        static SignalCreator()
        {
            CreatorFunctions = new Dictionary<string, Func<SignalStateBaseDefinition, SignalStateBase>>
            {
                { Constants.SignalIds.Open, (x) => new OpenSignalState(x) },
                { Constants.SignalIds.Closed, (x) => new ClosedSignalState(x) },
                { Constants.SignalIds.NextClosed, (x) => new NextSignalClosedSignalState(x) }
            };
        }

        public static SignalStateBase? Create(SignalController controller, SignalStateBaseDefinition def)
        {
            if (CreatorFunctions.TryGetValue(def.Id, out var creator))
            {
                var result = creator(def);
                result.Controller = controller;
                return result;
            }

            return null;
        }
    }
}
