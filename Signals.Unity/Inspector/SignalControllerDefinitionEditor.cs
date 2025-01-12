using Signals.Common;
using Signals.Common.Aspects;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    [CustomEditor(typeof(SignalControllerDefinition))]
    internal class SignalControllerDefinitionEditor : Editor
    {
        private SerializedProperty _prop = null!;
        private ReorderableList _stateList = null!;

        private void OnEnable()
        {
            _stateList = EditorHelper.CreateReorderableList(serializedObject, serializedObject.FindProperty(nameof(SignalControllerDefinition.OtherAspects)),
                true, true, true, "Other States");

            _stateList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _stateList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.ObjectField(rect, element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            _prop = serializedObject.FindProperty(nameof(SignalControllerDefinition.DefaultAspect));

            do
            {
                switch (_prop.name)
                {
                    // Draw a reorderable list for this property instead of the normal field.
                    case nameof(SignalControllerDefinition.OtherAspects):
                        EditorGUILayout.Space();
                        _stateList.DoLayoutList();

                        EditorGUILayout.HelpBox("Order is important, as conditions are checked from top to bottom\n" +
                            "Open state is used if none of these meet their condition", MessageType.Info);

                        if (GUILayout.Button("Get States From Children"))
                        {
                            var def = (SignalControllerDefinition)target;
                            def.OtherAspects = def.GetComponentsInChildren<SignalAspectBaseDefinition>().Where(x => x != def.DefaultAspect).ToArray();
                            AssetHelper.SaveAsset(target);
                        }
                        continue;
                    default:
                        EditorGUILayout.PropertyField(_prop);
                        continue;
                }


            } while (_prop.Next(false));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
