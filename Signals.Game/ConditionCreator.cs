using Signals.Common.Conditions;
using Signals.Game.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game
{
    /// <summary>
    /// Class to instantiate the implementation of a <see cref="ConditionBaseDefinition"/>.
    /// </summary>
    public static class ConditionCreator
    {
        private static Type[] s_defaultTypes;
        private static HashSet<Type> s_failedConditions = new HashSet<Type>();

        internal static Dictionary<Type, Func<ConditionBaseDefinition, ICondition>> CreatorFunctions;

        static ConditionCreator()
        {
            CreatorFunctions = new Dictionary<Type, Func<ConditionBaseDefinition, ICondition>>();

            Add((x) => new ActsAsDistantCondition(x));
            Add((x) => new ShortDistanceCondition(x));
            Add((x) => new WrongSideOfTrackCondition(x));

            s_defaultTypes = CreatorFunctions.Keys.ToArray();

            static void Add<T>(Func<ConditionBaseDefinition, ConditionBase<T>> func)
                where T : ConditionBaseDefinition
            {
                CreatorFunctions.Add(typeof(T), func);
            }
        }

        internal static ICondition? Create(ConditionBaseDefinition? def)
        {
            if (def == null) return null;

            var t = def.GetType();

            if (CreatorFunctions.TryGetValue(t, out var creator))
            {
                var result = creator(def);
                return result;
            }

            // Otherwise every single signal would output the same error...
            if (!s_failedConditions.Contains(t))
            {
                SignalsMod.Error($"Failed to find creator function for condition '{t.FullName}'");
                s_failedConditions.Add(t);
            }

            return null;
        }

        /// <summary>
        /// Add your own condition creators for custom conditions.
        /// </summary>
        /// <param name="func">
        /// The method that turns the definition into an implementation.
        /// <para>Inputs is the definition.</para>
        /// </param>
        /// <returns><see langword="true"/> if the type was sucessfully added, otherwise <see langword="false"/>.</returns>
        public static bool AddCreatorFunction<T>(Func<ConditionBaseDefinition, ConditionBase<T>> func)
            where T : ConditionBaseDefinition
        {
            var t = typeof(T);

            if (CreatorFunctions.ContainsKey(t))
            {
                SignalsMod.Error($"Could not add type '{t.FullName}' to condition creators, type already exists!");
                return false;
            }

            s_failedConditions.Clear();
            CreatorFunctions.Add(t, func);
            return true;
        }

        /// <summary>
        /// Remove your custom condition creators.
        /// </summary>
        /// <returns><see langword="true"/> if the type was sucessfully removed, otherwise <see langword="false"/>.</returns>
        /// <remarks>This method will not remove the default types.</remarks>
        public static bool RemoveCreatorFunction<T>()
            where T : ConditionBaseDefinition
        {
            var t = typeof(T);

            if (s_defaultTypes.Contains(t))
            {
                SignalsMod.Warning("Attempt to remove default signal aspect stopped.");
                return false;
            }

            s_failedConditions.Clear();
            return CreatorFunctions.Remove(t);
        }
    }
}
