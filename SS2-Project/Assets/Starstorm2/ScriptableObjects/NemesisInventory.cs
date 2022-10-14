using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Starstorm2.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NemesisInventory", menuName = "Starstorm2/NemesisInventory")]
    public class NemesisInventory : ScriptableObject
    {
        [Serializable]
        public struct NemesisItemStringReference
        {
            public string itemDefName;
            public float chanceForApplying;
            public int maxStacks;
            public int minStacks;
            public ItemDef ItemDef
            {
                get
                {
                    if (_itemDef)
                    {
                        return _itemDef;
                    }
                    else
                    {
                        if (!ItemCatalog.availability.available)
                        {
                            SS2Log.Error($"Tried to get the ItemDef of name {itemDefName} when the ItemCatalog has not initialized!");
                            return null;
                        }
                        ItemIndex index = ItemCatalog.FindItemIndex(itemDefName);
                        if (index == ItemIndex.None)
                        {
                            SS2Log.Error($"Could not find ItemDef of name {itemDefName}, FindItemIndex returned ItemIndex.None!");
                            return null;
                        }
                        _itemDef = ItemCatalog.GetItemDef(index);
                        return _itemDef;
                    }
                }
                private set
                {
                    _itemDef = value;
                }
            }

            private ItemDef _itemDef;


            public NemesisItemStringReference(string itemDefName, int maxStacks, int minStacks, float chanceForApplying)
            {
                this.itemDefName = itemDefName;
                this.maxStacks = maxStacks;
                this.minStacks = minStacks;
                this.chanceForApplying = chanceForApplying;
                this._itemDef = null;
            }
            internal void GetItemDef()
            {
                var foo = ItemDef;
            }
        }

        public List<NemesisItemStringReference> nemesisItems;

        internal WeightedSelection<NemesisItemStringReference> nemesisItemSelections = new WeightedSelection<NemesisItemStringReference>();

        public void Initialize()
        {
            for (int i = 0; i < nemesisItems.Count; i++)
            {
                var current = nemesisItems[i];
                current.GetItemDef();
                nemesisItems[i] = current;
            }

            foreach (NemesisItemStringReference itemStringRef in nemesisItems)
            {
                if (itemStringRef.ItemDef)
                {
                    WeightedSelection<NemesisItemStringReference>.ChoiceInfo choiceInfo = new WeightedSelection<NemesisItemStringReference>.ChoiceInfo
                    {
                        weight = itemStringRef.chanceForApplying,
                        value = itemStringRef
                    };
                    nemesisItemSelections.AddChoice(choiceInfo);
                }
            }
        }

        public void GiveItems(Inventory inv)
        {
            if (NetworkServer.active && Run.instance)
            {
                float itemsPerMin = 0.5f;
                float itemsDev = 0.2f;
                int runTimeMins = (int)Math.Round(Run.instance.GetRunStopwatch() / 60);
                int numItems = (int)(runTimeMins * itemsPerMin * Run.instance.difficultyCoefficient);

                numItems += (int)(numItems * UnityEngine.Random.Range(-itemsDev, itemsDev));

                for (int i = 0; i < numItems; i++)
                {
                    var selected = nemesisItemSelections.Evaluate(Run.instance.runRNG.nextNormalizedFloat);
                    if (inv.GetItemCount(selected.ItemDef) == 0)
                    {
                        inv.GiveItem(selected.ItemDef, UnityEngine.Random.Range(selected.minStacks, selected.maxStacks));
                    }
                }

                SS2Log.Info($"Given the following items to {inv.gameObject}:");
                foreach (ItemIndex i in inv.itemAcquisitionOrder)
                {
                    SS2Log.Info($"{inv.GetItemCount(i)}x {ItemCatalog.GetItemDef(i).name}");
                }
            }
        }
    }
}
