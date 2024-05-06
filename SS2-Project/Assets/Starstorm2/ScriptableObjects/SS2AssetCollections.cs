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
namespace SS2
{
    //todo: a lot
    [CreateAssetMenu(fileName = "BodyAssetCollection", menuName = "Starstorm2/AssetCollections/BodyAssetCollection")]
    public class BodyAssetCollection : AssetCollection
    {
        public GameObject bodyPrefab;
        public GameObject masterPrefab;

        public void OnValidate()
        {
            //todo: ensure field values are also contained in AssetCollection.assets
        }
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
    }
    [CreateAssetMenu(fileName = "ArtifactAssetCollection", menuName = "Starstorm2/AssetCollections/ArtifactAssetCollection")]
    public class ArtifactAssetCollection : AssetCollection
    {
        public ArtifactCode artifactCode;
        public ArtifactDef artifactDef;
    }
    [CreateAssetMenu(fileName = "EquipmentAssetCollection", menuName = "Starstorm2/AssetCollections/EquipmentAssetCollection")]
    public class EquipmentAssetCollection : AssetCollection
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
    public class InteractableAssetCollection : AssetCollection
    {
        public GameObject interactablePrefab;
        public InteractableCardProvider interactableCardProvider;
    }
    [CreateAssetMenu(fileName = "ItemAssetCollection", menuName = "Starstorm2/AssetCollections/ItemAssetCollection")]
    public class ItemAssetCollection : AssetCollection
    {
        public List<GameObject> itemDisplayPrefabs;
        public ItemDef itemDef;
    }
    [CreateAssetMenu(fileName = "ItemTierAssetCollection", menuName = "Starstorm2/AssetCollections/ItemTierAssetCollection")]
    public class ItemTierAssetCollection : AssetCollection
    {

    }


    [CustomEditor(typeof(AssetCollection))]
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
            //todo: add folder selection and actually sort objects in selection (reuse contentutils.addcontentfromassetcollection)
            UnityEngine.Object[] objects = Selection.objects;
            AssetCollection assetCollection = (AssetCollection)target;

            for (int i = 0; i < objects.Length; i++)
            {
                UnityEngine.Object ob = (UnityEngine.Object)objects[i];
                HGArrayUtilities.ArrayInsert<UnityEngine.Object>(ref assetCollection.assets, 0, ref ob);
            }
            Debug.Log($"wawa");
        }
    }
}
