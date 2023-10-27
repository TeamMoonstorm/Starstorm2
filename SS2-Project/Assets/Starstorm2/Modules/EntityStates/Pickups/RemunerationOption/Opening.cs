using System;
using EntityStates.Barrel;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.RemunerationOption
{

	// VOID TOKEN SPITTER
	public class Opening : EntityState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			this.PlayAnimation("Body", "Opening");
			this.timeBetweenDrops = Opening.duration / (float)Opening.maxItemDropCount;
			this.chestBehavior = base.GetComponent<ChestBehavior>();
			if (base.sfxLocator)
			{
				Util.PlaySound(base.sfxLocator.openSound, base.gameObject);
			}
		}
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active)
			{
				this.itemDropAge += Time.fixedDeltaTime;
				if (this.itemDropCount < (float)Opening.maxItemDropCount && this.itemDropAge > this.timeBetweenDrops)
				{
					this.itemDropCount += 1f;
					this.itemDropAge -= this.timeBetweenDrops;
					this.chestBehavior.dropPickup = PickupCatalog.FindPickupIndex(RoR2Content.MiscPickups.LunarCoin.miscPickupIndex); // VOID COIN
					this.chestBehavior.ItemDrop();
				}
				if (base.fixedAge >= Opening.duration)
				{
					this.outer.SetNextState(new Opened());
					return;
				}
			}
		}

		public static float duration = 1f;

		public static int maxItemDropCount = 5;

		private ChestBehavior chestBehavior;

		private float itemDropCount;

		private float timeBetweenDrops;

		private float itemDropAge;
	}
}
