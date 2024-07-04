using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace SS2.Components
{
	public sealed class LunarGamblerBehavior : NetworkBehaviour
	{
		private static float refreshDuration = 1f;
		private bool waitingForRefresh;
		private float refreshTimer;

		private int purchaseCount;
		public int maxPurchaseCount = 10;
		private PurchaseInteraction purchaseInteraction;
		private Xoroshiro128Plus rng;
		public CursePool cursePool;

		public Transform particleOrigin;
		private void Start()
		{
			this.rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
			this.purchaseInteraction = base.GetComponent<PurchaseInteraction>();
		}

		public void FixedUpdate()
		{
			if (this.waitingForRefresh)
			{
				this.refreshTimer -= Time.fixedDeltaTime;
				if (this.refreshTimer <= 0f && this.purchaseCount < this.maxPurchaseCount)
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

			CurseIndex curseIndex = cursePool.GenerateCurse(this.rng);
			CurseManager.AddCurse(curseIndex);

			GameObject effectPrefab = SS2Assets.LoadAsset<GameObject>("CurseIconSingle", SS2Bundle.Interactables);
			EffectData effectData = new EffectData
			{
				origin = particleOrigin.position,
				genericUInt = (uint)(curseIndex + 1)
			};
			EffectManager.SpawnEffect(effectPrefab, effectData, true);

			EntityStateMachine machine = base.GetComponent<EntityStateMachine>();
			if(machine)
            {
				machine.SetNextState(new EntityStates.LunarTable.Activate());
            }
		}
	}
}
