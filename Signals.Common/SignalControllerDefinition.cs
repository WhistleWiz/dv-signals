using UnityEngine;

namespace Signals.Common
{
    [AddComponentMenu("DV Signals/Signal Controller")]
    public class SignalControllerDefinition : MonoBehaviour
    {
        private const float HalfGauge = 1.435f / 2.0f;
        private const float HalfTrackLength = 5.0f;
        private static readonly Vector3 TrainSize = new Vector3(3.5f, 5.0f, 1.0f);
        private static readonly Vector3 TrainUp = new Vector3(0, TrainSize.y / 2, 0);
        private static readonly Vector3 TrackSize = new Vector3(0.07f, 0.152f, HalfTrackLength * 2);
        private static readonly Vector3 TrackSide = new Vector3(0.0351f, 0, 0);
        private static readonly Vector3 TrackUp = new Vector3(0, TrackSize.y / -2, 0);
        private static readonly Vector3 CatenaryUp = new Vector3(0, 6 - TrackUp.y, 0);
        private static readonly Color Color2 = new Color(0.9f, 0.9f, 0.9f, 0.2f);
        // Turntable stuff.
        private const int Sides = 48;
        private const float TurntableRadius = 12.324f;
        private static readonly Vector3 TurntableOffset = new Vector3(0, 0, TurntableRadius);
        private static readonly Vector3 TurntableWallSize1 = new Vector3(0.37f, 0.07f, 23.6f);
        private static readonly Vector3 TurntableWallSize2 = new Vector3(0.05f, 0.65f, 23.6f);
        private static readonly Vector3 TurntableWallSize3 = new Vector3(0.39f, 0.24f, 23.6f);
        private static readonly Vector3 TurntableWallOffset1 = new Vector3(2.314f, 0.750f, 0);
        private static readonly Vector3 TurntableWallOffset2 = new Vector3(2.375f, 0.390f, 0);
        private static readonly Vector3 TurntableWallOffset3 = new Vector3(2.305f, -0.055f, 0);
        // Bufferstop stuff.
        private static readonly Vector3 BufferOffset = new Vector3(0, 1.015f, 2.145f);
        private static readonly Vector3 BufferSize = new Vector3(2.465f, 0.325f, 0.22f);

        public static bool TurntableVis = false;
        public static bool BufferVis = false;

        [Tooltip("The individual signals for this controller")]
        public SignalDefinition[] Signals = new SignalDefinition[0];
        [Tooltip("How aspects in signals should be treated between controllers\n" +
            " • Active: uses the aspect of the currently active signal (ex.: signal for the current junction branch)\n" +
            " • Most Restrictive: uses the most restrictive aspect from all signals")]
        public ControllerMode Mode = ControllerMode.Active;
        [Tooltip("Negative numbers for right side, positive for left side")]
        public float Offset = -2.05f;
        [Tooltip("Flips the prefab if the signal is placed on the opposite side of the track")]
        public bool FlipPrefabWhenInOppositeSide = false;

        [Header("Optional")]
        [Tooltip("The shunting signals for this controller")]
        public SignalDefinition[] ShuntingSignals = new SignalDefinition[0];
        [Tooltip("Additional static signals for hoverables\n" +
            "These do not have blocks assigned")]
        public SignalDefinition[] DisplaySignals = new SignalDefinition[0];
        public TracksideObject[] TracksideObjects = new TracksideObject[0];
        [Tooltip("If the controller's prefab is flipped, these transforms will be flipped back (useful for text)")]
        public Transform[] UnflipTransforms = new Transform[0];

        private void OnDrawGizmos()
        {
            if (transform.parent != null) return;

            Vector3 offset = Vector3.left * Offset;
            Vector3 trackOffset = Vector3.right * HalfGauge;

            Gizmos.color = Color2;
            Gizmos.DrawCube(TrainUp + offset, TrainSize);
            Gizmos.DrawWireCube(TrackUp + TrackSide + trackOffset + offset, TrackSize);
            Gizmos.DrawWireCube(TrackUp - TrackSide - trackOffset + offset, TrackSize);
            Gizmos.DrawWireCube(CatenaryUp + offset, TrackSize);

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(TrainUp + offset, TrainSize);
            Gizmos.DrawLine(Vector3.forward * HalfTrackLength + trackOffset + offset, Vector3.back * HalfTrackLength + trackOffset + offset);
            Gizmos.DrawLine(Vector3.forward * HalfTrackLength - trackOffset + offset, Vector3.back * HalfTrackLength - trackOffset + offset);

            // Forward direction indicator.
            Gizmos.DrawLine(Vector3.forward * 0.5f + trackOffset * 0.5f + offset, Vector3.back * 0.5f + offset);
            Gizmos.DrawLine(Vector3.forward * 0.5f - trackOffset * 0.5f + offset, Vector3.back * 0.5f + offset);

            if (TurntableVis) DrawTurntable(offset);
            if (BufferVis) DrawBufferStop(offset);
        }

        private void DrawTurntable(Vector3 offset)
        {
            Gizmos.color = Color2;
            Vector3 prev = -TurntableOffset;
            Vector3 next;
            offset += TurntableOffset;

            for (int i = 0; i < Sides; i++)
            {
                next = Quaternion.Euler(0, 360.0f / Sides, 0) * prev;
                Gizmos.DrawLine(offset + prev, offset + next);
                prev = next;
            }

            Gizmos.DrawWireCube(offset + TurntableWallOffset1, TurntableWallSize1);
            Gizmos.DrawWireCube(offset + TurntableWallOffset2, TurntableWallSize2);
            Gizmos.DrawWireCube(offset + TurntableWallOffset3, TurntableWallSize3);
            Gizmos.DrawWireCube(offset + Mirror(TurntableWallOffset1), TurntableWallSize1);
            Gizmos.DrawWireCube(offset + Mirror(TurntableWallOffset2), TurntableWallSize2);
            Gizmos.DrawWireCube(offset + Mirror(TurntableWallOffset3), TurntableWallSize3);
        }

        private void DrawBufferStop(Vector3 offset)
        {
            Gizmos.color = Color2;
            Gizmos.DrawWireCube(offset + BufferOffset, BufferSize);
        }

        private static Vector3 Mirror(Vector3 v)
        {
            return new Vector3(-v.x, v.y, v.z);
        }
    }
}
