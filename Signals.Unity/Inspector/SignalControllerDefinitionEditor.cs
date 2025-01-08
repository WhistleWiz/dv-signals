using Signals.Common;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    [CustomEditor(typeof(SignalControllerDefinition))]
    internal class SignalControllerDefinitionEditor : Editor
    {
        private SerializedProperty _openState = null!;
        private ReorderableList _stateList = null!;

        private void OnEnable()
        {
            _openState = serializedObject.FindProperty(nameof(SignalControllerDefinition.OpenState));

            _stateList = EditorHelper.CreateReorderableList(serializedObject, serializedObject.FindProperty(nameof(SignalControllerDefinition.OtherStates)),
                true, true, true, "Other States");

            _stateList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _stateList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.ObjectField(rect, element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_openState);
            _stateList.DoLayoutList();

            EditorGUILayout.HelpBox("Order is important, as conditions are checked from top to bottom\n" +
                "Open state is used if none of these meet their condition", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
