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

        public override void Initialize()
        {
            On.RoR2.SetStateOnHurt.OnTakeDamageServer += SetStateDamageGalvanic;
            On.RoR2.SetStateOnHurt.SetStun += SetStunAddGalvanicAura;

            On.RoR2.BuffWard.BuffTeam += Behave;

            stunVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/StunChanceOnHit/ImpactStunGrenade.prefab").WaitForCompletion(); //LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade")
            galvanicAura = AssetCollection.FindAsset<GameObject>("GalvanicAura");
        }

        private void Behave(On.RoR2.BuffWard.orig_BuffTeam orig, BuffWard self, IEnumerable<TeamComponent> recipients, float radiusSqr, Vector3 currentPosition)
        {
            //orig(self, recipients, radiusSqr, currentPosition);
            SS2Log.Warning("self " + self.buffDef + " | ");
            foreach(var target in recipients)
            {
                SS2Log.Warning("target: " + target);
            }

            foreach (TeamComponent teamComponent in recipients)
            {
                Vector3 vector = teamComponent.transform.position - currentPosition;
                if (self.shape == BuffWard.BuffWardShape.VerticalTube)
                {
                    vector.y = 0f;
                }
                SS2Log.Warning("health component: " + teamComponent + " | " + vector.sqrMagnitude + " | " + radiusSqr);
                if (vector.sqrMagnitude <= radiusSqr)
                {
                    CharacterBody component = teamComponent.GetComponent<CharacterBody>();
                    SS2Log.Warning("cb: " + component + " | rq grounded: " + !self.requireGrounded + " | nomotor: " + !component.characterMotor);
                    if (!component.characterMotor)
                    {
                        SS2Log.Warning("grounded: " + component.characterMotor.isGrounded);
                    }
                    if (component && (!self.requireGrounded || !component.characterMotor || component.characterMotor.isGrounded))
                    {
                        SS2Log.Warning("winning : " + self.buffDuration);
                        component.AddTimedBuff(self.buffDef.buffIndex, self.buffDuration);
                    }
                }
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
                        token.timer = duration * 999;
                    }
                    else
                    {
                        token.timer = duration * 999;
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