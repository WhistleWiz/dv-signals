using Signals.Common.States;
using UnityEditor;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    [CustomEditor(typeof(SignalStateBaseDefinition), true)]
    internal class SignalStateBaseDefinitionEditor : Editor
    {
        private SignalStateBaseDefinition _def = null!;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            _def = (SignalStateBaseDefinition)target;

            if (_def is GenericSignalStateDefinition) return;

            // For signals without a user defined ID, show it as a readonly field.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Default ID", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.TextField("Id", _def.Id);
            GUI.enabled = true;
        }
    }
}
