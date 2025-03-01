using Signals.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Signals.Unity.Utilities
{
    internal class SequencePreview : EditorWindow
    {
        private static Vector2 s_minSize = new Vector2(200, 200);

        private SignalLightSequenceDefinition? _def;
        private Vector2 _scroll = Vector2.zero;

        public static void ShowWindow(SignalLightSequenceDefinition definition)
        {
            var window = GetWindow<SequencePreview>();
            window._def = definition;
            window.minSize = s_minSize;
            window.titleContent = new GUIContent("Sequence Preview");
            window.Show();
        }

        private void OnGUI()
        {
            if (_def == null)
            {
                EditorGUILayout.LabelField("No sequence set!");
                return;
            }

            Dictionary<SignalLightDefinition, List<bool>> lights = _def.Lights.Where(x => x != null).Distinct().ToDictionary(k => k!, v => new List<bool>());

            int offset = 0;
            int length = _def.Lights.Length;
            int step = _def.Step;
            HashSet<SignalLightDefinition> changedThisFrame = new HashSet<SignalLightDefinition>();

            do
            {
                changedThisFrame.Clear();

                for (int i = 0; i < length; i++)
                {
                    var light = _def.Lights[(offset + i) % length];

                    if (light == null) continue;

                    if (changedThisFrame.Contains(light))
                    {
                        lights[light][lights[light].Count - 1] = _def.States[i];
                    }
                    else
                    {
                        lights[light].Add(_def.States[i]);
                        changedThisFrame.Add(light);
                    }
                }

                offset = (offset + step) % length;
            } while (offset != 0);

            EditorGUILayout.LabelField("Sequences (● = ON / ○ = OFF)");

            using var scrollView = new EditorGUILayout.ScrollViewScope(_scroll);

            _scroll = scrollView.scrollPosition;

            foreach (var light in lights.Values)
            {
                EditorGUILayout.LabelField(BuildOutput(light));
            }
        }

        private static string BuildOutput(List<bool> states)
        {
            StringBuilder output = new StringBuilder();

            foreach (bool state in states)
            {
                if (state)
                {
                    output.Append("●        ");
                }
                else
                {
                    output.Append("○        ");
                }
            }

            return output.ToString();
        }
    }
}
