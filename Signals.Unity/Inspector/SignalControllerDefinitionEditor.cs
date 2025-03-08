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
        private ReorderableList _aspectList = null!;
        private ReorderableList _displayList = null!;

        private void OnEnable()
        {
            _aspectList = EditorHelper.CreateReorderableList(serializedObject, serializedObject.FindProperty(nameof(SignalControllerDefinition.Aspects)),
                true, true, true, "Aspects");

            _aspectList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _aspectList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.ObjectField(rect, element, GUIContent.none);
            };

            _displayList = EditorHelper.CreateReorderableList(serializedObject, serializedObject.FindProperty(nameof(SignalControllerDefinition.Displays)),
                true, true, true, "Displays");

            _displayList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _displayList.serializedProperty.GetArrayElementAtIndex(index);
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
                        EditorGUILayout.HelpBox("Order is important, as conditions are checked from top to bottom", MessageType.Info);

                        _aspectList.DoLayoutList();

                        if (GUILayout.Button("Get Aspects From Children"))
                        {
                            def.Aspects = def.GetComponentsInChildren<AspectBaseDefinition>().ToArray();
                            AssetHelper.SaveAsset(target);
                        }
                        break;
                    case nameof(SignalControllerDefinition.Displays):
                        EditorHelper.DrawHeader("Optional");
                        _displayList.DoLayoutList();

                        if (GUILayout.Button("Get Displays From Children"))
                        {
                            def.Displays = def.GetComponentsInChildren<InfoDisplayDefinition>().ToArray();
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
