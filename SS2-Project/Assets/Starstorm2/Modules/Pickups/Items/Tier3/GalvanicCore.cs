using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

#if DEBUG

namespace SS2.Items
{
    public sealed class GalvanicCore : SS2Item, IContentPackModifier
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acGalvanicCore", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance to stun on hit. (1 = 100%)")]
        [FormatToken("SS2_ITEM_GALVANICCORE_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float stunChance = .1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Health penalty applied to nearby enemies. (1 = 100%)")]
        [FormatToken("SS2_ITEM_GALVANICCORE_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float healthPenalty = .2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Damage penalty applied to nearby enemies. (1 = 100%)")]
        [FormatToken("SS2_ITEM_GALVANICCORE_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float damagePenalty = .3f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Speed penalty applied to nearby enemies. (1 = 100%)")]
        [FormatToken("SS2_ITEM_GALVANICCORE_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float speedPenalty = .8f;

        public static GameObject stunVFX;
        public static GameObject galvanicAura;

        private static GameObject gwbVFX;

        public override void Initialize()
        {
            On.RoR2.SetStateOnHurt.OnTakeDamageServer += SetStateDamageGalvanic;
            On.RoR2.SetStateOnHurt.SetStun += SetStunAddGalvanicAura;

            On.RoR2.CharacterBody.OnBuffFirstStackGained += FinalBuffStackGainedUpdateStats;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += FinalBuffStackLostPreventHealing;

            stunVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/StunChanceOnHit/ImpactStunGrenade.prefab").WaitForCompletion(); //LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade")
            galvanicAura = AssetCollection.FindAsset<GameObject>("GalvanicAura");
            
            gwbVFX = AssetCollection.FindAsset<GameObject>("GreaterBannerBuffEffectTEMP");

            TempVisualEffectAPI.AddTemporaryVisualEffect(gwbVFX.InstantiateClone("GreaterBannerBuffEffectTWOTEMP", false), (CharacterBody body) => { return body.HasBuff(SS2Content.Buffs.bdGalvanized); }, true, "MainHurtbox");

        }

        private void FinalBuffStackGainedUpdateStats(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            if (buffDef == SS2Content.Buffs.bdGalvanized)
            {
                SS2Log.Warning("awwawa : " + self.healthComponent.health + " | " + self.cursePenalty);
            }
            orig(self, buffDef);
            if (buffDef == SS2Content.Buffs.bdGalvanized)
            {
                self.RecalculateStats();
                SS2Log.Warning("awwawa : " + self.healthComponent.health + " | " + self.cursePenalty);
            }
        }

        private void FinalBuffStackLostPreventHealing(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            float? health = null;

            if (buffDef == SS2Content.Buffs.bdGalvanized)
            {
                health = self.healthComponent.health;
                SS2Log.Warning("awwawa : " + self.healthComponent.health + " | " + self.cursePenalty);

            }
            orig(self, buffDef);
            if (buffDef == SS2Content.Buffs.bdGalvanized && health != null)
            {
                self.RecalculateStats();
                self.healthComponent.health = health.Value;
                SS2Log.Warning("awwawa : " + self.healthComponent.health + " | " + self.cursePenalty);
            }

        }

        private void SetStunAddGalvanicAura(On.RoR2.SetStateOnHurt.orig_SetStun orig, SetStateOnHurt self, float duration)
        {
            orig(self, duration);
            if(self.targetStateMachine)
            {
                var cb = self.targetStateMachine.GetComponent<CharacterBody>();
                if(cb)
                {
                    var token = cb.gameObject.GetComponent<GalvanicAuraToken>();
                    if (!token)
                    {
                        token = cb.gameObject.AddComponent<GalvanicAuraToken>();
                    }

                    if (!token.aura)
                    {
                        SS2Log.Warning("WAw aW ");
                        token.aura = UnityEngine.Object.Instantiate(galvanicAura, cb.corePosition, Quaternion.identity);
                        token.aura.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(cb.gameObject, null);
                        SS2Log.Warning("token.aura : " + token.aura + " | " + token.timer);
                        token.timer = duration;
                    }
                    else
                    {
                        token.timer = duration;
                    }
                }
            }
        }

        private void SetStateDamageGalvanic(On.RoR2.SetStateOnHurt.orig_OnTakeDamageServer orig, RoR2.SetStateOnHurt self, RoR2.DamageReport damageReport)
        {
            orig(self, damageReport);
            if (!self.targetStateMachine || !self.spawnedOverNetwork)
            {
                return;
            }
            HealthComponent victim = damageReport.victim;
            DamageInfo damageInfo = damageReport.damageInfo;
            CharacterMaster attackerMaster = damageReport.attackerMaster;
            if (!victim.isInFrozenState && self.canBeStunned && attackerMaster && attackerMaster.inventory)
            {
                if(attackerMaster.inventory.GetItemCount(SS2Content.Items.GalvanicCore) > 0 && Util.CheckRoll(stunChance * 100, attackerMaster))
                {
                    EffectManager.SimpleImpactEffect(stunVFX, damageInfo.position, -damageInfo.force, true);
                    self.SetStun(2f);
                }
            }
        }
        public sealed class GalvanicCoreBuffBehavior : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdGalvanized;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (characterBody.HasBuff(SS2Content.Buffs.bdGalvanized))
                {
                    var intermediate = ((-healthPenalty) / (healthPenalty - 1f)); //makes the value input into the config accurate (ex .2 actually removes 20% of health)
                    args.baseCurseAdd += intermediate;
                    args.damageTotalMult *= (1 - damagePenalty);
                    args.moveSpeedReductionMultAdd += speedPenalty;
                }
            }
        }
    }
}
#endif