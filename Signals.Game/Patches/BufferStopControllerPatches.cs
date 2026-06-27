using HarmonyLib;
using System.Collections.Generic;

namespace Signals.Game.Patches
{
    [HarmonyPatch(typeof(BufferStop))]
    internal class BufferStopControllerPatches
    {
        public static List<BufferStop> Stops = new List<BufferStop>();

        // Buffer stops disable themselves on Awake(), which prevents
        // Object.FindObjectsOfType<BufferStop>() from working on it.
        // Instead, they just get cached, and the list is cleared once
        // all are spawned.
        [HarmonyPatch("Awake"), HarmonyPostfix]
        private static void AwakePostfix(BufferStop __instance)
        {
            Stops.Add(__instance);
        }
    }
}
