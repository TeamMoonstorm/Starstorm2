using System;
using EntityStates;
using EntityStates.Scrapper;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using System.Collections.Generic;
namespace SS2
{
    public class PedestalBehavior : NetworkBehaviour
    {
        public PickupDropTable dropTable;
        public Transform dropTransform;
        public PickupDisplay pickupDisplay;

        public Vector3 dropVelocity;

        [SyncVar(hook = nameof(OnSyncPickupIndex))]
        private UniquePickup pickup = UniquePickup.none;

        private PickupPickerController pickupPickerController;
        private Interactor interactor;
        private Xoroshiro128Plus rng;
        private ItemTier currentTier;

        private void Awake()
        {
            pickupPickerController = base.GetComponent<PickupPickerController>();
        }
        private void Start()
        {
            if (NetworkServer.active)
            {
                rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
                GenerateNewPickupServer();
            }
            if (NetworkClient.active)
            {
                UpdatePickupDisplay();
            }
        }
        [Server]
        public void GenerateNewPickupServer()
        {
            var newPickup = UniquePickup.none;
            if (dropTable)
            {
                newPickup = dropTable.GeneratePickup(rng);
            }
            SetPickup(newPickup);
        }
        private void SetPickup(UniquePickup newPickup)
        {
            if (!pickup.Equals(newPickup))
            {
                pickup = newPickup;
                currentTier = ItemTier.NoTier;

                var pickupDef = PickupCatalog.GetPickupDef(pickup.pickupIndex);
                if (pickupDef != null && pickupDef.itemIndex != ItemIndex.None)
                {
                    var itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
                    if (itemDef)
                    {
                        currentTier = itemDef.tier;
                    }
                }
                UpdatePickupDisplay();
            }
        }
        private void OnSyncPickupIndex(UniquePickup newPickup)
        {
            SetPickup(newPickup);
            if (NetworkClient.active)
            {
                UpdatePickupDisplay();
            }
        }
        private void UpdatePickupDisplay()
        {
            if (pickupDisplay)
            {
                pickupDisplay.SetPickup(pickup);
            }    
        }
        // Called by PickupPickerController.onServerInteractionBegin
        public void SetOptionsFromInteractor(Interactor activator)
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

            interactor = activator;

            List<PickupPickerController.Option> options = new List<PickupPickerController.Option>();
            for (int i = 0; i < inventory.itemAcquisitionOrder.Count; i++)
            {
                ItemIndex itemIndex = inventory.itemAcquisitionOrder[i];
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);

                // If the item is not temporary and is not scrap and matches the tier of the item on the pedestal
                
                if (pickupIndex != PickupIndex.none && inventory.GetItemCountPermanent(itemIndex) > 0 && itemDef != null && itemDef.tier == currentTier)
                {
                    var scrapIndex = PickupCatalog.FindScrapIndexForItemTier(itemDef.tier);
                    if (scrapIndex != PickupIndex.none)
                    {
                        continue;
                    }
                    var pickup = new UniquePickup(pickupIndex);
                    options.Add(new PickupPickerController.Option
                    {
                        available = true,
                        pickup = pickup
                    });
                }
            }
            pickupPickerController.SetOptionsServer(options.ToArray());
        }

        // called by PickupPickerController.onPickupSelected
        [Server]
        public void BeginTrading(int intPickupIndex)
        {
            var newPickupIndex = new PickupIndex(intPickupIndex);
            PickupDef pickupDef = PickupCatalog.GetPickupDef(newPickupIndex);
            if (pickupDef != null && interactor)
            {
                // Remove item picked by player from their inventory and give it the pedestal's item
                CharacterBody body = interactor.GetComponent<CharacterBody>();
                if (body && body.master && body.inventory)
                {
                    if (body.inventory.GetItemCountPermanent(pickupDef.itemIndex) > 0)
                    {
                        body.inventory.RemoveItemPermanent(pickupDef.itemIndex);
                        // ScrapperController.CreateItemTakenOrb(body.corePosition, base.gameObject, pickupDef.itemIndex); // nah

                        PickupDef currentPickupDef = PickupCatalog.GetPickupDef(pickup.pickupIndex);
                        ItemIndex itemIndex = (currentPickupDef != null) ? currentPickupDef.itemIndex : ItemIndex.None;
                        if (itemIndex != ItemIndex.None) // unfortunate that it can only grant items, but trading with the pedestal implies giving it an item anyways
                        {
                            body.inventory.GiveItemPermanent(itemIndex);
                            GenericPickupController.SendPickupMessage(body.master, pickup);
                        }
                    }
                }

                // set our new pickup to player-picked item
                SetPickup(new UniquePickup(newPickupIndex));
            }
        }

        public UniquePickup GetPickup()
        {
            return pickup;
        }
    }
}
