using Signals.Common.Aspects;
using UnityEngine;

namespace Signals.Common
{
    public class SignalControllerDefinition : MonoBehaviour
    {
        private const float HalfGauge = 1.435f / 2.0f;

        [Tooltip("Each possible state for the signal\n" +
            "Order is important, as conditions are checked from first to last\n" +
            "Open state is used if none of these meet their condition")]
        public AspectBaseDefinition[] Aspects = new AspectBaseDefinition[0];

        [Header("Optional")]
        public Sprite? OffStateHUDSprite;
        [Tooltip("Used for mechanical signals")]
        public Animator? Animator;

        private void OnDrawGizmos()
        {
            Vector3 offset = Vector3.right * HalfGauge;

            Gizmos.color = new Color(0.9f, 0.9f, 0.9f, 0.2f);
            Gizmos.DrawCube(Vector3.up * 2.0f, new Vector3(3.5f, 4.0f, 1.0f));

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.up * 2.0f, new Vector3(3.5f, 4.0f, 1.0f));
            Gizmos.DrawLine(Vector3.forward * 100 + offset, Vector3.back * 100 + offset);
            Gizmos.DrawLine(Vector3.forward * 100 - offset, Vector3.back * 100 - offset);
        }
    }
}
