using MSU;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using RoR2.ContentManagement;
using RoR2.Orbs;
using RoR2.Items;
namespace SS2.Items
{
    public sealed class SnakeEyes : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acSnakeEyes", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => true;
        public static GameObject orbEffect;
        public override void Initialize()
        {
            orbEffect = SS2Assets.LoadAsset<GameObject>("CritOnShrineOrbEffect", SS2Bundle.Items);
            RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += OnInteractionBegin;
        }
        private void OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);
            if (self.isShrine && activator && activator.TryGetComponent(out CharacterBody body) && body.inventory)
            {
                int crit = body.inventory.GetItemCount(SS2Content.Items.SnakeEyes);
                if (crit > 0)
                {
                    GrantItemOrb orb = new GrantItemOrb();
                    orb.origin = self.transform.position;
                    orb.target = Util.FindBodyMainHurtBox(body);
                    orb.item = SS2Content.Items.StackSnakeEyes.itemIndex;
                    orb.count = 2 + 1 * crit;
                    orb.effectPrefab = orbEffect;
                    OrbManager.instance.AddOrb(orb);
                }
            }
        }
        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender.inventory) return;
            args.critAdd += sender.inventory.GetItemCount(SS2Content.Items.StackSnakeEyes);
        }
        public class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef() => SS2Content.Items.SnakeEyes;

            private static readonly Queue<ReduceCooldownOrb> orbQueue = new Queue<ReduceCooldownOrb>();
            private static float minInterval = 0.07f;
            private float stopwatch;
            private void FixedUpdate()
            {
                if (orbQueue.Count > 0)
                {
                    float t = Util.Remap(orbQueue.Count, 0, 30, 1, 5);
                    stopwatch += Time.fixedDeltaTime * t;
                    while (stopwatch > minInterval)
                    {
                        stopwatch -= minInterval;
                        Orb orb = orbQueue.Dequeue();
                        OrbManager.instance.AddOrb(orb);
                    }
                }
            }
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                DamageInfo damageInfo = damageReport.damageInfo;
                float extraCrit = body.crit - 100;
                if(extraCrit > 0)
                {
                    float chance = Util.ConvertAmplificationPercentageIntoReductionPercentage(extraCrit);
                    if(Util.CheckRoll(chance * damageInfo.procCoefficient, body.master))
                    {
                        ReduceCooldownOrb orb = new ReduceCooldownOrb();
                        orb.origin = damageInfo.position;
                        orb.target = body.mainHurtBox;
                        orbQueue.Enqueue(orb);
                    }                 
                }                
            }
        }

        public class ReduceCooldownOrb : Orb
        {
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
                if(target && target.healthComponent)
                {
                    target.healthComponent.body.skillLocator.DeductCooldownFromAllSkillsServer(1);
                }
            }
        }
    }
}
