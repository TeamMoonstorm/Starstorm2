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
		public static GameObject shopOptionPrefab;
		public static GameObject dropletPrefab;
		[SystemInitializer]
		private static void Init()
        {
			dropletPrefab = SS2Assets.LoadAsset<GameObject>("RemunerationDroplet", SS2Bundle.Items);
			shopOptionPrefab = SS2Assets.LoadAsset<GameObject>("RemunerationPedestal", SS2Bundle.Items);
		}

		//[SyncVar]
		//[NonSerialized]
		//public PickupIndex pickupIndex = PickupIndex.none;

		// ^^^ IDK HOW PICKUPS WORK REALLY. SHOULD FIGURE IT OUT. HAHA

		[SyncVar]
		private bool alive = true;

		public RemunerationShopBehavior shop;

		private float stopwatch = 5f;
		public void OnCollisionEnter(Collision collision)
		{
			if (NetworkServer.active && this.alive)
			{
				this.alive = false;
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(shopOptionPrefab, base.transform.position + Vector3.down, Quaternion.identity);
				RemunerationChoiceBehavior choice = gameObject.GetComponent<RemunerationChoiceBehavior>();
				if(choice)
                {
					choice.shop = this.shop;
                }
				NetworkServer.Spawn(gameObject);
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

        private void FixedUpdate()
        {
			this.stopwatch -= Time.fixedDeltaTime;
			if(this.stopwatch <= 0 && shop)
            {
				this.shop.FailDroplet(this);
				Destroy(base.gameObject);
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
