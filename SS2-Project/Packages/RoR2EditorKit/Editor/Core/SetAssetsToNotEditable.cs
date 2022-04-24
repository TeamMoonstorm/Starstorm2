using RoR2EditorKit.Common;
using RoR2EditorKit.Settings;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Data;

namespace RoR2EditorKit.Core
{
    internal static class SetAssetsToNotEditable
    {
        private static RoR2EditorKitSettings Settings { get => RoR2EditorKitSettings.GetOrCreateSettings<RoR2EditorKitSettings>(); }
        [InitializeOnLoadMethod]
        private static void Init()
        {
            if(IsRoR2EKInPackages() && !Settings.madeRoR2EKAssetsNonEditable)
            {
                MakeAssetsUneditable();
            }
        }

        private static bool IsRoR2EKInPackages()
        {
            string iconPath = AssetDatabase.GetAssetPath(Constants.Icon);
            return iconPath.StartsWith(Constants.PackageFolderPath);
        }

        private static void MakeAssetsUneditable()
        {

            AssetDatabase.SaveAssets();
            var iconPath = AssetDatabase.GetAssetPath(Constants.Icon);
            var assetsFolder = Path.GetDirectoryName(iconPath);
            var folderAsset = AssetDatabase.LoadAssetAtPath<Object>(assetsFolder);

            var explicitAssetPaths = new List<string>();

            PopulateWithExplicitAssets(new List<Object> { folderAsset }, explicitAssetPaths);

            var stringBuilder = new List<string>();

            stringBuilder.Add($"Turned the following {explicitAssetPaths.Count} assets to notEditable.");

            foreach(string assetPath in explicitAssetPaths)
            {
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (!asset)
                    continue;

                asset.hideFlags = HideFlags.NotEditable;
                new SerializedObject(asset).ApplyModifiedProperties();
                stringBuilder.Add(asset.name);
            }
            Debug.Log(string.Join("\n", stringBuilder));
            Settings.madeRoR2EKAssetsNonEditable = true;
            new SerializedObject(Settings).ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        private static void PopulateWithExplicitAssets(IEnumerable<Object> inputAssets, List<string> outputAssets)
        {
            foreach (var asset in inputAssets)
            {
                var assetPath = AssetDatabase.GetAssetPath(asset);

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    var files = Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories);
                    var assets = files.Select(path => AssetDatabase.LoadAssetAtPath<Object>(path));
                    PopulateWithExplicitAssets(assets, outputAssets);
                }
                else if (asset is UnityPackage up)
                {
                    PopulateWithExplicitAssets(up.AssetFiles, outputAssets);
                }
                else
                {
                    outputAssets.Add(assetPath);
                }
            }
        }
    }
}
