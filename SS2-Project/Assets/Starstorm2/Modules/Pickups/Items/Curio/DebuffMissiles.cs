using MSU;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using RoR2.ContentManagement;
using RoR2.Items;
using RoR2.Orbs;
namespace SS2.Items
{
    public sealed class DebuffMissiles : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acDebuffMissiles", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => true;
		static GameObject orbEffect;
		public override void Initialize()
		{
			orbEffect = SS2Assets.LoadAsset<GameObject>("DebuffDamageOrbEffect", SS2Bundle.Items);
		}
		public class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
		{
			[ItemDefAssociation(useOnServer = true, useOnClient = false)]
			private static ItemDef GetItemDef() => SS2Content.Items.DebuffMissiles;

			private static readonly Queue<DamageOrb> orbQueue = new Queue<DamageOrb>();
			private static float minInterval = 0.04f;
			private float stopwatch;
			private void FixedUpdate()
			{
				if (orbQueue.Count > 0)
				{
					stopwatch += Time.fixedDeltaTime;
					while(stopwatch > minInterval && orbQueue.Count > 0)
                    {						
						DamageOrb orb = orbQueue.Dequeue();
						if(orb.target && orb.target.healthComponent.alive)
                        {
							stopwatch -= minInterval;
							orb.origin = body.inputBank.aimOrigin;
							OrbManager.instance.AddOrb(orb);
						}						
                    }
				}
			}
			public void OnDamageDealtServer(DamageReport damageReport)
			{
				DamageInfo damageInfo = damageReport.damageInfo;
				if (!damageInfo.damageType.IsDamageSourceSkillBased || !damageReport.victimBody) return;
				int debuffs = 0;
				foreach (BuffIndex buffType in BuffCatalog.debuffBuffIndices)
				{
					if (damageReport.victimBody.HasBuff(buffType))
					{
						debuffs++;
					}
				}
				DotController dotController = DotController.FindDotController(damageReport.victim.gameObject);
				if (dotController)
				{
					for (DotController.DotIndex dotIndex = DotController.DotIndex.Bleed; dotIndex < DotController.DotIndex.Count; dotIndex++)
					{
						if (dotController.HasDotActive(dotIndex))
						{
							debuffs++;
						}
					}
				}
				float damageCoefficient = 0.2f + 0.05f * (stack - 1);
				float damage = body.damage * damageCoefficient;
				Vector3 origin = body.inputBank.aimOrigin;
				for (int i = 0; i < debuffs; i++)
                {
					DamageOrb orb = new DamageOrb();
					orb.origin = origin;
					orb.damageValue = damage;
					orb.isCrit = damageInfo.crit;
					orb.teamIndex = damageReport.attackerTeamIndex;
					orb.attacker = damageInfo.attacker;
					orb.procChainMask = damageInfo.procChainMask;
					orb.procCoefficient = damageInfo.procCoefficient * 0.2f;
					orb.damageColorIndex = DamageColorIndex.Item;
					HurtBox[] group = damageReport.victimBody.hurtBoxGroup.hurtBoxes;
					int index = UnityEngine.Random.Range(0, group.Length - 1);
					HurtBox target = group[index];
					if (target)
					{
						orb.target = target;
					}
					orbQueue.Enqueue(orb);
				}				
			}
		}
		public class DamageOrb : GenericDamageOrb
		{
			public override void Begin()
			{
				speed = 100f;
				base.Begin();
			}
			public override GameObject GetOrbEffect()
			{
				return orbEffect;
			}
		}
	}
}
