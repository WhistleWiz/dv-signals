using UnityEditor;
using UnityEngine;

namespace Signals.Unity
{
    internal static class AssetHelper
    {
        public static void SaveAsset(Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
    }
}
