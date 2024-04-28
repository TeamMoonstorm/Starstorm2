﻿using Moonstorm.Starstorm2.Buffs;
using RoR2;
using RoR2.Items;
using UnityEngine;
using RoR2.Orbs;
namespace Moonstorm.Starstorm2.Items
{
    public sealed class DetritiveTrematode : ItemBase
    {
        private const string token = "SS2_ITEM_DETRITIVETREMATODE_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("DetritiveTrematode", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigName = "Trematode Threshold", ConfigDesc = "Amount of missing health needed for Trematode to proc. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float missingHealthPercentage = 0.30f;

        [RooConfigurableField(SS2Config.IDItem, ConfigName = "Trematode Threshold Per Stack", ConfigDesc = "Increase in missing health threshold, per stack. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float missingHealthPercentagePerStack = 0.15f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Damage dealt by the Trematode debuff, per second. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float trematodeDamage = 1f;

        public static GameObject biteEffect = SS2Assets.LoadAsset<GameObject>("TrematodeBiteEffect", SS2Bundle.Items);

        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.DetritiveTrematode;

            private static BodyIndex fucker = BodyIndex.None;
            private static BodyIndex Fucker => fucker == BodyIndex.None ? BodyCatalog.FindBodyIndexCaseInsensitive("ArtifactShellBody") : fucker;
            public void OnDamageDealtServer(DamageReport report)
            {
                var victim = report.victim;
                var attacker = report.attacker;

                if (!victim.alive || victim.body.bodyIndex == Fucker) return;

                //var dotController = DotController.FindDotController(victim.gameObject);               
                //if (dotController)
                //    hasDot = dotController.HasDotActive(Trematodes.index);
                bool hasDot = victim.body.HasBuff(SS2Content.Buffs.BuffTrematodes);
                // dots apparently dont get updated instantly???? so we can apply multiple of the same dot before HasDotActive returns true. hopo game

                //30% + 15% per stack hyperbolically
                //1 = 30%
                //5 = 63.5%
                //10 = 83.8%
                float requiredHealthPercentage = 1 - (1 - missingHealthPercentage) * Mathf.Pow(1 - missingHealthPercentagePerStack, stack - 1);

                //25% + 10% per stack
                //1 = 30%
                //5 = 65%
                // 9 = 105%
                //float requiredHealthPercentage = missingHealthPercentage + missingHealthPercentagePerStack * (stack - 1);
                if (victim.combinedHealthFraction < requiredHealthPercentage && !hasDot && (victim.gameObject != attacker))
                {

                    // do the first "tick" instantly
                    FuckinUhhhhhhhhhhOrb orb = new FuckinUhhhhhhhhhhOrb();
                    orb.origin = victim.body.mainHurtBox.transform.position;
                    orb.target = victim.body.mainHurtBox;
                    orb.damageValue = trematodeDamage * base.body.damage * 0.5f; // 2 ticks per second means divide by 2
                    orb.attacker = base.gameObject;
                    OrbManager.instance.AddOrb(orb);

                    var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = attacker,
                        victimObject = victim.gameObject,
                        dotIndex = Trematodes.index,
                        duration = Mathf.Infinity,
                        damageMultiplier = trematodeDamage * stack,
                    };
                    DotController.InflictDot(ref dotInfo);
                    
                    

                    EffectManager.SimpleEffect(biteEffect, report.damageInfo.position, Quaternion.identity, true);
                }
            }
        }

        // dumb (but correct) way of adding an extra instance of damage from within HealthComponent.TakeDamage
        // simply calling TakeDamage again can make enemies die twice
        private class FuckinUhhhhhhhhhhOrb : Orb
        {
            public override void Begin()
            {
                base.duration = 0;
            }
            public override void OnArrival()
            {
                if (this.target)
                {
                    HealthComponent healthComponent = this.target.healthComponent;
                    if (healthComponent)
                    {
                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = this.damageValue;
                        damageInfo.attacker = this.attacker;
                        damageInfo.inflictor = null;
                        damageInfo.force = Vector3.zero;
                        damageInfo.crit = false;
                        damageInfo.procChainMask = default(ProcChainMask);
                        damageInfo.procCoefficient = 0f;
                        damageInfo.position = this.target.transform.position;
                        damageInfo.damageColorIndex = DamageColorIndex.Item;
                        damageInfo.damageType = DamageType.DoT;
                        healthComponent.TakeDamage(damageInfo);

                        //purposefully not doing this. keeping it here to remind u not to do this.
                        //GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                        //GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                    }
                }
            }
            public float damageValue;
            public GameObject attacker;
            public TeamIndex teamIndex;
        }
    }
}
