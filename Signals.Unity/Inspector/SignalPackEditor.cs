using Signals.Common;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Signals.Unity.Inspector
{
    [CustomEditor(typeof(SignalPack))]
    internal class SignalPackEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (GUILayout.Button("Export mod"))
            {
                var pack = (SignalPack)target;

                if (!pack.Validate())
                {
                    EditorUtility.DisplayDialog("Error exporting!", "Could not export the pack; errors are in the unity console.", "I will fix them");
                    return;
                }

                var path = Export(pack);
                Debug.Log($"[{System.DateTime.Now:HH:mm:ss}] Pack exported!");
                GUIUtility.ExitGUI();
                EditorUtility.RevealInFinder(path);
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
