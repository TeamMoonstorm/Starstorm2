using System;
using EntityStates;	
using EntityStates.Scrapper;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using System.Collections.Generic;
namespace SS2
{
	// this is for evil tp
	// TODO: combine with TraderController (zanzan)?????????
	public class TradeController : NetworkBehaviour
	{
		public static GameObject pickupPrefablol; // move into tradedef
		[SystemInitializer]
		private static void InitPrefabTEMP()
        {
			pickupPrefablol = R2API.PrefabAPI.InstantiateClone(UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OptionPickup/OptionPickup.prefab").WaitForCompletion(), "BRUHHHHHHHHHHHHHH");
			pickupPrefablol.GetComponent<PickupPickerController>().panelPrefab = SS2Assets.LoadAsset<GameObject>("TradeOptionPickerPanel", SS2Bundle.Interactables);
		}
		public TradeDef[] trades; // rename to explicitTrades
		public SerializableEntityStateType tradeState = new SerializableEntityStateType(typeof(WaitToBeginScrapping));
		public GameObject pickupPrefab; // move into tradedef
		public Transform dropTransform;

		public float dropUpVelocityStrength = 15f;
		public float dropForwardVelocityStrength = 5f;

		private List<ItemIndex> desiredItems = new List<ItemIndex>();	
		private PickupPickerController pickupPickerController;
		private EntityStateMachine esm;
		public ItemIndex lastTradedItemIndex { get; private set; }
		private Interactor interactor;
		private Xoroshiro128Plus rng;
		private void Awake()
        {
			pickupPickerController = base.GetComponent<PickupPickerController>();
			esm = base.GetComponent<EntityStateMachine>();
			// temp
			pickupPrefab = pickupPrefablol;
		}
        private void Start()
        {
			for (int i = 0; i < trades.Length; i++)
			{
				if (trades[i].desiredItem)
					desiredItems.Add(trades[i].desiredItem.itemIndex);
			}
			if(NetworkServer.active)
				rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
		}
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
			this.interactor = activator;
			List<PickupPickerController.Option> list = new List<PickupPickerController.Option>();
			for (int i = 0; i < inventory.itemAcquisitionOrder.Count; i++)
			{
				ItemIndex itemIndex = inventory.itemAcquisitionOrder[i];
				PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);
				if (pickupIndex != PickupIndex.none && desiredItems.Contains(itemIndex))
				{
					// have a required itemcount for tradedefs, set available to false if player has less
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
		public void BeginTrading(int intPickupIndex)
		{
			PickupDef pickupDef = PickupCatalog.GetPickupDef(new PickupIndex(intPickupIndex));
			if (pickupDef != null && this.interactor)
			{
				this.lastTradedItemIndex = pickupDef.itemIndex;
				CharacterBody component = this.interactor.GetComponent<CharacterBody>();
				if (component && component.inventory)
				{
					if (component.inventory.GetItemCount(pickupDef.itemIndex) > 0)
					{
						component.inventory.RemoveItem(pickupDef.itemIndex);
						ScrapperController.CreateItemTakenOrb(component.corePosition, base.gameObject, pickupDef.itemIndex);
					}
				}
			}
			foreach(TradeDef trade in trades) // temp ///////////////////////////////////////////////////////////////////////////////////
            {
				if(trade.desiredItem.itemIndex == pickupDef.itemIndex)
                {
					Transform dropTransform = this.dropTransform ? this.dropTransform : base.transform;
					GenericPickupController.CreatePickupInfo createPickupInfo = new GenericPickupController.CreatePickupInfo
					{
						pickerOptions = PickupPickerController.GenerateOptionsFromArray(trade.GenerateUniquePickups(rng)),
						prefabOverride = pickupPrefab,
						position = dropTransform.position,
						rotation = Quaternion.identity,
						pickupIndex = PickupCatalog.FindPickupIndex(SS2Content.ItemTierDefs.Curio.tier)
					};
					PickupDropletController.CreatePickupDroplet(createPickupInfo, createPickupInfo.position, Vector3.up * dropUpVelocityStrength + dropTransform.forward * dropForwardVelocityStrength);
					break;
                }
            }
			if (this.esm) // later
			{
				this.esm.SetNextState(EntityStateCatalog.InstantiateState(ref this.tradeState));
			}
		}
		
	}
}
