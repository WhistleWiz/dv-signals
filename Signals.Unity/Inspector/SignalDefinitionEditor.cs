using Signals.Common;
using Signals.Common.Aspects;
using Signals.Common.Displays;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    [CustomEditor(typeof(SignalDefinition))]
    internal class SignalDefinitionEditor : Editor
    {
        private ReorderableList _aspectList = null!;
        private ReorderableList _displayList = null!;

        private void OnEnable()
        {
            _aspectList = EditorHelper.CreateReorderableList(serializedObject, serializedObject.FindProperty(nameof(SignalDefinition.Aspects)),
                true, true, true, "Aspects");
            EditorHelper.AddBasicDrawerToList(_aspectList);

            _displayList = EditorHelper.CreateReorderableList(serializedObject, serializedObject.FindProperty(nameof(SignalDefinition.Displays)),
                true, true, true, "Displays");
            EditorHelper.AddBasicDrawerToList(_displayList);
        }

        public override void OnInspectorGUI()
        {
            var def = (SignalDefinition)target;
            var prop = serializedObject.FindProperty(nameof(SignalDefinition.Aspects));

            do
            {
                switch (prop.name)
                {
                    case nameof(SignalDefinition.Aspects):
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("Order is important, as conditions are checked from top to bottom", MessageType.Info);

                        _aspectList.DoLayoutList();

                        if (GUILayout.Button("Get Aspects From Children"))
                        {
                            var aspects = def.GetComponentsInChildren<AspectBaseDefinition>().ToList();
                            var conditions = aspects.OfType<CombinationAspectDefinition>().SelectMany(x => x.Conditions).ToHashSet();
                            aspects.RemoveAll(conditions.Contains);
                            def.Aspects = aspects.ToArray();
                            AssetHelper.SaveAsset(target);
                        }
                        break;
                    case nameof(SignalDefinition.Displays):
                        EditorHelper.DrawHeader("Optional");

                        _displayList.DoLayoutList();

                        if (GUILayout.Button("Get Displays From Children"))
                        {
                            def.Displays = def.GetComponentsInChildren<DisplayBaseDefinition>();
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
