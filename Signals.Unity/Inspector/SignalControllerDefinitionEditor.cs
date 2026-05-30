using Signals.Common;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    [CustomEditor(typeof(SignalControllerDefinition))]
    internal class SignalControllerDefinitionEditor : Editor
    {
        private ReorderableList _signalList = null!;
        private ReorderableList _shuntingSignalList = null!;

        private void OnEnable()
        {
            _signalList = EditorHelper.CreateReorderableList(serializedObject,
                serializedObject.FindProperty(nameof(SignalControllerDefinition.Signals)), true, true, true, "Signals");
            EditorHelper.AddBasicDrawerToList(_signalList);
            _shuntingSignalList = EditorHelper.CreateReorderableList(serializedObject,
                serializedObject.FindProperty(nameof(SignalControllerDefinition.ShuntingSignals)), true, true, true, "Shunting Signals");
            EditorHelper.AddBasicDrawerToList(_shuntingSignalList);
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
                            def.Signals = def.GetComponentsInChildren<SignalDefinition>().Where(x => !def.ShuntingSignals.Contains(x)).ToArray();
                            AssetHelper.SaveAsset(target);
                        }
                        break;
                    case nameof(SignalControllerDefinition.ShuntingSignals):
                        EditorHelper.DrawHeader("Optional");

                        _shuntingSignalList.DoLayoutList();

                        if (GUILayout.Button("Get Signals From Children"))
                        {
                            def.ShuntingSignals = def.GetComponentsInChildren<SignalDefinition>().Where(x => !def.Signals.Contains(x)).ToArray();
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
