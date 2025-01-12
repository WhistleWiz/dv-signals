using Signals.Common.Aspects;
using Signals.Game.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game
{
    /// <summary>
    /// Class to instantiate the implementations of signal aspect definitions.
    /// </summary>
    public static class AspectCreator
    {
        private static Type[] s_defaultTypes;
        private static HashSet<Type> s_failedAspects = new HashSet<Type>();

        internal static Dictionary<Type, Func<SignalAspectBaseDefinition, SignalController, SignalAspectBase>> CreatorFunctions;

        static AspectCreator()
        {
            CreatorFunctions = new Dictionary<Type, Func<SignalAspectBaseDefinition, SignalController, SignalAspectBase>>
            {
                { typeof(OpenSignalAspectDefinition), (x, y) => new OpenSignalAspect(x, y) },
                { typeof(ClosedSignalAspectDefinition), (x, y) => new ClosedSignalAspect(x, y) },
                { typeof(IsNextClosedSignalAspectDefinition), (x, y) => new IsNextClosedSignalAspect(x, y) },
                { typeof(IsNextAspectSignalAspectDefinition), (x, y) => new IsNextAspectSignalAspect(x, y) }
            };

            s_defaultTypes = CreatorFunctions.Keys.ToArray();
        }

        internal static SignalAspectBase? Create(SignalController controller, SignalAspectBaseDefinition? def)
        {
            if (def == null) return null;

            var t = def.GetType();

            if (CreatorFunctions.TryGetValue(t, out var creator))
            {
                var result = creator(def, controller);
                return result;
            }

            // Otherwise every single signal would output the same error...
            if (!s_failedAspects.Contains(t))
            {
                SignalsMod.Error($"Failed to find creator function for state '{t.FullName}'");
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
        public static bool AddCreatorFunction<T>(Func<SignalAspectBaseDefinition, SignalController, SignalAspectBase> func)
            where T : SignalAspectBaseDefinition
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
        /// <param name="id">The ID to remove.</param>
        /// <returns><see langword="true"/> if the ID was sucessfully removed, otherwise <see langword="false"/>.</returns>
        /// <remarks>This method will not remove the default IDs.</remarks>
        public static bool RemoveCreatorFunction<T>()
            where T : SignalAspectBaseDefinition
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
