using Signals.Common.Aspects;
using UnityEditor;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    [CustomEditor(typeof(SignalAspectBaseDefinition), true)]
    internal class SignalStateBaseDefinitionEditor : Editor
    {
        private SignalAspectBaseDefinition _def = null!;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            _def = (SignalAspectBaseDefinition)target;

            if (_def is GenericSignalAspectDefinition) return;

            // For signals without a user defined ID, show it as a readonly field.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Default ID", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.TextField("Id", _def.Id);
            GUI.enabled = true;
        }
    }
}
