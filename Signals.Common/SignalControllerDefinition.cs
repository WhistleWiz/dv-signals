using Signals.Common.Aspects;
using Signals.Common.Displays;
using UnityEngine;

namespace Signals.Common
{
    [AddComponentMenu("DV Signals/Signal Controller")]
    public class SignalControllerDefinition : MonoBehaviour
    {
        private const float HalfGauge = 1.435f / 2.0f;
        private static readonly Vector3 TrainSize = new Vector3(3.5f, 5.0f, 1.0f);
        private static readonly Vector3 TrainUp = new Vector3(0, TrainSize.y / 2, 0);

        [Tooltip("Each possible state for the signal\n" +
            "Order is important, as conditions are checked from first to last\n" +
            "The signal is turned off if none of these meet their conditions")]
        public AspectBaseDefinition[] Aspects = new AspectBaseDefinition[0];
        public Sprite? OffStateHUDSprite;

        [Header("Optional")]
        [Tooltip("Displays that aren't part of aspects")]
        public InfoDisplayDefinition[] Displays = new InfoDisplayDefinition[0];
        [Tooltip("Extra indicators")]
        public AspectBaseDefinition[] Indicators = new AspectBaseDefinition[0];
        //[Tooltip("How aspects in subsignals should be treated between main signals\n" +
        //    " • Ignore: uses only this controller's aspects\n" +
        //    " • Most Restrictive With Main: uses the most restrictive aspect from the subcontrollers, ignoring the main\n" +
        //    " • Active: uses the aspect of the currently active subcontroller (ex.: subcontroller for the current junction branch)")]
        //public SubsignalMode SubsignalMode = SubsignalMode.Ignore;
        //[Tooltip("Additional signals within the same main signal")]
        //public SubsignalControllerDefinition[] Subsignals = new SubsignalControllerDefinition[0];

        private void OnDrawGizmos()
        {
            if (transform.parent != null) return;

            Vector3 offset = Vector3.right * HalfGauge;

            Gizmos.color = new Color(0.9f, 0.9f, 0.9f, 0.2f);
            Gizmos.DrawCube(TrainUp, TrainSize);

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(TrainUp, TrainSize);
            Gizmos.DrawLine(Vector3.forward * 100 + offset, Vector3.back * 100 + offset);
            Gizmos.DrawLine(Vector3.forward * 100 - offset, Vector3.back * 100 - offset);
        }
    }
}
