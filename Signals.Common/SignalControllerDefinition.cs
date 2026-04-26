using UnityEngine;

namespace Signals.Common
{
    [AddComponentMenu("DV Signals/Signal Controller")]
    public class SignalControllerDefinition : MonoBehaviour
    {
        private const float HalfGauge = 1.435f / 2.0f;
        private static readonly Vector3 TrainSize = new Vector3(3.5f, 5.0f, 1.0f);
        private static readonly Vector3 TrainUp = new Vector3(0, TrainSize.y / 2, 0);

        public float Offset = 2.05f;
        [Tooltip("The individual signals for this controller")]
        public SignalDefinition[] Signals = new SignalDefinition[0];
        [Tooltip("How aspects in signals should be treated between controllers\n" +
            " • Active: uses the aspect of the currently active signal (ex.: signal for the current junction branch)\n" +
            " • Most Restrictive: uses the most restrictive aspect from all signals")]
        public ControllerMode Mode = ControllerMode.Active;

        [Header("Optional")]
        [Tooltip("The shunting signal for this controller")]
        public SignalDefinition? ShuntingSignal;

        private void OnDrawGizmos()
        {
            if (transform.parent != null) return;

            Vector3 offset = Vector3.left * Offset;
            Vector3 trackOffset = Vector3.right * HalfGauge;

            Gizmos.color = new Color(0.9f, 0.9f, 0.9f, 0.2f);
            Gizmos.DrawCube(TrainUp + offset, TrainSize);

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(TrainUp + offset, TrainSize);
            Gizmos.DrawLine(Vector3.forward * 100 + trackOffset + offset, Vector3.back * 100 + trackOffset + offset);
            Gizmos.DrawLine(Vector3.forward * 100 - trackOffset + offset, Vector3.back * 100 - trackOffset + offset);
        }
    }
}
