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
            var reserver = CreateReserver(__instance);

            // Force the new modes into the private list of modes...
            var t = typeof(CommsRadioController);
            var f = t.GetField("allModes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var l = ((List<ICommsRadioMode>)f.GetValue(__instance));
            l.Add(reserver);

            // Reactivate the GOs.
            reserver.gameObject.SetActive(true);
        }

        private static CommsRadioSignalReserver CreateReserver(CommsRadioController controller)
        {
            var go = new GameObject(nameof(CommsRadioSignalReserver));
            go.transform.parent = controller.transform;
            go.transform.localPosition = Vector3.zero;
            go.SetActive(false);
            var mode = go.AddComponent<CommsRadioSignalReserver>();
            mode.Controller = controller;

            return mode;
        }
    }
}
