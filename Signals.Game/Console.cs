using CommandTerminal;
using UnityEngine;

namespace Signals.Game
{
    internal static class Console
    {
        private static void OutsideSessionError() => Debug.LogError("Cannot be used outside of a loaded session!");

        [RegisterCommand("Signals.RestrictAll",
            Help = "Sets all signals in the registry to its most restrictive aspect, and their operation mode to temporarily manual",
            MinArgCount = 0, MaxArgCount = 0)]
        public static void CloseAll(CommandArg[] args)
        {
            if (!SignalManager.Running)
            {
                OutsideSessionError();
                return;
            }

            foreach (var signal in SignalManager.Instance.AllSignals)
            {
                signal.ChangeToMostRestrictive(true);
                signal.Operation = SignalOperationMode.TempOverride;
            }
        }

        [RegisterCommand("Signals.Panic",
            Help = "Sets all signals in the registry to its most restrictive aspect, and their operation mode to semi-manual",
            MinArgCount = 0, MaxArgCount = 0)]
        public static void Panic(CommandArg[] args)
        {
            if (!SignalManager.Running)
            {
                OutsideSessionError();
                return;
            }

            foreach (var signal in SignalManager.Instance.AllSignals)
            {
                signal.ChangeToMostRestrictive(true);
                signal.Operation = SignalOperationMode.SemiManual;
            }
        }
    }
}
