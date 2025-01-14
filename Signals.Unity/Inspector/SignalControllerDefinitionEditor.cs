using Signals.Common;
using Signals.Common.Aspects;
using Signals.Common.Displays;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    [CustomEditor(typeof(SignalControllerDefinition))]
    internal class SignalControllerDefinitionEditor : Editor
    {
        private ReorderableList _stateList = null!;

        private void OnEnable()
        {
            _stateList = EditorHelper.CreateReorderableList(serializedObject, serializedObject.FindProperty(nameof(SignalControllerDefinition.Aspects)),
                true, true, true, "Aspects");

            _stateList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _stateList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.ObjectField(rect, element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            var def = (SignalControllerDefinition)target;
            var prop = serializedObject.FindProperty(nameof(SignalControllerDefinition.Aspects));

            do
            {
                switch (prop.name)
                {
                    case nameof(SignalControllerDefinition.Aspects):
                        EditorGUILayout.Space();
                        _stateList.DoLayoutList();

                        EditorGUILayout.HelpBox("Order is important, as conditions are checked from top to bottom", MessageType.Info);

                        if (GUILayout.Button("Get Aspects From Children"))
                        {
                            def.Aspects = def.GetComponentsInChildren<AspectBaseDefinition>().ToArray();
                            AssetHelper.SaveAsset(target);
                        }
                        break;
                    case nameof(SignalControllerDefinition.Displays):
                        EditorGUILayout.PropertyField(prop);
                        if (GUILayout.Button("Get Displays From Children"))
                        {
                            def.Displays = def.GetComponentsInChildren<InfoDisplay>().ToArray();
                            AssetHelper.SaveAsset(target);
                        }
                        break;
                    default:
                        EditorGUILayout.PropertyField(prop);
                        break;
                }
            } while (prop.Next(false));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
