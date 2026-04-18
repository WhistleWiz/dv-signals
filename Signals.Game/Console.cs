using CommandTerminal;
using Signals.Game.Railway;
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

        [RegisterCommand("Signals.Reserve",
            Help = "Reserves a signal's tracks, with an optional duration",
            Hint = "Signals.Reserve 123 30",
            MinArgCount = 1, MaxArgCount = 2)]
        public static void Reserve(CommandArg[] args)
        {
            if (!SignalManager.Running)
            {
                OutsideSessionError();
                return;
            }

            if (SignalManager.Instance.TryGetSignal(args[0].Int, out var signal))
            {
                Debug.LogError($"Could not find signal with ID '{args[0]}'");
                return;
            }

            float duration = 0;

            if (args.Length == 2)
            {
                duration = args[1].Float;

                if (duration <= 0)
                {
                    Debug.LogError($"Invalid reservation duration specified: {args[1]}");
                    return;
                }
            }

            TrackReserver.ReserveForSignal(signal);

            if (duration > 0)
            {
                TrackReserver.ClearFromSignalDelayed(signal, duration);
            }
        }

        [RegisterCommand("Signals.Unreserve",
            Help = "Reserves a signal's tracks, with an optional delay",
            Hint = "Signals.Unreserve 123 30",
            MinArgCount = 1, MaxArgCount = 2)]
        public static void Unreserve(CommandArg[] args)
        {
            if (!SignalManager.Running)
            {
                OutsideSessionError();
                return;
            }

            if (SignalManager.Instance.TryGetSignal(args[0].Int, out var signal))
            {
                Debug.LogError($"Could not find signal with ID '{args[0]}'");
                return;
            }

            float delay = 0;

            if (args.Length == 2)
            {
                delay = args[1].Float;

                if (delay <= 0)
                {
                    Debug.LogError($"Invalid delay specified: {args[1]}");
                    return;
                }
            }

            if (delay > 0)
            {
                TrackReserver.ClearFromSignalDelayed(signal, delay);
            }
            else
            {
                TrackReserver.ClearFromSignal(signal);
            }
        }
    }
}
