using Signals.Common.Displays;
using Signals.Game.Controllers;
using Signals.Game.Displays;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game
{
    public static class DisplayCreator
    {
        private static Type[] s_defaultTypes;
        private static HashSet<Type> s_failedDisplays = new HashSet<Type>();

        internal static Dictionary<Type, Func<InfoDisplayDefinition, BasicSignalController, InfoDisplay>> CreatorFunctions;

        static DisplayCreator()
        {
            CreatorFunctions = new Dictionary<Type, Func<InfoDisplayDefinition, BasicSignalController, InfoDisplay>>
            {
                { typeof(SignalNameDisplayDefinition), (x, y) => new SignalNameDisplay(x, y) },
                { typeof(JunctionBranchDisplayDefinition), (x, y) => new JunctionBranchDisplay(x, y) },
                { typeof(TrackIdDisplayDefinition), (x, y) => new TrackIdDisplay(x, y) },
                { typeof(DistanceToNextDisplayDefinition), (x, y) => new DistanceToNextDisplay(x, y) },
                { typeof(StaticDisplayDefinition), (x, y) => new StaticDisplay(x, y) },
                { typeof(NextStationDisplayDefinition), (x, y) => new NextStationDisplay(x, y) }
            };

            s_defaultTypes = CreatorFunctions.Keys.ToArray();
        }

        internal static InfoDisplay? Create(BasicSignalController controller, InfoDisplayDefinition? def)
        {
            if (def == null) return null;

            var t = def.GetType();

            if (CreatorFunctions.TryGetValue(t, out var creator))
            {
                var result = creator(def, controller);
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
        public static bool AddCreatorFunction<T>(Func<InfoDisplayDefinition, BasicSignalController, InfoDisplay> func)
            where T : InfoDisplayDefinition
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
        /// <returns><see langword="true"/> if the tpe was sucessfully removed, otherwise <see langword="false"/>.</returns>
        /// <remarks>This method will not remove the default types.</remarks>
        public static bool RemoveCreatorFunction<T>()
            where T : InfoDisplayDefinition
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
