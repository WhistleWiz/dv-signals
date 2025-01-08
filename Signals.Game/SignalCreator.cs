using Signals.Common.States;
using Signals.Game.States;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game
{
    public static class SignalCreator
    {
        private static Type[] s_defaultTypes;
        private static HashSet<Type> s_failedStates = new HashSet<Type>();

        internal static Dictionary<Type, Func<SignalStateBaseDefinition, SignalStateBase>> CreatorFunctions;

        static SignalCreator()
        {
            CreatorFunctions = new Dictionary<Type, Func<SignalStateBaseDefinition, SignalStateBase>>
            {
                { typeof(OpenSignalStateDefinition), (x) => new OpenSignalState(x) },
                { typeof(ClosedSignalStateDefinition), (x) => new ClosedSignalState(x) },
                { typeof(IsNextClosedSignalStateDefinition), (x) => new IsNextClosedSignalState(x) },
                { typeof(IsNextStateSignalStateDefinition), (x) => new IsNextStateSignalState(x) }
            };

            s_defaultTypes = CreatorFunctions.Keys.ToArray();
        }

        internal static SignalStateBase? Create(SignalController controller, SignalStateBaseDefinition def)
        {
            var t = def.GetType();

            if (CreatorFunctions.TryGetValue(t, out var creator))
            {
                var result = creator(def);
                result.Controller = controller;
                return result;
            }

            // Otherwise every single signal would output the same error...
            if (!s_failedStates.Contains(t))
            {
                SignalsMod.Error($"Failed to find creator function for state '{t.FullName}'");
                s_failedStates.Add(t);
            }

            return null;
        }

        /// <summary>
        /// Add your own state creators for custom signal states.
        /// </summary>
        /// <param name="func">The method that turns the definition into an implementation.</param>
        /// <returns><see langword="true"/> if the type was sucessfully added, otherwise <see langword="false"/>.</returns>
        public static bool AddCreatorFunction<T>(Func<SignalStateBaseDefinition, SignalStateBase> func)
            where T : SignalStateBaseDefinition
        {
            var t = typeof(T);

            if (CreatorFunctions.ContainsKey(t))
            {
                SignalsMod.Error($"Could not add type '{t.FullName}' to signal state creators, type already exists!");
                return false;
            }

            s_failedStates.Clear();
            CreatorFunctions.Add(t, func);
            return true;
        }

        /// <summary>
        /// Remove your custom state creators.
        /// </summary>
        /// <param name="id">The ID to remove.</param>
        /// <returns><see langword="true"/> if the ID was sucessfully removed, otherwise <see langword="false"/>.</returns>
        /// <remarks>This method will not remove the default IDs.</remarks>
        public static bool RemoveCreatorFunction<T>()
            where T : SignalStateBaseDefinition
        {
            var t = typeof(T);

            if (s_defaultTypes.Contains(t))
            {
                SignalsMod.Warning("Attempt to remove default signal type stopped.");
                return false;
            }

            s_failedStates.Clear();
            return CreatorFunctions.Remove(t);
        }
    }
}
