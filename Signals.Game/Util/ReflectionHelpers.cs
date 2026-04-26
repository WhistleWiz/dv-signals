using DV.Logic.Job;
using DV.Signs;
using System.Reflection;
using UnityEngine;

namespace Signals.Game.Util
{
    internal static class ReflectionHelpers
    {
        private const BindingFlags PrivateFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        // TrackID.
        private static PropertyInfo? s_trackIdTrimmedOrderNumber;
        private static PropertyInfo TrackIdTrimmedOrderNumber
        {
            get
            {
                if (s_trackIdTrimmedOrderNumber == null)
                {
                    // Why is it private?
                    // No really, why?
                    s_trackIdTrimmedOrderNumber = typeof(TrackID).GetProperty("TrimmedOrderNumber", PrivateFlags);
                }

                return s_trackIdTrimmedOrderNumber;
            }
        }

        public static string GetTrimmedOrderNumber(TrackID trackID) => (string)TrackIdTrimmedOrderNumber.GetValue(trackID);

        private static FieldInfo? s_trackIdType;
        private static FieldInfo TrackIdType
        {
            get
            {
                if (s_trackIdType == null)
                {
                    s_trackIdType = typeof(TrackID).GetField("trackType", PrivateFlags);
                }

                return s_trackIdType;
            }
        }

        public static string GetTrackType(TrackID trackID) => (string)TrackIdType.GetValue(trackID);


        // SignHover.
        private static FieldInfo? s_signHoverIsHovered;
        private static FieldInfo SignHoveredIsHovered
        {
            get
            {
                if (s_signHoverIsHovered == null)
                {
                    s_signHoverIsHovered = typeof(SignHover).GetField("isHovered", PrivateFlags);
                }

                return s_signHoverIsHovered;
            }
        }

        public static bool IsHovered(SignHover sign) => (bool)SignHoveredIsHovered.GetValue(sign);

        private static FieldInfo? s_signHoverRenderers;
        private static FieldInfo SignHoveredRenderers
        {
            get
            {
                if (s_signHoverRenderers == null)
                {
                    s_signHoverRenderers = typeof(SignHover).GetField("renderers", PrivateFlags);
                }

                return s_signHoverRenderers;
            }
        }

        public static MeshRenderer[] GetRenderers(SignHover sign) => (MeshRenderer[])SignHoveredRenderers.GetValue(sign);
    }
}
