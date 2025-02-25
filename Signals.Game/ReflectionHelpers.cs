using DV.Logic.Job;
using System.Reflection;

namespace Signals.Game
{
    internal static class ReflectionHelpers
    {
        private static BindingFlags PrivateFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        private static PropertyInfo? s_trackIdProperty;
        private static PropertyInfo TrackIdProperty
        {
            get
            {
                if (s_trackIdProperty == null)
                {
                    s_trackIdProperty = typeof(TrackID).GetProperty("TrimmedOrderNumber", PrivateFlags);
                }

                return s_trackIdProperty;
            }
        }

        public static string GetTrimmedOrderNumber(TrackID trackID) => (string)TrackIdProperty.GetValue(trackID);
    }
}
