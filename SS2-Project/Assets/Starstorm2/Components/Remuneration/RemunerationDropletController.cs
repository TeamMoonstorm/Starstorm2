using System;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace Moonstorm.Starstorm2.Components
{
	public class RemunerationDropletController : NetworkBehaviour
	{
		//what the droplet will spawn
		public static GameObject shopOptionPrefab;


		//[SyncVar]
		//[NonSerialized]
		//public PickupIndex pickupIndex = PickupIndex.none;

		// ^^^ IDK HOW PICKUPS WORK REALLY. SHOULD FIGURE IT OUT. HAHA

		private bool alive = true;

		public RemunerationShopBehavior shop;

		public void OnCollisionEnter(Collision collision)
		{
			if (NetworkServer.active && this.alive)
			{
				this.alive = false;
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Items.Remuneration.HOPEFULLYTEMPORARYREMUNERATIONSHOPOPTIONPREFABLOL, base.transform.position + Vector3.down, Quaternion.identity);
				RemunerationChoiceBehavior choice = gameObject.GetComponent<RemunerationChoiceBehavior>();
				if(choice)
                {
					choice.shop = this.shop;
                }
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}


		private void Start()
		{
			//always void droplet
			PickupDef pickupDef = PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex(DLC1Content.Items.CloverVoid.itemIndex));
			GameObject gameObject = (pickupDef != null) ? pickupDef.dropletDisplayPrefab : null;
			if (gameObject)
			{
				UnityEngine.Object.Instantiate<GameObject>(gameObject, base.transform);
			}
		}

		
	}
}
