using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Utilities
{
    public static class AssetDatabaseUtils
    {
        /// <summary>
        /// Finds all assets of Type T
        /// </summary>
        /// <typeparam name="T">The Type of asset to find</typeparam>
        /// <param name="assetNameFilter">A filter to narrow down the search results</param>
        /// <returns>An IEnumerable of all the Types found inside the AssetDatabase.</returns>
        public static IEnumerable<T> FindAssetsByType<T>(string assetNameFilter = null) where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids;
            if (assetNameFilter != null)
                guids = AssetDatabase.FindAssets($"{assetNameFilter} t:{typeof(T).Name}", null);
            else
                guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", null);
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                    assets.Add(asset);
            }
            return assets;
        }

        /// <summary>
        /// Finds an asset of Type T
        /// </summary>
        /// <typeparam name="T">The Type of asset to find</typeparam>
        /// <param name="assetNameFilter">A filter to narrow down the search results</param>
        /// <returns>The asset found</returns>
        public static T FindAssetByType<T>(string assetNameFilter = null) where T : UnityEngine.Object
        {
            string[] guids;

            if (assetNameFilter != null)
                guids = AssetDatabase.FindAssets($"{assetNameFilter} t{typeof(T).Name}", null);
            else
                guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", null);

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids.First()));
        }

        /// <summary>
        /// Creates a generic asset at the currently selected folder
        /// </summary>
        /// <param name="asset">The asset to create</param>
        /// <returns>The Created asset</returns>
        public static Object CreateAssetAtSelectionPath(Object asset)
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            path = AssetDatabase.GenerateUniqueAssetPath($"{path}/{asset.name}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();

            return asset;
        }

        /// <summary>
        /// Creates a prefab at the currently selected folder
        /// </summary>
        /// <param name="asset">The prefab to create</param>
        /// <returns>The newely created prefab in the AssetDatabase</returns>
        public static GameObject CreatePrefabAtSelectionPath(GameObject asset)
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            path = AssetDatabase.GenerateUniqueAssetPath($"{path}/{asset.name}.prefab");
            return PrefabUtility.SaveAsPrefabAsset(asset, path);
        }

        public static void UpdateNameOfObject(Object obj)
        {
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(obj), obj.name);
        }
    }
}
