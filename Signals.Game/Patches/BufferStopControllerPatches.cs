using HarmonyLib;
using System.Collections.Generic;

namespace Signals.Game.Patches
{
    [HarmonyPatch(typeof(BufferStop))]
    internal class BufferStopControllerPatches
    {
        private const string MuseumTurntable = "[Y]_[CS]_[M";
        public static List<BufferStop> Stops = new List<BufferStop>();

        // Buffer stops disable themselves on Awake(), which prevents
        // Object.FindObjectsOfType<BufferStop>() from working on it.
        // Instead, they just get cached, and the list is cleared once
        // all are spawned.
        [HarmonyPatch("Awake"), HarmonyPostfix]
        private static void AwakePostfix(BufferStop __instance)
        {
            // Don't add them to the museum tracks.
            var turntable = __instance.GetComponentInParent<TurntableOutgoingTrack>();
            if (turntable != null && turntable.name.StartsWith(MuseumTurntable)) return;

            Stops.Add(__instance);
        }
    }
}
