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
        public override bool IsAvailable(ContentPack contentPack) => false;
		public static GameObject orbEffect;
		public static float healthDamage = .01f;
		public static float percentChancePerDebuff = 3f;
		public override void Initialize()
		{
			orbEffect = SS2Assets.LoadAsset<GameObject>("DebuffDamageOrbEffect", SS2Bundle.Items);
		}
		public class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
		{
			[ItemDefAssociation(useOnServer = true, useOnClient = false)]
			private static ItemDef GetItemDef() => SS2Content.Items.DebuffMissiles;
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
				float chance = (percentChancePerDebuff + (stack - 1)) * debuffs;
				if(Util.CheckRoll(chance * damageReport.damageInfo.procCoefficient, body.master))
                {
					Vector3 origin = body.inputBank.aimOrigin;
					DamageOrb orb = new DamageOrb();
					orb.origin = origin;
					orb.damageValue = 4f * damageInfo.damage;// damageReport.victimBody.healthComponent.health * healthDamage;
					orb.isCrit = false;
					orb.teamIndex = damageReport.attackerTeamIndex;
					orb.attacker = damageInfo.attacker;
					orb.procChainMask = damageInfo.procChainMask;
					orb.procCoefficient = 1f;
					orb.damageColorIndex = DamageColorIndex.Void;
					HurtBox[] group = damageReport.victimBody.hurtBoxGroup.hurtBoxes;
					int index = UnityEngine.Random.Range(0, group.Length - 1);
					HurtBox target = group[index];
					if (target)
					{
						orb.target = target;
						OrbManager.instance.AddOrb(orb);
					}					
				}						
			}
		}
		public class DamageOrb : GenericDamageOrb
		{
			public override void Begin()
			{
				speed = 75f;
				base.Begin();
			}
			public override GameObject GetOrbEffect()
			{
				return orbEffect;
			}
		}
	}
}
