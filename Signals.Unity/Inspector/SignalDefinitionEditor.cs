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
        private SerializedProperty _indicators = null!;
        private SerializedProperty _sprites = null!;

        private GUIContent _spriteContent = new GUIContent("Inactive Sprite", "The sprite while the indicator is not active\n" +
            "Leave empty to hide while inactive");

        private void OnEnable()
        {
            _aspectList = EditorHelper.CreateReorderableList(serializedObject, serializedObject.FindProperty(nameof(SignalDefinition.Aspects)),
                true, true, true, "Aspects");
            EditorHelper.AddBasicDrawerToList(_aspectList);

            _indicators = serializedObject.FindProperty(nameof(SignalDefinition.Indicators));
            _sprites = serializedObject.FindProperty(nameof(SignalDefinition.InactiveSprites));
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
                        EditorGUILayout.PropertyField(prop);
                        if (GUILayout.Button("Get Displays From Children"))
                        {
                            var displays = def.GetComponentsInChildren<DisplayBaseDefinition>().ToList();
                            var actuals = displays.OfType<MoveSwapDisplayDefinition>().Select(x => x.ActualDisplay).ToHashSet();
                            actuals.UnionWith(displays.OfType<AspectConditionalDisplayDefinition>().Select(x => x.ActualDisplay));
                            displays.RemoveAll(actuals.Contains);
                            def.Displays = displays.ToArray();
                            AssetHelper.SaveAsset(target);
                        }
                        break;
                        case nameof(SignalDefinition.Indicators):
                        DrawIndicators();
                        break;
                        case nameof(SignalDefinition.InactiveSprites):
                        // Skip this property as it is drawn with the previous one.
                        break;
                    default:
                        EditorGUILayout.PropertyField(prop);
                        break;
                }
            } while (prop.Next(false));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawIndicators()
        {
            _indicators.isExpanded = EditorGUILayout.Foldout(_indicators.isExpanded, "Indicators", true);

            if (!_indicators.isExpanded) return;

            EditorGUI.indentLevel++;

            int length = EditorGUILayout.DelayedIntField("Size", _indicators.arraySize);

            _indicators.arraySize = length;
            _sprites.arraySize = length;

            for (int i = 0; i < length; i++)
            {
                if (i > 0) EditorGUILayout.Space(4);

                EditorGUILayout.PropertyField(_indicators.GetArrayElementAtIndex(i));
                EditorGUILayout.PropertyField(_sprites.GetArrayElementAtIndex(i), _spriteContent);
            }

            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
        }
    }
}
