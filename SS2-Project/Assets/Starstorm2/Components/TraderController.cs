using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2;
using EntityStates.Trader.Bag;
using RoR2.UI;
using R2API;

namespace SS2.Components
{
    public class TraderController : NetworkBehaviour
    {
        public PickupIndex nextReward { get; private set; }
        public PickupIndex favoriteItem { get; private set; }

        public EntityStateMachine esm;
        public PickupPickerController pickupPickerController;
        private Interactor interactor;

        public TraderDropTable dropTable; // make generic
        public float t1Min = 10;
        public float t1Max = 35;
        public float t2Min = 40;
        public float t2Max = 60;
        public float t3Min = 60;
        public float t3Max = 100;
        public float bossMin = 75;
        public float bossMax = 120;

        

        private Xoroshiro128Plus rng;
        public Dictionary<PickupIndex, float> itemValues = new Dictionary<PickupIndex, float>();
        private List<PickupIndex> specialItems = new List<PickupIndex>();
        public PickupIndex[] potentialRewards = new PickupIndex[4];
        void Start()
        {
            //Assign a favorite item.
            //Give every item a value.
            if (NetworkServer.active)
            {
                favoriteItem = FindFavorite();
                rng = new Xoroshiro128Plus(Run.instance.treasureRng); 
                foreach (PickupIndex item in Run.instance.availableTier1DropList)
                {
                    AddItem(item);                 
                }
            }
        }

        private PickupIndex FindFavorite()
        {
            int itemCount = ItemCatalog.allItemDefs.Length;
            int randomindex = UnityEngine.Random.Range(0, itemCount); //dual wielding randoms
            ItemDef favoriteItem = ItemCatalog.allItemDefs[randomindex];

            //if an item can't be found, then find an item.
            if (!Run.instance.IsItemAvailable(favoriteItem.itemIndex))
                favoriteItem = RoR2Content.Items.BarrierOnOverHeal;

            //replace trash with trash
            if (favoriteItem == RoR2Content.Items.ScrapGreen || favoriteItem == RoR2Content.Items.ScrapWhite || favoriteItem == RoR2Content.Items.ScrapYellow || favoriteItem == RoR2Content.Items.ScrapRed)
                favoriteItem = RoR2Content.Items.BarrierOnOverHeal;

            return PickupCatalog.FindPickupIndex(favoriteItem.itemIndex);
        }

        // NOT NETWORKED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        private void AddItem(PickupIndex pickupIndex)
        {
            float value = 0;
            ItemIndex itemIndex = PickupCatalog.GetPickupDef(pickupIndex).itemIndex;
            ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
            if(itemDef)
            {
                switch (itemDef.tier)
                {
                    case ItemTier.Boss:
                        value = rng.RangeFloat(bossMin, bossMax);
                        break;
                    //rare, boss, and void equivalents:
                    case ItemTier.Tier3:
                    case ItemTier.VoidTier3:
                    case ItemTier.VoidBoss:
                        value = rng.RangeFloat(t3Min, t3Max);
                        break;
                    //uncommon and void equivalent:
                    case ItemTier.Tier2:
                    case ItemTier.VoidTier2:
                        value = rng.RangeFloat(t2Min, t2Max);
                        break;
                    //common and void equivalent:
                    case ItemTier.Tier1:
                    case ItemTier.VoidTier1:
                        value = rng.RangeFloat(t1Min, t1Max);
                        break;
                }
                //we stay silly
                if (itemDef == RoR2Content.Items.BarrierOnOverHeal && favoriteItem.itemIndex != RoR2Content.Items.BarrierOnOverHeal.itemIndex)
                    value -= 0.2f;
            }
            else
            {
                SS2Log.Error("non item pickups arent supported yet");
                return;
            }
            itemValues.Add(pickupIndex, value);

        }
        [Server]
        public void AssignPotentialInteractor(Interactor activator)
        {
            if (!activator)
            {
                return;
            }
            CharacterBody component = activator.GetComponent<CharacterBody>();
            if (!component)
            {
                return;
            }
            Inventory inventory = component.inventory;
            if (!inventory)
            {
                return;
            }
            this.interactor = activator;
            List<PickupPickerController.Option> list = new List<PickupPickerController.Option>();
            for (int i = 0; i < inventory.itemAcquisitionOrder.Count; i++)
            {
                ItemIndex itemIndex = inventory.itemAcquisitionOrder[i];
                PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);
                if (pickupIndex != PickupIndex.none && itemValues.ContainsKey(pickupIndex))
                {
                    list.Add(new PickupPickerController.Option
                    {
                        available = true,
                        pickupIndex = pickupIndex
                    });
                }
            }
            pickupPickerController.SetOptionsServer(list.ToArray());
        }

        [Server]
        public void BeginTrade(int intPickupIndex)
        {
            PickupIndex pickup = new PickupIndex(intPickupIndex);
            PickupDef pickupDef = PickupCatalog.GetPickupDef(pickup);
            if (pickupDef != null && interactor)
            {
                CharacterBody interactorBody = interactor.GetComponent<CharacterBody>();
                if (interactorBody && interactorBody.inventory)
                {
                    interactorBody.inventory.RemoveItem(pickupDef.itemIndex, 1);
                    ScrapperController.CreateItemTakenOrb(interactorBody.corePosition, gameObject, pickupDef.itemIndex); // remove later
                }
            }
            if(!IsSpecial(pickup))
            {
                nextReward = PickupCatalog.FindPickupIndex(SS2Content.Items.ScavengersFortune.itemIndex); // temp until we add tradedefs here
            }
            else
            {
                float value = itemValues[pickup];
                PickupIndex[] potentialRewards = dropTable.GenerateDrops(4, value, this.rng); // could have random number of drops. doesnt really matter
                for (int i = 0; i < potentialRewards.Length; i++)
                {
                    SS2Log.Info($"TraderController.BeginTrade({intPickupIndex}): POTENTIAL ITEM " + i + "=" + (potentialRewards[i] != PickupIndex.none ? PickupCatalog.GetPickupDef(potentialRewards[i]).internalName : "NOTHING"));
                }
                PickupIndex reward = rng.NextElementUniform(potentialRewards);
                if (reward == PickupIndex.none) reward = PickupCatalog.FindScrapIndexForItemTier(ItemTier.Tier1); // TEMP FOR DEBUG(?)
                nextReward = reward; ///////// PROBABLY NEED TO TURN THIS INTO LIST OF PENDING REWARDS FOR MULTIPLAYER
                itemValues[pickup] *= 0.8f; ////////////////////////////////////////////////////////////////////////////////////////////////////////
            }
            if (esm)
            {
                esm.SetNextState(new WaitToBeginTrade());
            }
        }
        public bool IsSpecial(PickupIndex pickupIndex)
        {
            return PickupCatalog.GetPickupDef(pickupIndex).itemIndex == SS2Content.Items.ScavengersFortune.itemIndex;
            //return specialItems.Contains(pickupIndex); // implement this when we combine it with tradecontroller?
        }
        public float GetValue(PickupIndex pickupIndex)
        {
            float value = itemValues[pickupIndex];
            if (pickupIndex == favoriteItem)
                value = 0.2f + value * 2; //////////////////////////////////////////////////////////////////
            return value;
        }
    }
}
