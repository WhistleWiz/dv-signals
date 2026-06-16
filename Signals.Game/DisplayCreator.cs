using Signals.Common.Displays;
using Signals.Game.Displays;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game
{
    /// <summary>
    /// Class to instantiate the implementation of an <see cref="DisplayBaseDefinition"/>.
    /// </summary>
    public static class DisplayCreator
    {
        private static Type[] s_defaultTypes;
        private static HashSet<Type> s_failedDisplays = new HashSet<Type>();

        internal static Dictionary<Type, Func<DisplayBaseDefinition, Signal, IDisplay>> CreatorFunctions;

        static DisplayCreator()
        {
            CreatorFunctions = new Dictionary<Type, Func<DisplayBaseDefinition, Signal, IDisplay>>();

            Add((x, y) => new StaticDisplay(x, y));
            Add((x, y) => new SignalIdDisplay(x, y));
            Add((x, y) => new SignalNameDisplay(x, y));

            Add((x, y) => new JunctionBranchDisplay(x, y));
            Add((x, y) => new JunctionIdDisplay(x, y));

            Add((x, y) => new DistanceToNextDisplay(x, y));
            Add((x, y) => new NextStationDisplay(x, y));
            Add((x, y) => new SpeedLimitDisplay(x, y));
            Add((x, y) => new TrackInfoDisplay(x, y));

            s_defaultTypes = CreatorFunctions.Keys.ToArray();

            static void Add<T>(Func<DisplayBaseDefinition, Signal, DisplayBase<T>> func)
                where T : DisplayBaseDefinition
            {
                CreatorFunctions.Add(typeof(T), func);
            }
        }

        internal static IDisplay? Create<T>(Signal signal, T? def)
            where T : DisplayBaseDefinition
        {
            if (def == null) return null;

            var t = def.GetType();

            if (CreatorFunctions.TryGetValue(t, out var creator))
            {
                var result = creator(def, signal);
                return result;
            }

            // Otherwise every single signal would output the same error...
            if (!s_failedDisplays.Contains(t))
            {
                SignalsMod.Error($"Failed to find creator function for display '{t.FullName}'");
                s_failedDisplays.Add(t);
            }

            return null;
        }

        /// <summary>
        /// Add your own display creators for custom signal displays.
        /// </summary>
        /// <param name="func">
        /// The method that turns the definition into an implementation.
        /// <para>Inputs are the definition and the controller.</para>
        /// </param>
        /// <returns><see langword="true"/> if the type was sucessfully added, otherwise <see langword="false"/>.</returns>
        public static bool AddCreatorFunction<T>(Func<DisplayBaseDefinition, Signal, DisplayBase<T>> func)
            where T : DisplayBaseDefinition
        {
            var t = typeof(T);

            if (CreatorFunctions.ContainsKey(t))
            {
                SignalsMod.Error($"Could not add type '{t.FullName}' to signal display creators, type already exists!");
                return false;
            }

            s_failedDisplays.Clear();
            CreatorFunctions.Add(t, func);
            return true;
        }

        /// <summary>
        /// Remove your custom display creators.
        /// </summary>
        /// <returns><see langword="true"/> if the type was sucessfully removed, otherwise <see langword="false"/>.</returns>
        /// <remarks>This method will not remove the default types.</remarks>
        public static bool RemoveCreatorFunction<T>()
            where T : DisplayBaseDefinition
        {
            var t = typeof(T);

            if (s_defaultTypes.Contains(t))
            {
                SignalsMod.Warning("Attempt to remove default signal display stopped.");
                return false;
            }

            s_failedDisplays.Clear();
            return CreatorFunctions.Remove(t);
        }
    }
}
