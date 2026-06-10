using UnityEngine;

namespace Signals.Common
{
    [AddComponentMenu("DV Signals/Trackside Object")]
    public class TracksideObject : MonoBehaviour
    {
        private const float HalfGauge = 1.435f / 2.0f;
        private const float HalfTrackLength = 0.5f;
        private static readonly Vector3 TrackSize = new Vector3(0.07f, 0.152f, HalfTrackLength * 2);
        private static readonly Vector3 TrackSide = new Vector3(0.0351f, 0, 0);
        private static readonly Vector3 TrackUp = new Vector3(0, TrackSize.y / -2, 0);
        private static readonly Vector3 TrackSize2 = new Vector3(0.132f, 0.01f, HalfTrackLength * 2);
        private static readonly Vector3 TrackUp2 = new Vector3(0, -TrackSize.y + (TrackSize2.y / 2), 0);
        private static readonly Color Color2 = new Color(0.9f, 0.9f, 0.9f, 0.2f);

        public static float CurrentGauge = HalfGauge;

        [Tooltip("The distance from the controller\n" +
            "Negative values to place before the controller")]
        public float OffsetFromController = -10.0f;
        public float OffsetFromTrack = -2.05f;
        [Tooltip("If true, the object will be placed at the rail's position")]
        public bool AtRail = false;
        public bool MirrorWhenOnOppositeSide = false;

        private void OnDrawGizmos()
        {
            if (!AtRail) return;

            var trackPos = transform.position;

            Gizmos.color = Color2;
            Gizmos.DrawWireCube(trackPos + (OffsetFromTrack > 0 ? TrackSide : -TrackSide) + TrackUp, TrackSize);
            Gizmos.DrawWireCube(trackPos + (OffsetFromTrack > 0 ? TrackSide : -TrackSide) + TrackUp2, TrackSize2);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(Vector3.forward * HalfTrackLength + trackPos, Vector3.back * HalfTrackLength + trackPos);
        }

        public static void SetGaugeToDefault()
        {
            CurrentGauge = HalfGauge;
        }
    }
}
