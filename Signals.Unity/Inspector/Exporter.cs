using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Signals.Common;
using Signals.Unity.Validation;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    internal class Exporter : EditorWindow
    {
        private class SignalResults
        {
            public string Name;
            public bool Expanded = true;
            public List<Result> Results;
            public SignalControllerDefinition? Prefab;

            public SignalResults(string name, SignalControllerDefinition? prefab = null)
            {
                Name = name;
                Results = new List<Result>();
                Prefab = prefab;
            }
        }

        private class TableWidths
        {
            public const float Padding = 4.0f;

            public static readonly GUILayoutOption IconWidth = GUILayout.Width(18);

            public float Name = 30.0f;
            public float Result = 20.0f;
            public float Message = 100.0f;

            public void Reset()
            {
                Name = 30.0f;
                Result = 20.0f;
                Message = 100.0f;
            }

            public void ResizeName(float value)
            {
                Name = Mathf.Max(Name, value);
            }

            public void ResizeName(string text)
            {
                ResizeName(TextSize(text));
            }

            public void ResizeValue(float value)
            {
                Result = Mathf.Max(Result, value);
            }

            public void ResizeValue(string text)
            {
                ResizeValue(TextSize(text));
            }

            public void ResizeMessage(float value)
            {
                Message = Mathf.Max(Message, value);
            }

            public void ResizeMessage(string text)
            {
                ResizeMessage(TextSize(text));
            }

            public GUILayoutOption[] ToOptions()
            {
                return new[] { GUILayout.Width(Name), GUILayout.Width(Result), GUILayout.Width(Message) };
            }

            public static float TextSize(string text) => EditorStyles.label.CalcSize(new GUIContent(text)).x + Padding;
        }

        public static void Open(SignalPack pack)
        {
            var window = GetWindow<Exporter>();
            window.titleContent = new GUIContent("Exporter");
            window._pack = pack;
            window.DoValidation();
        }

        private Vector2 _scroll = Vector2.zero;
        private SignalPack? _pack;
        private List<SignalResults> _results = new List<SignalResults>();
        private TableWidths _widths = new TableWidths();
        private bool _hasErrors = false;

        private void OnGUI()
        {
            if (_pack == null || _results.Count == 0)
            {
                EditorGUILayout.LabelField("Environment was refreshed, please export again.");
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            var oldReached = false;
            var otherReached = false;

            foreach (var result in _results)
            {
                if (!oldReached && result.Name.StartsWith("Old"))
                {
                    oldReached = true;
                    EditorHelper.DrawSeparator();
                }

                if (!otherReached && result.Name.StartsWith("Other"))
                {
                    otherReached = true;
                    EditorHelper.DrawSeparator();
                }

                DrawResults(result, _widths.ToOptions());
            }

            EditorGUILayout.EndScrollView();

            if (_hasErrors )
            {
                EditorGUILayout.LabelField("Failures detected while exporting signals!");
            }
            else
            {
                EditorGUILayout.LabelField("All good for exporting!");
            }

            EditorGUILayout.Space();

            GUI.enabled = !_hasErrors;

            if (GUILayout.Button("Export mod") && !_hasErrors)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var path = Export(_pack);
                sw.Stop();
                Debug.Log($"[{System.DateTime.Now:HH:mm:ss}] Pack exported! ({sw.Elapsed.TotalSeconds:F4})");
                GUIUtility.ExitGUI();
                EditorUtility.RevealInFinder(path);
            }

            GUI.enabled = true;
        }

        private void DrawResults(SignalResults results, GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            results.Expanded = EditorGUILayout.BeginFoldoutHeaderGroup(results.Expanded, results.Name);

            if (!results.Expanded) goto End;

            foreach (var result in results.Results)
            {
                foreach (var entry in result.Entries)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(result.Name, options[0]);
                    EditorGUILayout.LabelField(entry.Status.ToString(),
                        EditorHelper.StyleWithTextColour(GetStatusColor(entry.Status), GUI.skin.label), options[1]);
                    EditorGUILayout.LabelField(entry.Message, options[2]);
                    EditorGUILayout.EndHorizontal();
                }
            }

        End:
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }

        private void DoValidation()
        {
            _results.Clear();
            _hasErrors = false;

            if (_pack == null) return;

            if (_pack.Signal == null)
            {
                var entry = new Result("Controller");
                entry.AddCritical("Main controller cannot be null");
                var result = new SignalResults(name);
                result.Results.Add(entry);
                _results.Add(result);
                _hasErrors = true;
                goto Widths;
            }

            ValidateController(_pack.Signal, "Main Signal");

            ValidateController(_pack.DivergingSignal, "Diverging Signal");
            ValidateController(_pack.LeftJunctionSignal, "Left Junction Signal");
            ValidateController(_pack.RightJunctionSignal, "Right Junction Signal");
            ValidateController(_pack.EntrySignal, "Entry Signal");
            ValidateController(_pack.ExitSignal, "Exit Signal");
            ValidateController(_pack.ExitPassengerSignal, "Exit Passenger Signal");
            ValidateController(_pack.ExitMainlineSignal, "Exit Mainline Signal");

            ValidateController(_pack.ShuntingSignal, "Shunting Signal");
            ValidateController(_pack.MajorShuntingSignal, "Major Shunting Signal");
            ValidateController(_pack.JunctionShuntingSignal, "Junction Shunting Signal");
            ValidateController(_pack.SpacingSignal, "Spacing Signal");
            ValidateController(_pack.TurntableSignal, "Turntable Signal");
            ValidateController(_pack.BufferStopSignal, "Buffer Stop Signal");

            ValidateController(_pack.DistantSignal, "Distant Signal");
            ValidateController(_pack.RepeaterSignal, "Repeater Signal");

            ValidateController(_pack.CombinedSignal, "Combined Main Signal");
            ValidateController(_pack.CombinedLeftJunctionSignal, "Combined Left Junction Signal");
            ValidateController(_pack.CombinedRightJunctionSignal, "Combined Right Junction Signal");

            ValidateController(_pack.OldSignal, "Old Main Signal");

            ValidateController(_pack.OldDivergingSignal, "Old Diverging Signal");
            ValidateController(_pack.OldLeftJunctionSignal, "Old Left Junction Signal");
            ValidateController(_pack.OldRightJunctionSignal, "Old Right Junction Signal");
            ValidateController(_pack.OldEntrySignal, "Old Entry Signal");
            ValidateController(_pack.OldExitSignal, "Old Exit Signal");
            ValidateController(_pack.OldExitPassengerSignal, "Old Exit Passenger Signal");
            ValidateController(_pack.OldExitMainlineSignal, "Old Exit Mainline Signal");

            ValidateController(_pack.OldShuntingSignal, "Old Shunting Signal");
            ValidateController(_pack.OldMajorShuntingSignal, "Old Major Shunting Signal");
            ValidateController(_pack.OldJunctionShuntingSignal, "Old Junction Shunting Signal");
            ValidateController(_pack.OldSpacingSignal, "Old Spacing Signal");
            ValidateController(_pack.OldTurntableSignal, "Old Turntable Signal");
            ValidateController(_pack.OldBufferStopSignal, "Old Buffer Stop Signal");

            ValidateController(_pack.OldDistantSignal, "Old Distant Signal");
            ValidateController(_pack.OldRepeaterSignal, "Old Repeater Signal");

            ValidateController(_pack.OldCombinedSignal, "Old Combined Main Signal");
            ValidateController(_pack.OldCombinedLeftJunctionSignal, "Old Combined Left Junction Signal");
            ValidateController(_pack.OldCombinedRightJunctionSignal, "Old Combined Right Junction Signal");

            for (int i = 0; i < _pack.OtherSignals.Length; i++)
            {
                // Instead of a skip, throw an error.
                if (_pack.OtherSignals[i] == null)
                {
                    var result = new Result("Controller");
                    result.AddFailure("Controller is null");
                    var results = new SignalResults($"Other {i}");
                    results.Results.Add(result);
                    _results.Add(results);
                    _hasErrors = true;
                    continue;
                }

                ValidateController(_pack.OtherSignals[i], $"Other {i}");
            }

        Widths:

            _widths.Reset();

            foreach (var item in _results)
            {
                foreach (var result in item.Results)
                {
                    _widths.ResizeName(result.Name);

                    foreach (var entry in result.Entries)
                    {
                        _widths.ResizeMessage(entry.Message);
                    }
                }
            }

            foreach (var value in System.Enum.GetNames(typeof(Status)))
            {
                _widths.ResizeValue(value);
            }
        }

        private void ValidateController(SignalControllerDefinition? controller, string name)
        {
            // No need to validate the same controller multiple times.
            if (controller != null)
            {
                var matching = _results.Find(x => x.Prefab == controller);

                if (matching != null)
                {
                    matching.Name = $"{matching.Name} / {name}";
                    return;
                }
            }

            var results = new SignalResults(name, controller);
            _results.Add(results);

            if (controller == null)
            {
                results.Expanded = false;
                results.Results.Add(Result.Skip("Controller"));
                return;
            }

            foreach (var validator in GetValidators())
            {
                var result = validator.ValidateController(controller);

                results.Results.Add(result);

                if (result.Status == Status.Critical) break;
            }

            _hasErrors |= results.Results.Any(x => x.Status > Status.Warning);
        }

        private IEnumerable<IValidatorBase> GetValidators()
        {
            yield return new ControllerValidator();
            yield return new SignalValidator();
            yield return new AspectValidator();
            yield return new DisplayValidator();
            yield return new ComponentsValidator();
        }

        private static string Export(SignalPack pack)
        {
            using var memoryStream = new MemoryStream();
            var fileName = pack.ModId;
            var path = AssetBundleHelper.GetFullPath(pack);

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                // Create Info.json from pack.
                JsonSerializer serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };

                var file = archive.CreateEntry($"{fileName}/{Constants.ModInfo}");

                using (var entryStream = file.Open())
                using (var streamWriter = new StreamWriter(entryStream))
                using (var jsonWr = new JsonTextWriter(streamWriter))
                {
                    serializer.Serialize(jsonWr, GetModInfo(pack));
                }

                // Create the asset bundle.
                var bundlePath = AssetBundleHelper.CreateBundle(path, AssetBundleHelper.GetAssetPath(pack),
                    new List<(Object, string?)> { (pack, null) });

                // Only add the bundle itself to the zip.
                // Meta files and manifests aren't needed.
                file = archive.CreateEntryFromFile(bundlePath, $"{fileName}/{Path.GetFileName(bundlePath)}");

                // Delete the bundle files, leaving only the one already inside the zip.
                AssetBundleHelper.DeleteBundle(path, bundlePath);
            }

            var outputPath = Path.Combine(path,
                $"{fileName}.zip");

            using (var fileStream = new FileStream(outputPath, FileMode.Create))
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                memoryStream.CopyTo(fileStream);
            }

            AssetDatabase.Refresh();

            return outputPath;
        }

        private static JObject GetModInfo(SignalPack pack)
        {
            List<string> reqs = new List<string> { Constants.MainModId };

            var modInfo = new JObject
            {
                { "Id", pack.ModId },
                { "DisplayName", pack.ModName },
                { "Version", pack.Version },
                { "Author", pack.Author },
                { "ManagerVersion", "0.27.3" },
                { "Requirements", JToken.FromObject(reqs.ToArray()) },
            };

            // If a homepage was defined, also add the link.
            if (!string.IsNullOrEmpty(pack.HomePage))
            {
                modInfo.Add("HomePage", pack.HomePage);
            }

            // If a repository was defined, also add the link.
            if (!string.IsNullOrEmpty(pack.Repository))
            {
                modInfo.Add("Repository", pack.Repository);
            }

            return modInfo;
        }

        private static Color GetStatusColor(Status status) => status switch
        {
            Status.Pass => Color.green,
            Status.Warning => Color.yellow,
            Status.Failure => Color.red,
            Status.Critical => Color.red,
            _ => Color.grey,
        };
    }
}
