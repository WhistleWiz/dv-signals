using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Signals.Common;
using Signals.Common.Aspects;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Signals.Unity.Inspector
{
    internal class Validator : EditorWindow
    {
        public static void Open(SignalPack pack)
        {
            var window = GetWindow<Validator>();
            window._pack = pack;
            window.DoValidation();
        }

        private SignalPack? _pack;
        private List<string> _errors = new List<string>();
        private Vector2 _scroll = Vector2.zero;

        private void OnGUI()
        {
            if (_pack == null)
            {
                EditorGUILayout.LabelField("Environment was refreshed, please reexport again.");
                return;
            }

            if (_errors.Count > 0)
            {
                EditorGUILayout.LabelField("Failures detected while exporting signals!");
                _scroll = EditorGUILayout.BeginScrollView(_scroll);

                foreach (var error in _errors)
                {
                    EditorGUILayout.LabelField(error);
                }

                EditorGUILayout.EndScrollView();
                return;
            }

            EditorGUILayout.LabelField("All good for exporting!");
            EditorGUILayout.Space();

            if (GUILayout.Button("Export mod"))
            {
                var sw = new System.Diagnostics.Stopwatch();
                var path = Export(_pack);
                sw.Stop();
                Debug.Log($"[{System.DateTime.Now:HH:mm:ss}] Pack exported! ({sw.Elapsed.TotalSeconds:F4})");
                GUIUtility.ExitGUI();
                EditorUtility.RevealInFinder(path);
            }
        }

        private void DoValidation()
        {
            if (_pack == null) return;

            _errors.Clear();

            if (_pack.Signal == null)
            {
                _errors.Add("Main signal is missing!");
                return;
            }

            foreach (var signal in _pack.AllSignals)
            {
                CheckController(signal);
            }
        }

        private void CheckController(SignalControllerDefinition signal)
        {
            for (int i = 0; i < signal.Aspects.Length; i++)
            {
                var aspect = signal.Aspects[i];

                if (aspect == null)
                {
                    _errors.Add($"{signal.name} - aspect {i} is null");
                    continue;
                }

                CheckAspect(aspect, $"{signal.name}/Aspect {i}");
            }

            for (int i = 0; i < signal.Displays.Length; i++)
            {
                var display = signal.Displays[i];

                if (display == null)
                {
                    _errors.Add($"{signal.name} - display {i} is null");
                    continue;
                }
            }

            //for (int i = 0; i < signal.Subsignals.Length; i++)
            //{
            //    var subsignal = signal.Subsignals[i];

            //    if (subsignal == null)
            //    {
            //        _errors.Add($"{signal.name} - subsignal {i} is null");
            //        continue;
            //    }

            //    CheckSubsignal(subsignal, signal.name);
            //}
        }

        private void CheckSubsignal(SubsignalControllerDefinition subsignal, string path)
        {
            path = $"{path}/{subsignal.name}";

            for (int i = 0; i < subsignal.Aspects.Length; i++)
            {
                var aspect = subsignal.Aspects[i];

                if (aspect == null)
                {
                    _errors.Add($"{path} - aspect {i} is null");
                    continue;
                }

                CheckAspect(aspect, $"{path}/Aspect {i}");
            }

            for (int i = 0; i < subsignal.Displays.Length; i++)
            {
                var display = subsignal.Displays[i];

                if (display == null)
                {
                    _errors.Add($"{path} - display {i} is null");
                    continue;
                }
            }
        }

        private void CheckAspect(AspectBaseDefinition aspect, string path)
        {
            if (aspect.OnLights.Any(x => x == null))
            {
                _errors.Add($"{path} - On Lights has null entries");
            }

            if (aspect.BlinkingLights.Any(x => x == null))
            {
                _errors.Add($"{path} - Blinking Lights has null entries");
            }

            if (aspect.LightSequences.Any(x => x == null))
            {
                _errors.Add($"{path} - Light Sequences has null entries");
            }
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
    }
}
