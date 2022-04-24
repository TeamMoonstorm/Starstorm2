using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Common
{
    /// <summary>
    /// Class filled with constants to use for asset creation or attributes
    /// </summary>
    public static class Constants
    {
        public const string RoR2EditorKit = nameof(RoR2EditorKit);
        public const string AssetFolderPath = "Assets/RoR2EditorKit";
        public const string PackageFolderPath = "Packages/riskofthunder-ror2editorkit";

        public const string RoR2EditorKitContextRoot = "Assets/Create/RoR2EditorKit/";
        public const string RoR2EditorKitscriptableRoot = "Assets/RoR2EditorKit/";
        public const int RoR2EditorKitContextPriority = 999999999;
        public const string RoR2EditorKitMenuRoot = "Tools/RoR2EditorKit/";
        public const string RoR2KitSettingsRoot = "Assets/ThunderkitSettings/RoR2EditorKit/";

        private const string nullMaterialGUID = "732339a737ef9a144812666d298e2357";
        private const string nullMeshGUID = "9bef9cd9cd0c4b244ad1ff166c26f57e";
        private const string nullSpriteGUID = "1a8e7e70058f32f4483753ec5be3838b";
        private const string nullPrefabGUID = "f6317a68216520848aaef2c2f470c8b2";
        private const string iconGUID = "efa2e3ecb36780a4d81685ecd4789ff3";

        /// <summary>
        /// Loads the RoR2EditorKit null material
        /// </summary>
        public static Material NullMaterial { get => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(nullMaterialGUID)); }

        /// <summary>
        /// Loads the RoR2EditorKit null mesh
        /// </summary>
        public static Mesh NullMesh { get => AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(nullMeshGUID)); }

        /// <summary>
        /// Loads the RoR2EditorKit null sprite
        /// </summary>
        public static Sprite NullSprite { get => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(nullSpriteGUID)); }

        /// <summary>
        /// Loads the RoR2EditorKit null prefab
        /// </summary>
        public static GameObject NullPrefab { get => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(nullPrefabGUID)); }

        public static Texture Icon { get => AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(iconGUID)); }
    }
}
