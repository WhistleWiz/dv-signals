using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Signals.Unity
{
    internal static class EditorHelper
    {
        public static ReorderableList CreateReorderableList(SerializedObject obj, SerializedProperty elements,
            bool draggable, bool displayHeader, bool displayButtons, string header)
        {
            return new ReorderableList(obj, elements, draggable, displayHeader, displayButtons, displayButtons)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, header);
                }
            };
        }

        public static void AddBasicDrawerToList(ReorderableList list)
        {
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.ObjectField(rect, element, GUIContent.none);
            };
        }

        public static void DrawHeader(string title)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        public static GUIStyle StyleWithTextColour(Color c, GUIStyle original)
        {
            return new GUIStyle(original) { normal = new GUIStyleState() { textColor = c } };
        }
    }
}
