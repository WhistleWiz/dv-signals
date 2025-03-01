using Signals.Common;
using Signals.Unity.Utilities;
using UnityEditor;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    [CustomEditor(typeof(SignalLightSequenceDefinition))]
    internal class SignalLightSequenceDefinitionEditor : Editor
    {
        private SerializedProperty _lights = null!;
        private SerializedProperty _states = null!;
        private SerializedProperty _time = null!;
        private SerializedProperty _step = null!;

        private GUIContent _stateContent = new GUIContent("State");

        private void OnEnable()
        {
            _lights = serializedObject.FindProperty(nameof(SignalLightSequenceDefinition.Lights));
            _states = serializedObject.FindProperty(nameof(SignalLightSequenceDefinition.States));
            _time = serializedObject.FindProperty(nameof(SignalLightSequenceDefinition.Timing));
            _step = serializedObject.FindProperty(nameof(SignalLightSequenceDefinition.Step));
        }

        public override void OnInspectorGUI()
        {
            _lights.isExpanded = EditorGUILayout.Foldout(_lights.isExpanded, "Lights and Intial States");

            if (_lights.isExpanded )
            {
                EditorGUI.indentLevel++;

                int length = EditorGUILayout.DelayedIntField("Size", _lights.arraySize);
                int step = _step.intValue;

                _lights.arraySize = length;
                _states.arraySize = length;

                for (int i = 0; i < length; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(_lights.GetArrayElementAtIndex(i));
                    EditorGUIUtility.labelWidth = 50;
                    EditorGUILayout.PropertyField(_states.GetArrayElementAtIndex(i), _stateContent, GUILayout.Width(66));
                    EditorGUIUtility.labelWidth = 0;

                    EditorGUILayout.EndHorizontal();

                    // Separate the lights by step groups.
                    if (step != 1 && i != length - 1 && (i + 1) % step == 0)
                    {
                        EditorGUILayout.Space(4);
                    }
                }

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_time);
            EditorGUILayout.PropertyField(_step);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Preview"))
            {
                SequencePreview.ShowWindow((SignalLightSequenceDefinition)target);
            }
        }
    }
}
