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
        }
    }
}
