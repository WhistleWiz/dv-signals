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
            _def = (SignalStateBaseDefinition)target;

            if (_def is GenericSignalStateDefinition) goto DrawBase;

            // For signals without a user defined ID, show it as a readonly field.
            GUI.enabled = false;
            EditorGUILayout.TextField("Id", _def.Id);
            GUI.enabled = true;

            DrawBase:
            base.OnInspectorGUI();
        }
    }
}
