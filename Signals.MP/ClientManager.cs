using MPAPI;
using MPAPI.Interfaces;
using Signals.Game;
using Signals.Game.Railway;
using UnityEngine;

namespace Signals.MP
{
    internal class ClientManager : MonoBehaviour
    {
        private IClient _client = null!;

        private bool _settingsSet = false;
        private string _pack = string.Empty;
        private bool _path;
        private bool _exit;
        private OutsideStationPlacement _placement;

        private void Awake()
        {
            _client = MultiplayerAPI.Client;

            _client.RegisterPacket<SettingsPacket>(SettingReceived);
            _client.RegisterPacket<OperationModePacket>(OperationModeReceived);
            _client.RegisterPacket<OverridePacket>(OverrideReceived);
            _client.RegisterPacket<ShuntingAllowedPacket>(ShuntingReceived);
            _client.RegisterPacket<ReservationSuccessPacket>(ReservationSuccessReceived);
            _client.RegisterPacket<ReservationFailurePacket>(ReservationFailureReceived);

            SignalManager.OperationModeChanged += ChangeOperationMode;
            SignalManager.OverrideChanged += ChangeOverride;
            SignalManager.ShuntingAllowedChanged += ChangeShunting;
        }

        private void OnDestroy()
        {
            SignalManager.OperationModeChanged -= ChangeOperationMode;
            SignalManager.OverrideChanged -= ChangeOverride;
            SignalManager.ShuntingAllowedChanged -= ChangeShunting;

            if (!_settingsSet) return;

            var settings = SignalsMod.Settings;
            settings.CustomPack = _pack;
            settings.SpecialPath = _path;
            settings.ExitSignalsOnStorageTracks = _exit;
            settings.OutsideStationPlacement = _placement;
        }

        private void SettingReceived(SettingsPacket packet)
        {
            var settings = SignalsMod.Settings;

            _pack = settings.CustomPack;
            _path = settings.SpecialPath;
            _exit = settings.ExitSignalsOnStorageTracks;
            _placement = settings.OutsideStationPlacement;
            _settingsSet = true;

            settings.CustomPack = packet.CustomPack;
            settings.SpecialPath = packet.SpecialPath;
            settings.ExitSignalsOnStorageTracks = packet.ExitSignalsOnStorageTracks;
            settings.OutsideStationPlacement = packet.OutsideStationPlacement;
        }

        private void OperationModeReceived(OperationModePacket packet)
        {
            if (!SignalManager.Instance.TryGetSignal(packet.SignalId, out var signal))
            {
                PrintError("operation mode change", packet.SignalId);
                return;
            }

            signal.ChangeOperationMode(packet.Mode);
        }

        private void OverrideReceived(OverridePacket packet)
        {
            if (!SignalManager.Instance.TryGetSignal(packet.SignalId, out var signal))
            {
                PrintError("aspect override change", packet.SignalId);
                return;
            }

            signal.SetAspectOverride(packet.Aspect);
        }

        private void ShuntingReceived(ShuntingAllowedPacket packet)
        {
            if (!SignalManager.Instance.TryGetSignal(packet.SignalId, out var signal))
            {
                PrintError("shunting allowed change", packet.SignalId);
                return;
            }

            signal.SetShuntingStatus(packet.Allowed);
        }

        private void ReservationSuccessReceived(ReservationSuccessPacket packet)
        {
            if (GetSignal(packet, "reservation success", out var signal))
            {
                TrackReserver.ReserveForSignal(signal);
                Game.MultiplayerIntegration.OnReservationRequestReceived?.Invoke(packet.SignalId, true);
            }
        }

        private void ReservationFailureReceived(ReservationFailurePacket packet)
        {
            if (GetSignal(packet, "reservation failure", out var signal))
            {
                Game.MultiplayerIntegration.OnReservationRequestReceived?.Invoke(packet.SignalId, false);
            }
        }

        private static void ChangeOperationMode(Signal signal, SignalOperationMode mode)
        {
            MultiplayerIntegration.SendChangeOperationMode(signal.Id, mode);
        }

        private static void ChangeOverride(Signal signal, int aspect)
        {
            MultiplayerIntegration.SendChangeOverride(signal.Id, aspect);
        }

        private static void ChangeShunting(Signal signal, bool allowed)
        {
            MultiplayerIntegration.SendChangeShunting(signal.Id, allowed);
        }

        private static void PrintError(string name, int signalId)
        {
            SignalsMod.Error($"[Networking] Received {name} for signal {signalId}, but it does not exist!\n" +
                $"Ensure both clients have the same signal pack active.");
        }

        private static bool GetSignal(SignalPacket packet, string name, out Signal signal)
        {
            if (SignalManager.Instance.TryGetSignal(packet.SignalId, out signal)) return true;

            PrintError(name, packet.SignalId);
            return false;
        }
    }
}
