using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace SS2.Components
{
	public sealed class LunarGamblerBehavior : NetworkBehaviour
	{
		private static float refreshDuration = .2f;
		private bool waitingForRefresh;
		private float refreshTimer;

		private PurchaseInteraction purchaseInteraction;
		private void Start()
		{
			this.purchaseInteraction = base.GetComponent<PurchaseInteraction>();
			if (Run.instance)
			{

			}
		}

		public void FixedUpdate()
		{
			if (this.waitingForRefresh)
			{
				this.refreshTimer -= Time.fixedDeltaTime;
				if (this.refreshTimer <= 0f)// && this.purchaseCount < this.maxPurchaseCount)
				{
					this.purchaseInteraction.SetAvailable(true);
					this.waitingForRefresh = false;
				}
			}
		}

		[Server]
		public void AddShrineStack(Interactor interactor)
		{
			this.waitingForRefresh = true;
			this.refreshTimer = refreshDuration;
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
			{
				origin = base.transform.position,
				rotation = Quaternion.identity,
				scale = 1f,
				color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.LunarItem)
			}, true);

			EntityStateMachine machine = base.GetComponent<EntityStateMachine>();
			if(machine)
            {
				machine.SetNextState(new EntityStates.LunarTable.Activate());
            }
		}
	}
}
