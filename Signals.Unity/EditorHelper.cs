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
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, header);
                }
            };
        }
    }
}
