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

            this.assets = this.assets.Distinct().ToArray();
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

}
