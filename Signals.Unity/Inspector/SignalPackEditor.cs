using Signals.Common;
using UnityEditor;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    [CustomEditor(typeof(SignalPack))]
    internal class SignalPackEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (GUILayout.Button("Validate and Export"))
            {
                Exporter.Open((SignalPack)target);
            }
        }
    }
}
