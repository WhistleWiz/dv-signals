using DV;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Signals.Game.Patches
{
    [HarmonyPatch(typeof(CommsRadioController))]
    internal static class CommsRadioControllerPatches
    {
        [HarmonyPatch("Awake"), HarmonyPostfix]
        private static void AwakePostfix(CommsRadioController __instance)
        {
            // Create the object as inactive to prevent Awake() from running too early.
            var go = new GameObject(nameof(CommsRadioSignalReserver));
            go.transform.parent = __instance.transform;
            go.SetActive(false);
            var mode = go.AddComponent<CommsRadioSignalReserver>();
            mode.Controller = __instance;

            // Force the new mode into the private list of modes...
            var t = typeof(CommsRadioController);
            var f = t.GetField("allModes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ((List<ICommsRadioMode>)f.GetValue(__instance)).Add(mode);

            // Reactivate the GO with the new mode and refresh the controller.
            go.SetActive(true);
        }
    }
}
