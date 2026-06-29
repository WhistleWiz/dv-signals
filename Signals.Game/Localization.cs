using DV.Localization;

namespace Signals.Game
{
    public static class Localization
    {
        public static class Radio
        {
            public static string ModeName => LocalizationAPI.L("dv_signals/radio/mode_name");
            public static string Begin => LocalizationAPI.L("dv_signals/radio/begin");
            public static string ReservationFailed => LocalizationAPI.L("dv_signals/radio/reservation_failed");
            public static string Reserve => LocalizationAPI.L("dv_signals/radio/reserve");

            public static string ReservationSuccess(float duration)
            {
                return LocalizationAPI.L("dv_signals/radio/reservation_success", duration.ToString("F0"));
            }

            public static string DisplayNoSignal(float duration)
            {
                return LocalizationAPI.L("dv_signals/radio/display_no_signal", duration.ToString("F0"));
            }

            public static string DisplaySignal(string signal, float duration)
            {
                return LocalizationAPI.L("dv_signals/radio/display_signal", signal, duration.ToString("F0"));
            }
        }
    }
}
