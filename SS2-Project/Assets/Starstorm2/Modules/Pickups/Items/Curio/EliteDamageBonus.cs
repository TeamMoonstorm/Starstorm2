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
        public override bool IsAvailable(ContentPack contentPack) => SS2Config.enableBeta;

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
			private static float minInterval = 0.05f;
			private float stopwatch;
            private void FixedUpdate()
            {
                if(orbQueue.Count > 0)
                {
					stopwatch += Time.fixedDeltaTime;
					while(stopwatch > minInterval)
                    {						
						EliteDamageOrb orb = orbQueue.Dequeue();
						stopwatch -= minInterval;
						OrbManager.instance.AddOrb(orb);												
                    }
                }
            }
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                DamageInfo damageInfo = damageReport.damageInfo;
                if (!damageReport.victimIsElite || !damageInfo.damageType.IsDamageSourceSkillBased) return;
                float chance = 20 + 10 * stack;
                if(Util.CheckRoll(chance * damageInfo.procCoefficient))
                {
                    EliteDamageOrb orb = new EliteDamageOrb();
                    orb.origin = damageInfo.position;
                    orb.target = body.mainHurtBox;
                    orbQueue.Enqueue(orb);
                }              
            }
		}
		public class EliteDamageOrb : Orb
        {
            public SkillSlot skillSlot;
            public override void Begin()
            {
                if (this.target)
                {
                    base.duration = 0.2f;
                    EffectData effectData = new EffectData
                    {
                        origin = this.origin,
                        genericFloat = base.duration
                    };
                    effectData.SetHurtBoxReference(this.target);
                    EffectManager.SpawnEffect(orbEffect, effectData, true);
                }
            }
            public override void OnArrival()
            {
                if (target && target.healthComponent)
                {
                    target.healthComponent.body.skillLocator.DeductCooldownFromAllSkillsServer(0.5f);
                }
            }
        }
	}
}
