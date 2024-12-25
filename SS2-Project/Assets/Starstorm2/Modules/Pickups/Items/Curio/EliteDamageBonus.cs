using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using RoR2.Items;
using RoR2.Orbs;
namespace SS2.Items
{
    public sealed class EliteDamageBonus : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acEliteDamageBonus", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => true;

		static GameObject orbEffect;
        public override void Initialize()
        {
			orbEffect = SS2Assets.LoadAsset<GameObject>("EliteDamageOrbEffect", SS2Bundle.Items);
        }
		public class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
		{
			[ItemDefAssociation(useOnServer = true, useOnClient = false)]
			private static ItemDef GetItemDef() => SS2Content.Items.EliteDamageBonus;

			private static readonly Queue<EliteDamageOrb> orbQueue = new Queue<EliteDamageOrb>();
			private static float minInterval = 0.1f;
			private float stopwatch;
            private void FixedUpdate()
            {
                if(orbQueue.Count > 0)
                {
					stopwatch += Time.fixedDeltaTime;
					while(stopwatch > minInterval && orbQueue.Count > 0)
                    {						
						EliteDamageOrb orb = orbQueue.Dequeue();
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
				if (!damageInfo.damageType.IsDamageSourceSkillBased) return;
				float damageCoefficient = 0.5f + 0.25f * (stack - 1);
				EliteDamageOrb orb = new EliteDamageOrb();
				orb.origin = base.body.inputBank.aimOrigin;
				orb.damageValue = damageInfo.damage * damageCoefficient;
				orb.isCrit = damageInfo.crit;
				orb.teamIndex = damageReport.attackerTeamIndex;
				orb.attacker = damageInfo.attacker;
				orb.procChainMask = damageInfo.procChainMask;
				//orb.procChainMask.AddProc(ProcType.Missile); // shouldnt need this cuz its skills only
				orb.procCoefficient = damageInfo.procCoefficient * 0.5f;
				orb.damageColorIndex = DamageColorIndex.Item;
				HurtBox mainHurtBox = damageReport.victimBody ? damageReport.victimBody.mainHurtBox : null;
				if (mainHurtBox)
				{
					orb.target = mainHurtBox;					
				}
				orbQueue.Enqueue(orb);
			}
		}
		public class EliteDamageOrb : GenericDamageOrb
        {
            public override void Begin()
            {
				speed = 80f;
                base.Begin();
            }
			public override GameObject GetOrbEffect()
			{
				return orbEffect;
			}
		}
	}
}
