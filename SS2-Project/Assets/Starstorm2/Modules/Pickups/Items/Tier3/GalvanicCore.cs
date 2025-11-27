using MonoMod.Cil;
using Mono.Cecil.Cil;
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
using RoR2.Orbs;
using SS2.Components;

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

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Stacking chance to stun on hit. (1 = 100%)")]
        [FormatToken("SS2_ITEM_GALVANICCORE_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 4)]
        public static float stunChanceStacking = .1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Radius of the aura around stunned enemies. (1 = 1m radius)")]
        [FormatToken("SS2_ITEM_GALVANICCORE_DESC", 5)]
        public static float baseAuraRadius = 15;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Stacking radius increase of the aura around stunned enemies. (1 = 1m radius)")]
        [FormatToken("SS2_ITEM_GALVANICCORE_DESC", 6)]
        public static float scalingAuraRadius = 5;
        public static GameObject stunVFX;
        public static GameObject galvanicAura;

        public static GameObject galvOrbVFX;
        public static GameObject galvOrbHit;

        public static float healthPenaltyCalculated;

        public override void Initialize()
        {
            On.RoR2.SetStateOnHurt.OnTakeDamageServer += SetStateDamageGalvanic;
            On.RoR2.SetStateOnHurt.SetStun += SetStunAddGalvanicAura;

            On.RoR2.CharacterBody.OnBuffFinalStackLost += FinalBuffStackLostPreventHealing;
            IL.RoR2.CharacterBody.RecalculateStats += RecalculateStatsPreventHealing;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsGalvanized;


            stunVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/StunChanceOnHit/ImpactStunGrenade.prefab").WaitForCompletion();
            galvanicAura = AssetCollection.FindAsset<GameObject>("GalvanicAura");

            galvOrbVFX = AssetCollection.FindAsset<GameObject>("GalvanicOrbEffect");
            galvOrbHit = AssetCollection.FindAsset<GameObject>("GalvanicHitEffect");

            healthPenaltyCalculated = ((-healthPenalty) / (healthPenalty - 1f)); //makes the value input into the config accurate (ex .2 actually removes 20% of health)
        }

        private void RecalculateStatsPreventHealing(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel? label = null;
            bool ILFound = c.TryGotoNext(MoveType.Before,
                x => x.MatchBleUn(out label), //the if else's jump statement
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_healthComponent"),
                x => x.MatchLdloc(152) //the first variable before the proc chain mask
            );

            if (ILFound && label != null)
            {
                c.Index += 1;
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<CharacterBody, bool>>((self) =>
                {
                    var token = self.gameObject.GetComponent<GalvanizedMarker>();
                    if(token && token.preventHeal)
                    {
                        token.preventHeal = false;
                        return false;
                    }
                    return true;
                });
                c.Emit(OpCodes.Brfalse, label);
            }
            else
            {
                SS2Log.Fatal("Galvanic Core prevent curse healing hook failed.");
                //maybe implment a failsafe with the damage fixing? it'd still kill at low health, but would prevent the item from Not Being Fun
            }
        }

        private void FinalBuffStackLostPreventHealing(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            if (buffDef == SS2Content.Buffs.bdGalvanized)
            {
                var token = MSU.MSUtil.EnsureComponent<GalvanizedMarker>(self.gameObject);
                token.preventHeal = true;
            }
            orig(self, buffDef);
        }

        private void RecalculateStatsGalvanized(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.GetBuffCount(SS2Content.Buffs.bdGalvanized) > 0)
            {
                args.baseCurseAdd += healthPenaltyCalculated;
                //args.damageTotalMult *= (1 - damagePenalty);
                args.moveSpeedReductionMultAdd += speedPenalty;
            }
        }

        private void SetStunAddGalvanicAura(On.RoR2.SetStateOnHurt.orig_SetStun orig, SetStateOnHurt self, float duration)
        {
            orig(self, duration);
            if(self.targetStateMachine)
            {
                var cb = self.targetStateMachine.GetComponent<CharacterBody>();
                if (cb && cb.healthComponent.lastHitAttacker) 
                { 
                    var attacker = cb.healthComponent.lastHitAttacker.GetComponent<CharacterBody>();
                    if (attacker.inventory.GetItemCount(SS2Content.Items.GalvanicCore) > 0)
                    {
                        cb.AddTimedBuff(SS2Content.Buffs.bdGalvanizedSource, duration);
                    }
                }
            }
        }

        private void SetStateDamageGalvanic(On.RoR2.SetStateOnHurt.orig_OnTakeDamageServer orig, RoR2.SetStateOnHurt self, RoR2.DamageReport damageReport)
        {
            if(damageReport.victim && damageReport.attacker)
            {
                damageReport.victim.lastHitAttacker = damageReport.attacker; //fixes stun working on first hits 
            }

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
                var count = attackerMaster.inventory.GetItemCount(SS2Content.Items.GalvanicCore);
                var combinedStunChance = stunChance + (stunChanceStacking * (count - 1));

                var calculatedStunChance = combinedStunChance / (combinedStunChance + 1);

                if (count > 0 && Util.CheckRoll(calculatedStunChance * 100 * damageInfo.procCoefficient, attackerMaster))
                {
                    EffectManager.SimpleImpactEffect(stunVFX, damageInfo.position, -damageInfo.force, true);
                    self.SetStun(2f);
                }
            }
        }

        public sealed class GalvanicCoreSourceBuffBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdGalvanizedSource;

            private GameObject aura;

            public bool doVFX = true;
            public float timer;

            private int attackerStacks = 1;
            private float radiusSquared = baseAuraRadius * baseAuraRadius;
            private TeamIndex attackerTeam = TeamIndex.Player;

            protected override void OnFirstStackGained()
            {
                base.OnFirstStackGained();
                aura = UnityEngine.Object.Instantiate(galvanicAura, characterBody.corePosition, Quaternion.identity);
                aura.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(characterBody.gameObject, null);

                if(characterBody.healthComponent.lastHitAttacker 
                    && characterBody.healthComponent.lastHitAttacker.TryGetComponent<CharacterBody>(out var attacker)){

                    attackerStacks = attacker.GetItemCount(SS2Content.Items.GalvanicCore);
                    if(attackerStacks > 1) //If they only have one stack, the default values will work
                    {
                        float sizeMultiplier = 1 + ((attackerStacks - 1) * (scalingAuraRadius / baseAuraRadius));
                        aura.transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier, sizeMultiplier);
                        float radius = baseAuraRadius * sizeMultiplier;
                        radiusSquared = radius * radius;
                    }

                    attackerTeam = attacker.teamComponent.teamIndex;
                }
                AttemptBuff();
            }

            protected override void OnAllStacksLost()
            {
                base.OnAllStacksLost();
                if (aura)
                {
                    Destroy(aura);
                }
            }

            public void FixedUpdate()
            {
                timer += Time.fixedDeltaTime;
                if(timer > .25 && NetworkServer.active)
                {
                    timer = 0;
                    AttemptBuff();
                }
            }

            private void AttemptBuff()
            {
                if(characterBody.healthComponent && characterBody.healthComponent.alive)
                {
                    for (TeamIndex teamIndex = TeamIndex.Neutral; teamIndex < TeamIndex.Count; teamIndex += 1)
                    {
                        if (teamIndex != attackerTeam)
                        {
                            BuffTeam(TeamComponent.GetTeamMembers(teamIndex), radiusSquared, characterBody.corePosition);
                        }
                    }
                    doVFX = !doVFX; //Makes it so we only spawn the VFX orbs every other tick of the buffward, reducing particle spam
                }
                else
                {
                    characterBody.RemoveBuff(SS2Content.Buffs.bdGalvanizedSource); //If we're dead or broken, immediately destroy this behavior
                }
            }

            private void BuffTeam(IEnumerable<TeamComponent> recipients, float radiusSqr, Vector3 currentPosition)
            {
                foreach (TeamComponent teamComponent in recipients)
                {
                    Vector3 vector = teamComponent.transform.position - currentPosition;
                    vector.y = 0f;
                    if (vector.sqrMagnitude <= radiusSqr)
                    {
                        CharacterBody targetBody = teamComponent.GetComponent<CharacterBody>();
                        if (!targetBody)
                        {
                            continue;
                        }

                        targetBody.AddTimedBuff(SS2Content.Buffs.bdGalvanized.buffIndex, .4f);
                        if (targetBody == characterBody)
                        {
                            continue; //If they're the source of VFX, we don't need to send an orb at them
                        }

                        if (doVFX && targetBody.mainHurtBox && targetBody.healthComponent && targetBody.healthComponent.alive)
                        {
                            GalvanicOrb galvOrb = new GalvanicOrb();
                            galvOrb.origin = characterBody.corePosition;

                            galvOrb.duration = 0.1f;
                            galvOrb.origin = characterBody.aimOrigin;
                            galvOrb.teamIndex = characterBody.teamComponent.teamIndex;

                            galvOrb.target = targetBody.mainHurtBox;
                            OrbManager.instance.AddOrb(galvOrb);
                        }
                    }
                }
            }
        }
    
        public class GalvanizedMarker : MonoBehaviour
        {
            public bool preventHeal = false;
        }

        public class GalvanicOrb : Orb //Harmless vfx orb
        {
            public override void Begin()
            {
                base.duration = 0.2f;

                EffectData effectData = new EffectData
                {
                    origin = this.origin,
                    genericFloat = base.duration
                };
                effectData.SetHurtBoxReference(this.target);

                EffectManager.SpawnEffect(galvOrbVFX, effectData, true);
            }

            public override void OnArrival() 
            {

            }

            public TeamIndex teamIndex;
        }
    }
}