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

        public static GameObject galvOrbVFX;
        public static GameObject galvOrbHit;

        public static float healthPenaltyCalculated;

        public override void Initialize()
        {
            On.RoR2.SetStateOnHurt.OnTakeDamageServer += SetStateDamageGalvanic;
            On.RoR2.SetStateOnHurt.SetStun += SetStunAddGalvanicAura;

            //On.RoR2.CharacterBody.OnBuffFirstStackGained += FirstStackGainedGalvanized;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += FinalBuffStackLostPreventHealing;

            IL.RoR2.CharacterBody.RecalculateStats += RecalculateStatsPreventHealing;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsGalvanized;


            stunVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/StunChanceOnHit/ImpactStunGrenade.prefab").WaitForCompletion(); //LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade")
            
            galvanicAura = AssetCollection.FindAsset<GameObject>("GalvanicAura");
            gwbVFX = AssetCollection.FindAsset<GameObject>("GreaterBannerBuffEffectTEMP");

            galvOrbVFX = AssetCollection.FindAsset<GameObject>("GalvanicOrbEffect");
            galvOrbHit = AssetCollection.FindAsset<GameObject>("GalvanicHitEffect");

            //TempVisualEffectAPI.AddTemporaryVisualEffect(galvAuraVFX.InstantiateClone("GalvanicAuraTempEffectInstance", false), (CharacterBody body) => { return body.HasBuff(SS2Content.Buffs.bdGalvanizedSource); }, true, "MainHurtbox");
            TempVisualEffectAPI.AddTemporaryVisualEffect(gwbVFX.InstantiateClone("GreaterBannerBuffEffectTWOTEMP", false), (CharacterBody body) => { return body.HasBuff(SS2Content.Buffs.bdGalvanized); }, true, "MainHurtbox");

            healthPenaltyCalculated = ((-healthPenalty) / (healthPenalty - 1f)); //makes the value input into the config accurate (ex .2 actually removes 20% of health)
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }
        private void RecalculateStatsPreventHealing(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel? label = null;
            bool ILFound = c.TryGotoNext(MoveType.Before,
                x => x.MatchBleUn(out label), //the if else's jump statement
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_healthComponent"),
                x => x.MatchLdloc(118) //the first variable before the proc chain mask
            );

            SS2Log.Warning("wawa " + ILFound + " | " + label);
            if (ILFound && label != null)
            {
                c.Index += 1;
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<CharacterBody, bool>>((self) =>
                {
                    var token = self.gameObject.GetComponent<GalvanizedMarker>();
                    if(token && token.preventHeal)
                    {
                        SS2Log.Warning("Preventing Healing via preventHeal");
                        token.preventHeal = false;
                        return false;
                    }
                    return true;
                });
                c.Emit(OpCodes.Brfalse, label);
            }
            else
            {
                SS2Log.Error("Galvanic Core prevent curse healing hook failed.");
                //maybe implment a failsafe with the damage fixing? it'd still kill at low health, but would prevent the item from Not Being Fun
            }
        }

        private void FinalBuffStackLostPreventHealing(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            //float? health = null;

            if (buffDef == SS2Content.Buffs.bdGalvanized)
            {
                //health = self.healthComponent.health;
                //self.SetAmputateMaxHealth(self.amputatedMaxHealth - healthPenaltyCalculated);
                //SS2Log.Warning("awwawa : " + self.healthComponent.health + " | " + self.cursePenalty);
                var token = self.gameObject.GetComponent<GalvanizedMarker>();
                if (!token)
                {
                    token = self.gameObject.AddComponent<GalvanizedMarker>();
                }
                token.preventHeal = true;


            }
            orig(self, buffDef);
            //if (buffDef == SS2Content.Buffs.bdGalvanized && health != null)
            //{
            //    //self.SetAmputateMaxHealth(self.amputatedMaxHealth - healthPenaltyCalculated);
            //    ////self.RecalculateStats();
            //    //float intendedHealth = self.healthComponent.fullHealth / (1 - healthPenalty);
            //    //self.healthComponent.Networkhealth = health.Value - (intendedHealth * healthPenalty);
            //    
            //    SS2Log.Warning("awwawa : " + self.healthComponent.health + " | " + self.cursePenalty);
            //    
            //}
        }

        private void RecalculateStatsGalvanized(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.GetBuffCount(SS2Content.Buffs.bdGalvanized) > 0)
            {
                args.baseCurseAdd += healthPenaltyCalculated;
                args.damageTotalMult *= (1 - damagePenalty);
                args.moveSpeedReductionMultAdd += speedPenalty;
            }
            //SS2Log.Warning("wawa 3");
            //var token = sender.gameObject.GetComponent<GalvanizedMarker>();
            //if (token && token.preventHeal)
            //{
            //    token.preventHeal = false;
            //    var hc = sender.healthComponent;
            //
            //    //float intendedHealth = self.healthComponent.fullHealth / (1 - healthPenalty);
            //    //sender.healthComponent.Networkhealth = health.Value - (intendedHealth * healthPenalty);
            //    sender.healthComponent.Networkhealth = hc.health - (hc.fullHealth * healthPenalty);
            //}
            //SS2Log.Warning("health: " + sender.healthComponent.health);
        }

        private void SetStunAddGalvanicAura(On.RoR2.SetStateOnHurt.orig_SetStun orig, SetStateOnHurt self, float duration)
        {
            orig(self, duration);
            if(self.targetStateMachine)
            {
                var cb = self.targetStateMachine.GetComponent<CharacterBody>();
                //SS2Log.Warning("waaawa. : ");
                if (cb && cb.healthComponent.lastHitAttacker) 
                { 
                    //SS2Log.Warning("cb.healthComponent.lastHitAttacker : " + cb.healthComponent.lastHitAttacker);
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
                if(attackerMaster.inventory.GetItemCount(SS2Content.Items.GalvanicCore) > 0 && Util.CheckRoll(stunChance * 100, attackerMaster))
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

            public float timer;

            protected override void OnFirstStackGained()
            {
                base.OnFirstStackGained();
                aura = UnityEngine.Object.Instantiate(galvanicAura, characterBody.corePosition, Quaternion.identity);
                aura.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(characterBody.gameObject, null);
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
                    for (TeamIndex teamIndex = TeamIndex.Neutral; teamIndex < TeamIndex.Count; teamIndex += 1)
                    {
                        if (teamIndex != TeamIndex.Player)
                        {
                            foreach(var member in TeamComponent.GetTeamMembers(teamIndex))
                            {
                                SS2Log.Warning(": " + member + " | " + member.GetComponent<CharacterBody>());
                            }
                            BuffTeam(TeamComponent.GetTeamMembers(teamIndex), 15^2, characterBody.corePosition);
                        }
                    }
                }
            }

            private void BuffTeam(IEnumerable<TeamComponent> recipients, float radiusSqr, Vector3 currentPosition)
            {
                SS2Log.Warning("Buff Team Started");
                foreach (TeamComponent teamComponent in recipients)
                {
                    Vector3 vector = teamComponent.transform.position - currentPosition;
                    vector.y = 0f;

                    if (vector.sqrMagnitude <= radiusSqr)
                    {
                        CharacterBody component = teamComponent.GetComponent<CharacterBody>();
                        if (component && !component.HasBuff(SS2Content.Buffs.bdGalvanizedSource))
                        {
                            GalvanicOrb galvOrb = new GalvanicOrb();
                            galvOrb.origin = characterBody.aimOrigin;

                            if (component.mainHurtBox)
                            {
                                    SS2Log.Warning("wawa : " + characterBody.corePosition + " | " + component.mainHurtBox.transform.position);
                                    galvOrb.target = component.mainHurtBox;
                                    OrbManager.instance.AddOrb(galvOrb);
                            }
                            component.AddTimedBuff(SS2Content.Buffs.bdGalvanized.buffIndex, .4f);
                        }
                        else if (component)
                        {
                            SS2Log.Warning("Had Buff: " + component);
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
                base.OnArrival();
            }

            public GameObject attacker;

            public TeamIndex teamIndex;
        }
    }
}