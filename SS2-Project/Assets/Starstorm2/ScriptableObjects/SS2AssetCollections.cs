using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.Skills;
using RoR2.ExpansionManagement;
using RoR2.EntitlementManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using HG;
using UnityEngine.Networking;
using RoR2.Projectile;
using System.IO;
using Path = System.IO.Path;
using System.Linq;
namespace SS2
{
    public class ExtendedAssetCollection : AssetCollection
    {
        public void OnValidate()
        {
            //very slow lol

            //ensure field values are contained in AssetCollection.assets
            foreach (FieldInfo f in this.GetType().GetFields())
            {
                object val = f.GetValue(this);
                if (val == null || (val != null && !(val is UnityEngine.Object))) continue;
                UnityEngine.Object obj = (UnityEngine.Object)val; // "specified cast is not valid" yes it is dumbfuck
                if (!obj) continue;
                bool contains = false;
                for (int i = 0; i < assets.Length; i++)
                {
                    if (assets[i] == obj)
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                {
                    ArrayUtils.ArrayInsert<UnityEngine.Object>(ref this.assets, 0, obj);
                }
            }

            //ensure all assets are distinct
            List<UnityEngine.Object> objects = new List<UnityEngine.Object>();           
            foreach(UnityEngine.Object obj in this.assets)
            {
                bool contains = false;
                foreach (UnityEngine.Object obj2 in objects)
                {
                    if (obj == obj2)//AssetDatabase.GetAssetPath(obj).Equals(AssetDatabase.GetAssetPath(obj2)))
                    {
                        contains = true;
                        break;
                    }
                }
                    
                if (!contains) objects.Add(obj);
            }
            this.assets = objects.ToArray();
        }
    }

    [CreateAssetMenu(fileName = "BodyAssetCollection", menuName = "Starstorm2/AssetCollections/BodyAssetCollection")]
    public class BodyAssetCollection : ExtendedAssetCollection
    {
        public GameObject bodyPrefab;
        public GameObject masterPrefab;

        
    }
    [CreateAssetMenu(fileName = "SurvivorAssetCollection", menuName = "Starstorm2/AssetCollections/SurvivorAssetCollection")]
    public class SurvivorAssetCollection : BodyAssetCollection
    {
        public SurvivorDef survivorDef;
    }
    [CreateAssetMenu(fileName = "MonsterAssetCollection", menuName = "Starstorm2/AssetCollections/MonsterAssetCollection")]
    public class MonsterAssetCollection : BodyAssetCollection
    {
        public MonsterCardProvider monsterCardProvider;
        public R2API.DirectorAPI.DirectorCardHolder dissonanceCardHolder;
    }
    [CreateAssetMenu(fileName = "ArtifactAssetCollection", menuName = "Starstorm2/AssetCollections/ArtifactAssetCollection")]
    public class ArtifactAssetCollection : ExtendedAssetCollection
    {
        public ArtifactCode artifactCode;
        public ArtifactDef artifactDef;
    }
    [CreateAssetMenu(fileName = "EquipmentAssetCollection", menuName = "Starstorm2/AssetCollections/EquipmentAssetCollection")]
    public class EquipmentAssetCollection : ExtendedAssetCollection
    {
        public List<GameObject> itemDisplayPrefabs;
        public EquipmentDef equipmentDef;
    }
    [CreateAssetMenu(fileName = "EliteAssetCollection", menuName = "Starstorm2/AssetCollections/EliteAssetCollection")]
    public class EliteAssetCollection : EquipmentAssetCollection
    {
        public List<EliteDef> eliteDefs;
    }
    [CreateAssetMenu(fileName = "InteractableAssetCollection", menuName = "Starstorm2/AssetCollections/InteractableAssetCollection")]
    public class InteractableAssetCollection : ExtendedAssetCollection
    {
        public GameObject interactablePrefab;
        public InteractableCardProvider interactableCardProvider;
    }
    [CreateAssetMenu(fileName = "ItemAssetCollection", menuName = "Starstorm2/AssetCollections/ItemAssetCollection")]
    public class ItemAssetCollection : ExtendedAssetCollection
    {
        public List<GameObject> itemDisplayPrefabs;
        public ItemDef itemDef;
    }
    [CreateAssetMenu(fileName = "ItemTierAssetCollection", menuName = "Starstorm2/AssetCollections/ItemTierAssetCollection")]
    public class ItemTierAssetCollection : ExtendedAssetCollection
    {

    }


    [CustomEditor(typeof(ExtendedAssetCollection))]
    public class AssetCollectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.BeginVertical("box");

            if (GUILayout.Button("Add Selected Objects"))
            {
                AddSelection();
                
            }
            EditorGUILayout.EndVertical();
        }

        private void AddSelection()
        {
            Undo.RecordObject(target, "Add selected assets to AssetCollection");

            UnityEngine.Object[] objects = Selection.objects;

            string path = GetSelectedFolderPath();
            if(!string.IsNullOrEmpty(path))
            {
                Debug.Log("FOLDER: " + path);

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
            
            
            AssetCollection assetCollection = (AssetCollection)target;

            for (int i = 0; i < objects.Length; i++)
            {
                UnityEngine.Object ob = (UnityEngine.Object)objects[i];
                if(ob && IsAsset(ob))
                    ArrayUtils.ArrayInsert<UnityEngine.Object>(ref assetCollection.assets, 0, ob);
            }

            (target as ExtendedAssetCollection).OnValidate(); // is this even right?

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

}
