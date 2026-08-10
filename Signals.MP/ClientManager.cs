using MPAPI;
using MPAPI.Interfaces;
using Signals.Game;
using Signals.Game.Controllers;
using Signals.Game.Railway;
using UnityEngine;

namespace Signals.MP
{
    internal class ClientManager : MonoBehaviour
    {
        // Original settings stored.
        private static bool s_settingsSet = false;
        private static string s_pack = string.Empty;
        private static bool s_path;
        private static bool s_rsrv;
        private static bool s_exit;

        private IClient _client = null!;

        // Locks to prevent resending when receiving.
        private bool _lockOp = false;
        private bool _lockOv = false;
        private bool _lockSh = false;
        private bool _lockRb = false;

        private void Awake()
        {
            _client = MultiplayerAPI.Client;

            _client.RegisterPacket<OperationModePacket>(OperationModeReceived);
            _client.RegisterPacket<OverridePacket>(OverrideReceived);
            _client.RegisterPacket<ShuntingAllowedPacket>(ShuntingReceived);
            _client.RegisterPacket<RequiredBranchPacket>(RequiredBranchReceived);
            _client.RegisterPacket<ReservationSuccessPacket>(ReservationSuccessReceived);
            _client.RegisterPacket<ReservationFailurePacket>(ReservationFailureReceived);
            _client.RegisterPacket<ReservationCancelSuccessPacket>(ReservationCancelSuccessReceived);

            SignalManager.OperationModeChanged += ChangeOperationMode;
            SignalManager.OverrideChanged += ChangeOverride;
            SignalManager.ShuntingAllowedChanged += ChangeShunting;
            SignalManager.RequiredBranchChanged += ChangeRequiredBranch;

            Game.MultiplayerIntegration.SetRunningStatus(true);

            SignalsMod.LogMP("Client loaded successfuly");
        }

        private void OnDestroy()
        {
            SignalManager.OperationModeChanged -= ChangeOperationMode;
            SignalManager.OverrideChanged -= ChangeOverride;
            SignalManager.ShuntingAllowedChanged -= ChangeShunting;
            SignalManager.RequiredBranchChanged -= ChangeRequiredBranch;

            Game.MultiplayerIntegration.SetRunningStatus(false);

            if (!s_settingsSet) return;

            var settings = SignalsMod.Settings;
            settings.CustomPack = s_pack;
            settings.SpecialPath = s_path;
            settings.SpecialReservation = s_rsrv;
            settings.ExitSignalsOnStorageTracks = s_exit;
        }

        private void OperationModeReceived(OperationModePacket packet)
        {
            if (GetSignal(packet, "operation mode change", out var signal))
            {
                _lockOp = true;
                signal.ChangeOperationMode(packet.Mode);
                _lockOp = false;
            }
        }

        private void OverrideReceived(OverridePacket packet)
        {
            if (GetSignal(packet, "aspect override change", out var signal))
            {
                _lockOv = true;
                signal.SetAspectOverride(packet.Aspect);
                _lockOv = false;
            }
        }

        private void ShuntingReceived(ShuntingAllowedPacket packet)
        {
            if (GetSignal(packet, "shunting allowed change", out var signal))
            {
                _lockSh = true;
                signal.SetShuntingStatus(packet.Allowed);
                _lockSh = false;
            }
        }

        private void RequiredBranchReceived(RequiredBranchPacket packet)
        {
            if (!SignalManager.Instance.TryGetController(packet.ControllerId, out var controller))
            {
                SignalsMod.Error($"[Networking] Received required branch change for controller {packet.ControllerId}, but it does not exist!\n" +
                    $"Ensure both clients have the same signal pack active.");
                return;
            }

            _lockRb = true;
            controller.ChangeRequiredBranch(packet.Branch);
            _lockRb = false;
        }

        private void ReservationSuccessReceived(ReservationSuccessPacket packet)
        {
            if (GetSignal(packet, "reservation success", out var signal))
            {
                // Host already reserved, but packet must still be received to call the functions.
                if (!MultiplayerAPI.Instance.IsHost)
                {
                    if (packet.Duration > 0)
                    {
                        TrackReserver.ReserveForSignal(signal, packet.Duration);
                    }
                    else
                    {
                        TrackReserver.ReserveForSignal(signal);
                    }
                }

                Game.MultiplayerIntegration.OnReservationRequestResultReceived?.Invoke(packet.SignalId, true);
            }
        }

        private void ReservationFailureReceived(ReservationFailurePacket packet)
        {
            if (GetSignal(packet, "reservation failure", out var signal))
            {
                Game.MultiplayerIntegration.OnReservationRequestResultReceived?.Invoke(packet.SignalId, false);
            }
        }

        private void ReservationCancelSuccessReceived(ReservationCancelSuccessPacket packet)
        {
            if (GetSignal(packet, "reservation cancel success", out var signal))
            {
                if (!MultiplayerAPI.Instance.IsHost)
                {
                    TrackReserver.ClearFromSignal(signal);
                }

                Game.MultiplayerIntegration.OnReservationClearResultReceived?.Invoke(packet.SignalId);
            }
        }

        private void ChangeOperationMode(Signal signal, SignalOperationMode mode)
        {
            if (_lockOp) return;

            _client.SendPacketToServer(OperationModePacket.FromSignal(signal));
        }

        private void ChangeOverride(Signal signal, int aspect)
        {
            if (_lockOv) return;

            _client.SendPacketToServer(OverridePacket.FromSignal(signal));
        }

        private void ChangeShunting(Signal signal, bool allowed)
        {
            if (_lockSh) return;

            _client.SendPacketToServer(ShuntingAllowedPacket.FromSignal(signal));
        }

        private void ChangeRequiredBranch(BasicSignalController controller, int? index)
        {
            if (_lockRb) return;

            _client.SendPacketToServer(RequiredBranchPacket.FromController(controller));
        }

        private static void PrintError(string name, int signalId)
        {
            SignalsMod.ErrorMP($"Received {name} for signal {signalId}, but it does not exist!\n" +
                $"Ensure both clients have the same signal pack active.");
        }

        private static bool GetSignal(SignalPacket packet, string name, out Signal signal)
        {
            if (SignalManager.Instance.TryGetSignal(packet.SignalId, out signal)) return true;

            PrintError(name, packet.SignalId);
            return false;
        }

        public static void Initialise(IClient client)
        {
            client.RegisterPacket<SettingsPacket>(SettingReceived);
        }

        private static void SettingReceived(SettingsPacket packet)
        {
            SignalsMod.LogMP("Received settings from server");

            var settings = SignalsMod.Settings;

            s_pack = settings.CustomPack;
            s_path = settings.SpecialPath;
            s_rsrv = settings.SpecialReservation;
            s_exit = settings.ExitSignalsOnStorageTracks;
            s_settingsSet = true;

            settings.CustomPack = packet.CustomPack;
            settings.SpecialPath = packet.SpecialPath;
            settings.SpecialReservation = packet.SpecialReservation;
            settings.ExitSignalsOnStorageTracks = packet.ExitSignalsOnStorageTracks;
        }
    }
}
