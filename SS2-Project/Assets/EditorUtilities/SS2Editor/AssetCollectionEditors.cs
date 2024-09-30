using MSU;
using RoR2;
using RoR2.Skills;
using RoR2.ExpansionManagement;
using RoR2.EntitlementManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HG;
using RoR2.Projectile;
using System.IO;
using Path = System.IO.Path;
using SS2;
using UnityEngine.Networking;
using System.Linq;
namespace Moonstorm.Starstorm2.Editor
{
    [CustomEditor(typeof(ExtendedAssetCollection))]
    public class AssetCollectionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginVertical("box");

            if (GUILayout.Button("Add Selected Objects"))
            {
                AddSelection();
            }
            //if (GUILayout.Button("Remove Duplicates"))
            //{
            //    RemoveDuplicates();
            //}
            EditorGUILayout.EndVertical();
        }

        private void AddSelection()
        {
            Undo.RecordObject(target, "Add selected assets to AssetCollection");


            UnityEngine.Object[] objects = Selection.objects;

            string path = GetSelectedFolderPath();
            if (!string.IsNullOrEmpty(path))
            {
                string[] guids = AssetDatabase.FindAssets("*", new string[] { path });
                List<UnityEngine.Object> folderObjects = new List<UnityEngine.Object>();
                foreach (string guid in guids)
                {
                    var pathh = AssetDatabase.GUIDToAssetPath(guid);
                    // LOADALLASSETSATPATH WOULDNT WORK IDKKKKKKKKKKKKKKKKKKKKKKKKKK
                    folderObjects.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(pathh));
                }
                objects = folderObjects.ToArray();
            }


            ExtendedAssetCollection assetCollection = (ExtendedAssetCollection)target;

            for (int i = 0; i < objects.Length; i++)
            {
                UnityEngine.Object ob = (UnityEngine.Object)objects[i];
                if (ob && IsAsset(ob) && !assetCollection.assets.Contains(ob)) //we O^50 in this bitch
                    ArrayUtils.ArrayInsert<UnityEngine.Object>(ref assetCollection.assets, 0, ob);
            }

            EditorUtility.SetDirty(target);
            //assetCollection.OnValidate(); // is this even right?

        }

        void RemoveDuplicates()
        {
            Undo.RecordObject(target, "Remove duplicate entries from AssetCollection");
            ExtendedAssetCollection assetCollection = (ExtendedAssetCollection)target;
            assetCollection.assets = assetCollection.assets.Distinct().ToArray();
            assetCollection.OnValidate();
        }

        public static string GetSelectedFolderPath()
        {
            string path = null;
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                string folder = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                {
                    path = Path.GetDirectoryName(folder) + @"\" + Path.GetFileName(folder);
                    break;
                }
            }
            return path;
        }

        public bool IsAsset(UnityEngine.Object asset)
        {
            switch (asset)
            {
                case GameObject gObject: return IsGameObjectAsset(gObject);
                case SkillDef sd:
                case SkillFamily sf:
                case SceneDef _sd:
                case ItemDef id:
                case ItemTierDef itd:
                case ItemRelationshipProvider irp:
                case ItemRelationshipType irt:
                case EquipmentDef ed:
                case BuffDef bd:
                case EliteDef _ed:
                case UnlockableDef ud:
                case SurvivorDef __sd:
                case ArtifactDef ad:
                case SurfaceDef ___sd:
                case NetworkSoundEventDef nsed:
                case MusicTrackDef mtd:
                case GameEndingDef ged:
                case EntityStateConfiguration esc:
                case ExpansionDef __ed:
                case EntitlementDef ___ed:
                case MiscPickupDef mpd:
                case EntityStateTypeCollection estc: return true;
            }
            return false;
        }
        public bool IsGameObjectAsset(GameObject go)
        {
            NetworkIdentity identity = go.GetComponent<NetworkIdentity>();
            bool isNetworkedByDefault = false;
            if (go.TryGetComponent<CharacterBody>(out var bodyComponent))
            {
                isNetworkedByDefault = true;
                return true;
            }
            if (go.TryGetComponent<CharacterMaster>(out var masterComponent))
            {
                isNetworkedByDefault = true;
                return true;
            }
            if (go.TryGetComponent<ProjectileController>(out var controllerComponent))
            {
                isNetworkedByDefault = true;
                return true;
            }
            if (go.TryGetComponent<Run>(out var runComponent))
            {
                isNetworkedByDefault = true;
                return true;
            }
            if (go.TryGetComponent<EffectComponent>(out var effectComponent))
            {
                return true;
            }
            if (identity && !isNetworkedByDefault)
            {
                return true;
            }
            return false;
        }
    }

    [CustomEditor(typeof(SurvivorAssetCollection))]
    public class SurvivorAssetCollectionEditor : AssetCollectionEditor
    {
    }
    [CustomEditor(typeof(MonsterAssetCollection))]
    public class MonsterAssetCollectionEditor : AssetCollectionEditor
    {
    }
    [CustomEditor(typeof(ArtifactAssetCollection))]
    public class ArtifactAssetCollectionEditor : AssetCollectionEditor
    {
    }
    [CustomEditor(typeof(EquipmentAssetCollection))]
    public class EquipmentAssetCollectionEditor : AssetCollectionEditor
    {
    }
    [CustomEditor(typeof(EliteAssetCollection))]
    public class EliteAssetCollectionEditor : AssetCollectionEditor
    {
    }
    [CustomEditor(typeof(InteractableAssetCollection))]
    public class InteractableAssetCollectionEditor : AssetCollectionEditor
    {
    }
    [CustomEditor(typeof(ItemAssetCollection))]
    public class ItemAssetCollectionEditor : AssetCollectionEditor
    {
    }

    [CustomEditor(typeof(SceneAssetCollection))]
    public class SceneAssetCollectionEditor : AssetCollectionEditor
    {
    }

    [CustomEditor(typeof(VanillaSurvivorAssetCollection))]
    public class VanillaSurvivorAssetCollectionEditor : AssetCollectionEditor
    {

    }
}