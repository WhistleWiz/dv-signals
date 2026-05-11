using Signals.Common;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    [CustomEditor(typeof(SignalControllerDefinition))]
    internal class SignalControllerDefinitionEditor : Editor
    {
        private ReorderableList _signalList = null!;

        private void OnEnable()
        {
            _signalList = EditorHelper.CreateReorderableList(serializedObject, serializedObject.FindProperty(nameof(SignalControllerDefinition.Signals)),
                true, true, true, "Signals");
            EditorHelper.AddBasicDrawerToList(_signalList);
        }

        public override void OnInspectorGUI()
        {
            var def = (SignalControllerDefinition)target;
            var prop = serializedObject.FindProperty(nameof(SignalControllerDefinition.Signals));

            do
            {
                switch (prop.name)
                {
                    case nameof(SignalControllerDefinition.Signals):
                        _signalList.DoLayoutList();

                        if (GUILayout.Button("Get Signals From Children"))
                        {
                            def.Signals = def.GetComponentsInChildren<SignalDefinition>();
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
