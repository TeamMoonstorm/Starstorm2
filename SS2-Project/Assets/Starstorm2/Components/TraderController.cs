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
        private static readonly uint valuesDirtyBit = 1U;
        public GameObject rewardControllerPrefab;
        public Queue<Reward> pendingRewards = new Queue<Reward>();
        public struct Reward
        {
            public PickupIndex drop;
            public GameObject target;
        }
        public PickupIndex favoriteItem { get; private set; }

        public EntityStateMachine esm;
        public PickupPickerController pickupPickerController;
        private Interactor interactor;

        public TraderDropTable dropTable; // make generic
        public float t1Min = 10; // yucky
        public float t1Max = 35;
        public float t2Min = 40;
        public float t2Max = 60;
        public float t3Min = 80;
        public float t3Max = 110;
        public float bossMin = 75;
        public float bossMax = 120;    

        private Xoroshiro128Plus rng;
        public Dictionary<PickupIndex, float> itemValues = new Dictionary<PickupIndex, float>();
        private List<PickupIndex> specialItems = new List<PickupIndex>();
        public PickupIndex[] potentialRewards = new PickupIndex[4];
        private PickupIndex[] allAvailableItems;
        private void Awake()
        {
            allAvailableItems = HG.ArrayUtils.Join(Run.instance.availableTier1DropList.ToArray(), Run.instance.availableTier2DropList.ToArray(), Run.instance.availableTier3DropList.ToArray(), Run.instance.availableBossDropList.ToArray());
            itemValues = new Dictionary<PickupIndex, float>(allAvailableItems.Length);
            //Assign a favorite item.
            //Give every item a value.
            if (NetworkServer.active)
            {
                rng = new Xoroshiro128Plus(Run.instance.treasureRng);
                favoriteItem = FindFavorite();
                foreach (PickupIndex item in allAvailableItems)
                {
                    AddItem(item);
                }
            }
        }

        private PickupIndex FindFavorite()
        {
            return rng.NextElementUniform(allAvailableItems);
        }

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
                if (itemDef == RoR2Content.Items.BarrierOnOverHeal)
                    value -= 0.2f;
            }
            else
            {
                SS2Log.Error("non item pickups arent supported yet");
                return;
            }
            if (pickupIndex == favoriteItem)
                value = 0.2f + value * 2; //////////////////////////////////////////////////////////////////
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
            PickupIndex[] potentialRewards = null;
            int rewardIndex = 0;
            if(IsSpecial(pickup))
            {
                potentialRewards = new PickupIndex[1] { PickupCatalog.FindPickupIndex(SS2Content.Items.ShardScav.itemIndex) }; // temp until we add tradedefs here
                pendingRewards.Enqueue(new Reward { drop = potentialRewards[0], target = interactor.gameObject });
            }
            else
            {
                float value = itemValues[pickup];
                SS2Log.Info($"START TRADE {pickupDef.internalName}. VALUE = {value}--------------------------------");
                potentialRewards = dropTable.GenerateDrops(4, value, this.rng); // could have random number of drops. doesnt really matter       
                for (int i = 0; i < potentialRewards.Length; i++)
                {
                    if (potentialRewards[i] == PickupIndex.none) potentialRewards[i] = PickupCatalog.FindScrapIndexForItemTier(ItemTier.Tier1);
                }
                SS2Log.Info($"END TRADE {pickupDef.internalName} -------------------------------------------------");
                rewardIndex = rng.RangeInt(0, potentialRewards.Length);
                PickupIndex reward = potentialRewards[rewardIndex];
                pendingRewards.Enqueue(new Reward { drop = reward, target = interactor.gameObject });
                itemValues[pickup] *= 0.75f; ////////////////////////////////////////////////////////////////////////////////////////////////////////
                if(itemValues.ContainsKey(reward)) // reward can be none or special item
                    itemValues[reward] *= 0.75f; //////////////////////////////////;le balace////////////////////////////////////////////////////////////////////////
                base.SetDirtyBit(valuesDirtyBit);
                /// chance to change favorite item could be good
            }
            if(rewardControllerPrefab)
            {
                PickupCarouselController reward = GameObject.Instantiate(rewardControllerPrefab, base.transform.position, Quaternion.identity).GetComponent<PickupCarouselController>();
                reward.options = potentialRewards;
                reward.chosenRewardIndex = (uint)rewardIndex;
                reward.interactor = interactor;
                pickupPickerController.networkUIPromptController.SetParticipantMaster(null);             
                NetworkServer.Spawn(reward.gameObject);              
            }
            if (esm)
            {
                esm.SetNextState(new WaitToBeginTrade()); ///
            }
        }
        public bool IsSpecial(PickupIndex pickupIndex)
        {
            return PickupCatalog.GetPickupDef(pickupIndex).itemIndex == SS2Content.Items.ScavengersFortune.itemIndex;
            //return specialItems.Contains(pickupIndex); // implement this when we combine it with tradecontroller?
        }
        public float GetValue(PickupIndex pickupIndex)
        {
            return itemValues[pickupIndex];
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            uint bits = base.syncVarDirtyBits;
            if (initialState) bits = valuesDirtyBit;
            bool shouldWrite = (bits & valuesDirtyBit) > 0U;
            writer.WritePackedUInt32(bits); // im learning please be nice
            if (shouldWrite)
            {
                writer.WritePackedUInt32((uint)allAvailableItems.Length);
                for (int i = 0; i < allAvailableItems.Length; i++)
                {
                    writer.Write(allAvailableItems[i]);
                    writer.Write(itemValues[allAvailableItems[i]]); // are floats bad?
                }
                writer.Write(favoriteItem); // should be a separate bit
            }
            return shouldWrite;
        }
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            uint dirtyBits = reader.ReadPackedUInt32();
            if ((dirtyBits & valuesDirtyBit) > 0U)
            {
                int count = (int)reader.ReadPackedUInt32();
                itemValues = new Dictionary<PickupIndex, float>(count);
                for (int i = 0; i < count; i++)
                {
                    itemValues.Add(reader.ReadPickupIndex(), reader.ReadSingle());
                }
                favoriteItem = reader.ReadPickupIndex();
            }
        }
    }
}
