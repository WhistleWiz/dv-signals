using Signals.Common;
using UnityEditor;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    [CustomEditor(typeof(TransformMoverTarget)), CanEditMultipleObjects]
    internal class TransformMoverTargetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Copy Transform Mover Values"))
            {
                var comp = (TransformMoverTarget)target;

                if (comp.Mover != null)
                {
                    comp.transform.localPosition = comp.Mover.OriginalPosition;
                    comp.transform.localRotation = Quaternion.Euler(comp.Mover.OriginalRotation);
                    comp.transform.localScale = comp.Mover.OriginalScale;
                }
            }

            if (GUILayout.Button("Set to Current Values"))
            {
                var comp = (TransformMoverTarget)target;
                comp.Reset();
            }
        }
    }
}
