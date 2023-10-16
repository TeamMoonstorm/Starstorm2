using Moonstorm.AddressableAssets;
using Moonstorm.Starstorm2;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NemesisInventory", menuName = "Starstorm2/NemesisInventory")]
    public class NemesisInventory : ScriptableObject
    {
        public List<NemesisAddressableItemReference> nemesisInventory = new List<NemesisAddressableItemReference>();
        internal WeightedSelection<NemesisAddressableItemReference> nemesisInventorySelection = new WeightedSelection<NemesisAddressableItemReference>();

        private static List<NemesisInventory> instances = new List<NemesisInventory>();

        private void Awake()
        {
            if(!instances.Contains(this))
                instances.Add(this);
        }

        private void OnDestroy()
        {
            if(instances.Contains(this))
                instances.Remove(this);
        }

        [SystemInitializer]
        private static void Initialize()
        {
            AddressableAsset.OnAddressableAssetsLoaded += InitializeNemesisInventories;

            void InitializeNemesisInventories()
            {
                foreach(NemesisInventory nemesisInventory in instances)
                {
                    nemesisInventory.CreateWeightedSelection();
                }
            }
        }

        public void GiveItems(Inventory inv)
        {
            if (NetworkServer.active && Run.instance)
            {
                inv.GiveItem(RoR2Content.Items.TeleportWhenOob); // teleport we noob

                float itemsPerMin = 0.5f;
                float itemsDev = 0.2f;
                int runTimeMins = (int)Math.Round(Run.instance.GetRunStopwatch() / 60);
                int numItems = (int)(runTimeMins * itemsPerMin * Run.instance.difficultyCoefficient);

                numItems += (int)(numItems * UnityEngine.Random.Range(-itemsDev, itemsDev));

                for (int i = 0; i < numItems; i++)
                {
                    var selected = nemesisInventorySelection.Evaluate(Run.instance.runRNG.nextNormalizedFloat);
                    if(selected.itemDef.Asset.requiredExpansion && !Run.instance.IsExpansionEnabled(selected.itemDef.Asset.requiredExpansion))
                    {
                        i--;
                        continue;
                    }
                    if (inv.GetItemCount(selected.itemDef.Asset) == 0)
                    {
                        inv.GiveItem(selected.itemDef.Asset, UnityEngine.Random.Range(selected.minStacks, selected.maxStacks));
                    }
                }
               

                List<string> log = new List<string>();
                foreach (ItemIndex i in inv.itemAcquisitionOrder)
                {
                    log.Add($"{inv.GetItemCount(i)}x {ItemCatalog.GetItemDef(i).name}");
                }
                SS2Log.Debug($"Given the following items to {inv.gameObject}:\n{string.Join("\n", log)}");
            }
        }

        private void CreateWeightedSelection()
        {
            foreach (NemesisAddressableItemReference itemRef in nemesisInventory)
            {
                if (itemRef.itemDef.Asset)
                {
                    WeightedSelection<NemesisAddressableItemReference>.ChoiceInfo choiceInfo = new WeightedSelection<NemesisAddressableItemReference>.ChoiceInfo
                    {
                        weight = itemRef.weight,
                        value = itemRef
                    };
                    nemesisInventorySelection.AddChoice(choiceInfo);
                }
            }
        }

        [Serializable]
        public struct NemesisAddressableItemReference
        {
            public AddressableItemDef itemDef;
            public float weight;
            public int maxStacks;
            public int minStacks;

            public NemesisAddressableItemReference(ItemDef itemDef, int maxStacks, int minStacks, float weight)
            {
                this.itemDef = new AddressableItemDef(itemDef);
                this.maxStacks = maxStacks;
                this.minStacks = minStacks;
                this.weight = weight;
            }

            public NemesisAddressableItemReference(string itemDefName, int maxStacks, int minStacks, float weight)
            {
                this.itemDef = new AddressableItemDef(itemDefName);
                this.maxStacks = maxStacks;
                this.minStacks = minStacks;
                this.weight = weight;
            }
        }
    }
}
