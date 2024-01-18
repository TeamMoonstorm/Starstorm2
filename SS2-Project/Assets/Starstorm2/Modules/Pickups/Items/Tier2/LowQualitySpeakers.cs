using R2API;
using RoR2;
using RoR2.Items;
using System;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{

    public sealed class LowQualitySpeakers : ItemBase
    {
        private const string token = "SS2_ITEM_LOWQUALITYSPEAKERS_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("LowQualitySpeakers", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Radius in which enemies are stunned, in meters.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float baseRadius = 13f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Additional stun radius per stack.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float radiusPerStack = 7f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Chance for this item to proc on taking damage")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float baseProcChance = 0.1f;

        public static GameObject burstEffect = SS2Assets.LoadAsset<GameObject>("SpeakerBurstEffect", SS2Bundle.Items);

        public override void Initialize()
        {
            base.Initialize();

            GlobalEventManager.onServerDamageDealt += SpeakerProc;
        }

        private void SpeakerProc(DamageReport damageReport)
        {          
            if (!damageReport.victimMaster || !damageReport.victimBody) return;

            CharacterBody body = damageReport.victimBody;
            int stack = body.inventory.GetItemCount(SS2Content.Items.LowQualitySpeakers);

            // 100% chance at 0% health, baseProcChance at 100% health
            // uses health fraction after damage, not before
            float procChance = Mathf.Lerp(1f, baseProcChance, body.healthComponent.combinedHealthFraction);
            SS2Log.Info($"LQS {procChance}");
            if(stack > 0 && Util.CheckRoll(procChance * 100f, body.master))
            {               
                float radius = baseRadius + stack * (radiusPerStack - 1);
                SS2Log.Info($"LQS RADIUS {radius}");

                
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.position = body.corePosition;
                blastAttack.baseDamage = 0f;
                blastAttack.baseForce = 600f;
                blastAttack.bonusForce = Vector3.zero;
                blastAttack.radius = radius;
                blastAttack.attacker = body.gameObject;
                blastAttack.inflictor = body.gameObject;
                blastAttack.teamIndex = body.teamComponent.teamIndex;
                blastAttack.crit = false;
                blastAttack.procChainMask = default(ProcChainMask);
                blastAttack.procCoefficient = 0f;
                blastAttack.falloffModel = BlastAttack.FalloffModel.Linear;
                blastAttack.damageColorIndex = DamageColorIndex.Default;
                blastAttack.damageType = DamageType.Stun1s;
                blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                blastAttack.Fire();

                EffectManager.SpawnEffect(burstEffect, new EffectData
                {
                    origin = body.corePosition,
                    scale = radius,
                    rotation = Quaternion.identity
                }, true);

            }


            



        }
    }
}
