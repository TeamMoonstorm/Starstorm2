using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace SS2.Components
{
	public class LunarGamblerBehavior : NetworkBehaviour
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
		private ChildLocator childLocator;
		private void Start()
		{
			this.rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
			this.purchaseInteraction = base.GetComponent<PurchaseInteraction>();
			this.childLocator = base.GetComponent<ChildLocator>();


			int stageClearCount = CurseManager.GetStageClearCount();
			if(stageClearCount > 0)
            {
				ParticleSystem ps = this.childLocator.FindChild("NumeralIconSingle").GetComponent<ParticleSystem>();
				ParticleSystem.TextureSheetAnimationModule ts = ps.textureSheetAnimation;
				ts.startFrame = stageClearCount - 1;
				ps.gameObject.SetActive(true);
			}		
		}

		public void FixedUpdate()
		{
			if(this.cursePool.IsEmpty())
            {
				this.waitingForRefresh = false;
				this.purchaseInteraction.SetAvailable(false);
            }

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
			int stageClearCount = CurseManager.GetStageClearCount();
			if(CurseManager.GetStageClearCount() > 0)
            {
				CashOut(stageClearCount, CurseManager.GetTotal());
				return;
            }
			
			AddCurse();
		}

		private void CashOut(int stageClearCount, int curseCount)
        {
			this.purchaseInteraction.SetAvailable(false);
			this.waitingForRefresh = false;

			CurseManager.ClearCurses(); // if something else adds curses, we need to track curse sources.

			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
			{
				origin = base.transform.position,
				rotation = Quaternion.identity,
				scale = 1f,
				color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.LunarItem)
			}, true);

			SS2Log.Info($"CASHOUT. stageClearCount={stageClearCount}  curseCount={curseCount}");
		}
		private void AddCurse()
        {
			if (this.cursePool.IsEmpty()) return;

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
			if (machine)
			{
				machine.SetNextState(new EntityStates.LunarTable.Activate());
			}
		}
	}
}
