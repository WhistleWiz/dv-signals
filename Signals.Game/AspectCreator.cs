using Signals.Common.Aspects;
using Signals.Game.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game
{
    /// <summary>
    /// Class to instantiate the implementation of a signal <see cref="AspectBaseDefinition"/>.
    /// </summary>
    public static class AspectCreator
    {
        private static Type[] s_defaultTypes;
        private static HashSet<Type> s_failedAspects = new HashSet<Type>();

        internal static Dictionary<Type, Func<AspectBaseDefinition, Signal, IAspect>> CreatorFunctions;

        static AspectCreator()
        {
            CreatorFunctions = new Dictionary<Type, Func<AspectBaseDefinition, Signal, IAspect>>();

            Add((x, y) => new AlwaysActiveAspect(x, y));

            Add((x, y) => new TrainDetectedAspect(x, y));
            Add((x, y) => new TrackReservedAspect(x, y));

            Add((x, y) => new IsNextAspectAspect(x, y));
            Add((x, y) => new IsNextAspectAnyAspect(x, y));
            Add((x, y) => new IsParentAspectAspect(x, y));
            Add((x, y) => new IsSelfAspectAnyAspect(x, y));
            Add((x, y) => new IsDeadEndAspect(x, y));

            Add((x, y) => new JunctionBranchAspect(x, y));
            Add((x, y) => new MatchingBranchAspect(x, y));
            Add((x, y) => new RequiredBranchAspect(x, y));
            Add((x, y) => new JunctionPathAspect(x, y));
            Add((x, y) => new MatchingPathAspect(x, y));
            Add((x, y) => new RequiredPathAspect(x, y));

            Add((x, y) => new MaxSpeedAspect(x, y));
            Add((x, y) => new CombinationAspect(x, y));
            Add((x, y) => new TurntableConnectedAspect(x, y));

            s_defaultTypes = CreatorFunctions.Keys.ToArray();

            static void Add<T>(Func<AspectBaseDefinition, Signal, AspectBase<T>> func)
                where T : AspectBaseDefinition
            {
                CreatorFunctions.Add(typeof(T), func);
            }
        }

        internal static IAspect? Create<T>(Signal signal, T? def)
            where T : AspectBaseDefinition
        {
            if (def == null) return null;

            var t = def.GetType();

            if (CreatorFunctions.TryGetValue(t, out var creator))
            {
                var result = creator(def, signal);
                return result;
            }

            // Otherwise every single signal would output the same error...
            if (!s_failedAspects.Contains(t))
            {
                SignalsMod.Error($"Failed to find creator function for aspect '{t.FullName}'");
                s_failedAspects.Add(t);
            }

            return null;
        }

        /// <summary>
        /// Add your own aspect creators for custom signal aspects.
        /// </summary>
        /// <param name="func">
        /// The method that turns the definition into an implementation.
        /// <para>Inputs are the definition and the controller.</para>
        /// </param>
        /// <returns><see langword="true"/> if the type was sucessfully added, otherwise <see langword="false"/>.</returns>
        public static bool AddCreatorFunction<T>(Func<AspectBaseDefinition, Signal, AspectBase<T>> func)
            where T : AspectBaseDefinition
        {
            var t = typeof(T);

            if (CreatorFunctions.ContainsKey(t))
            {
                SignalsMod.Error($"Could not add type '{t.FullName}' to signal aspect creators, type already exists!");
                return false;
            }

            s_failedAspects.Clear();
            CreatorFunctions.Add(t, func);
            return true;
        }

        /// <summary>
        /// Remove your custom aspect creators.
        /// </summary>
        /// <returns><see langword="true"/> if the type was sucessfully removed, otherwise <see langword="false"/>.</returns>
        /// <remarks>This method will not remove the default types.</remarks>
        public static bool RemoveCreatorFunction<T>()
            where T : AspectBaseDefinition
        {
            var t = typeof(T);

            if (s_defaultTypes.Contains(t))
            {
                SignalsMod.Warning("Attempt to remove default signal aspect stopped.");
                return false;
            }

            s_failedAspects.Clear();
            return CreatorFunctions.Remove(t);
        }
    }
}
