using System;
using System.Collections.Generic;
using RoR2.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2;
namespace SS2
{
    // has a desired item and a list of pickups it can give in return
    [CreateAssetMenu(fileName = "TradeDef", menuName = "Starstorm2/TradeDef")]
    public class TradeDef : ScriptableObject
    {
        public ItemDef desiredItem;
        public int maxOptions = 3;
        public TradeOption[] options;

        public PickupIndex[] GenerateUniquePickups(Xoroshiro128Plus rng)
        {          
            return GenerateUniquePickups(this.maxOptions, rng);
        }
        public PickupIndex[] GenerateUniquePickups(int maxDrops, Xoroshiro128Plus rng)
        {
            if (options.Length <= maxDrops) // dont bother making weighted selection if we dont need to
            {
                PickupIndex[] pickups = new PickupIndex[options.Length];
                for (int i = 0; i < options.Length; i++)
                {
                    pickups[i] = options[i].pickupIndex;
                }
                return pickups;
            }
            WeightedSelection<PickupIndex> selector = new WeightedSelection<PickupIndex>();
            foreach (TradeOption option in options)
            {
                if (Run.instance && Run.instance.IsPickupAvailable(option.pickupIndex))
                {
                    selector.AddChoice(option.pickupIndex, option.weight);
                }
            }
            PickupIndex[] array = GenerateUniquePickups(maxDrops, rng, selector);
            return array;
        }

        protected static PickupIndex[] GenerateUniquePickups(int maxDrops, Xoroshiro128Plus rng, WeightedSelection<PickupIndex> weightedSelection)
        {
            int num = Math.Min(maxDrops, weightedSelection.Count);
            int[] array = Array.Empty<int>();
            PickupIndex[] array2 = new PickupIndex[num];
            for (int i = 0; i < num; i++)
            {
                int choiceIndex = weightedSelection.EvaluateToChoiceIndex(rng.nextNormalizedFloat, array);
                WeightedSelection<PickupIndex>.ChoiceInfo choice = weightedSelection.GetChoice(choiceIndex);
                weightedSelection.RemoveChoice(choiceIndex);
                array2[i] = choice.value;
                Array.Resize<int>(ref array, i + 1);
                array[i] = choiceIndex;
            }
            return array2;
        }

        #region Init
        public void InitSingle()
        {
            for (int i = 0; i < options.Length; i++)
            {
                TradeOption option = options[i];
                UnityEngine.Object pickupDef = option.pickupDef;
                if (pickupDef is ItemDef itemDef)
                {
                    option.pickupIndex = PickupCatalog.FindPickupIndex(itemDef.itemIndex);                 
                }
                else if(pickupDef is EquipmentDef equipmentDef)
                {
                    option.pickupIndex = PickupCatalog.FindPickupIndex(equipmentDef.equipmentIndex);
                }
                else if(pickupDef is ArtifactDef artifactDef)
                {
                    option.pickupIndex = PickupCatalog.FindPickupIndex(artifactDef.artifactIndex);
                }
                else if(pickupDef is MiscPickupDef miscPickupDef)
                {
                    option.pickupIndex = PickupCatalog.FindPickupIndex(miscPickupDef.miscPickupIndex);
                }
                if (option.pickupIndex == PickupIndex.none && !string.IsNullOrWhiteSpace(option.pickupName))
                {
                    option.pickupIndex = PickupCatalog.FindPickupIndex(option.pickupName);
                }
                if (option.pickupIndex == PickupIndex.none)
                {
                    SS2Log.Error($"TradeDef {base.name} has invalid PickupDef");
                }
            }
        }

        [SystemInitializer(typeof(PickupCatalog))]
        private static void Init()
        {
            foreach(TradeDef trade in instances)
            {
                trade.InitSingle();
            }
        }
        private void OnEnable()
        {
            instances.Add(this);
        }
        private void OnDisable()
        {
            instances.Remove(this);
        }
        private static List<TradeDef> instances = new List<TradeDef>();
        #endregion

        [Serializable]
        public class TradeOption
        {
            [TypeRestrictedReference(new Type[]
            {
                typeof(ItemDef),
                typeof(EquipmentDef),
                typeof(MiscPickupDef),
                typeof(ArtifactDef),
            })]
            public UnityEngine.Object pickupDef;
            public string pickupName;
            [NonSerialized]
            public PickupIndex pickupIndex = PickupIndex.none;
            public float weight = 1f;
        }
    }
}
