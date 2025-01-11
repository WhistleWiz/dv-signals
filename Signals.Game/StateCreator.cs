using Signals.Common.States;
using Signals.Game.States;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game
{
    public static class StateCreator
    {
        private static Type[] s_defaultTypes;
        private static HashSet<Type> s_failedStates = new HashSet<Type>();

        internal static Dictionary<Type, Func<SignalStateBaseDefinition, SignalController, SignalStateBase>> CreatorFunctions;

        static StateCreator()
        {
            CreatorFunctions = new Dictionary<Type, Func<SignalStateBaseDefinition, SignalController, SignalStateBase>>
            {
                { typeof(OpenSignalStateDefinition), (x, y) => new OpenSignalState(x, y) },
                { typeof(ClosedSignalStateDefinition), (x, y) => new ClosedSignalState(x, y) },
                { typeof(IsNextClosedSignalStateDefinition), (x, y) => new IsNextClosedSignalState(x, y) },
                { typeof(IsNextStateSignalStateDefinition), (x, y) => new IsNextStateSignalState(x, y) }
            };

            s_defaultTypes = CreatorFunctions.Keys.ToArray();
        }

        internal static SignalStateBase? Create(SignalController controller, SignalStateBaseDefinition? def)
        {
            if (def == null) return null;

            var t = def.GetType();

            if (CreatorFunctions.TryGetValue(t, out var creator))
            {
                var result = creator(def, controller);
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
        /// <param name="func">
        /// The method that turns the definition into an implementation.
        /// <para>Inputs are the definition and the controller.</para>
        /// </param>
        /// <returns><see langword="true"/> if the type was sucessfully added, otherwise <see langword="false"/>.</returns>
        public static bool AddCreatorFunction<T>(Func<SignalStateBaseDefinition, SignalController, SignalStateBase> func)
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
