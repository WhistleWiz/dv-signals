using MPAPI.Interfaces.Packets;
using Signals.Game;
using Signals.Game.Controllers;

namespace Signals.MP
{
    public class SettingsPacket : IPacket
    {
        // API says no but eh.
        public string CustomPack { get; set; } = string.Empty;
        public bool SpecialPath { get; set; }
        public bool ExitSignalsOnStorageTracks { get; set; }
        public OutsideStationPlacement OutsideStationPlacement { get; set; }

        public static SettingsPacket Get()
        {
            var settings = SignalsMod.Settings;

            return new SettingsPacket()
            {
                CustomPack = settings.CustomPack,
                SpecialPath = settings.SpecialPath,
                ExitSignalsOnStorageTracks = settings.ExitSignalsOnStorageTracks,
                OutsideStationPlacement = settings.OutsideStationPlacement
            };
        }
    }

    public abstract class SignalPacket : IPacket
    {
        public int SignalId { get; set; }
    }

    public class ReservationRequestPacket : SignalPacket
    {
        public float Duration { get; set; }
    }

    public class ReservationSuccessPacket : SignalPacket
    {
        public float Duration { get; set; }
    }

    public class ReservationFailurePacket : SignalPacket { }

    public class ReservationCancelRequestPacket : SignalPacket { }

    public class ReservationCancelSuccessPacket : SignalPacket { }

    public class OperationModePacket : SignalPacket
    {
        public SignalOperationMode Mode { get; set; }

        public void Apply(Signal signal)
        {
            signal.ChangeOperationMode(Mode);
        }

        public static OperationModePacket FromSignal(Signal signal)
        {
            return new OperationModePacket() { SignalId = signal.Id, Mode = signal.Operation };
        }
    }

    public class OverridePacket : SignalPacket
    {
        public int Aspect { get; set; }

        public void Apply(Signal signal)
        {
            signal.SetAspectOverride(Aspect);
        }

        public static OverridePacket FromSignal(Signal signal)
        {
            return new OverridePacket() { SignalId = signal.Id, Aspect = signal.CurrentAspectIndex };
        }
    }

    public class ShuntingAllowedPacket : SignalPacket
    {
        public bool Allowed { get; set; }

        public void Apply(Signal signal)
        {
            signal.SetShuntingStatus(Allowed);
        }

        public static ShuntingAllowedPacket FromSignal(Signal signal)
        {
            return new ShuntingAllowedPacket() { SignalId = signal.Id, Allowed = signal.ShuntingAllowed };
        }
    }

    public class RequiredBranchPacket : IPacket
    {
        public int ControllerId { get; set; }
        public int Branch { get; set; }

        public void Apply(BasicSignalController controller)
        {
            controller.ChangeRequiredBranch(Branch);
        }

        public static RequiredBranchPacket FromController(BasicSignalController controller)
        {
            return new RequiredBranchPacket()
            {
                ControllerId = controller.Id,
                Branch = controller.RequiredJunctionBranch ?? -1
            };
        }
    }
}
