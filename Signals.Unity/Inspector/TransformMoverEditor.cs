using Signals.Common;
using UnityEditor;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    [CustomEditor(typeof(TransformMover)), CanEditMultipleObjects]
    internal class TransformMoverEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Preview Transformed"))
            {
                var comp = (TransformMover)target;
                comp.transform.localPosition = comp.TransformedPosition;
                comp.transform.localRotation = Quaternion.Euler(comp.TransformedRotation);
                comp.transform.localScale = comp.TransformedScale;
            }

            if (GUILayout.Button("Preview Original"))
            {
                var comp = (TransformMover)target;
                comp.transform.localPosition = comp.OriginalPosition;
                comp.transform.localRotation = Quaternion.Euler(comp.OriginalRotation);
                comp.transform.localScale = comp.OriginalScale;
            }

            if (GUILayout.Button("Reset"))
            {
                var comp = (TransformMover)target;
                comp.Reset();
            }
        }
    }
}
