using SS2.Components;
using RoR2;
using System;
using UnityEngine;
using MSU;
using R2API;
using RoR2.ContentManagement;
using static R2API.DamageAPI;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace SS2.Survivors
{
    public sealed class Pyro : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acPyro", SS2Bundle.Indev);
        public static ModdedDamageType FlamethrowerDamageType { get; private set; }
        public static ModdedDamageType FireballDamageType { get; private set; }
        public static ModdedDamageType FireballImpactDamageType { get; private set; }

        public static GameObject _hotFireVFX;
        public static GameObject _fireballExplosionVFX;
        public static HeatSkillDef _jetpackOverrideDef;
        public static BuffDef _bdPyroManiac;
        public static BuffDef _bdPyroJet;

        public override void Initialize()
        {
            ModifyPrefab();

            FlamethrowerDamageType = ReserveDamageType();
            FireballDamageType = ReserveDamageType();

            _hotFireVFX = AssetCollection.FindAsset<GameObject>("PyroHotFireVFX");
            _fireballExplosionVFX = AssetCollection.FindAsset<GameObject>("PyroFireballExplosionVFX");
            _bdPyroManiac = AssetCollection.FindAsset<BuffDef>("bdPyroManiac");
            _bdPyroJet = AssetCollection.FindAsset<BuffDef>("bdPyroJet");
            _jetpackOverrideDef = AssetCollection.FindAsset<HeatSkillDef>("sdPyro3a");

            On.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact += PSTI_OPI;

            //GlobalEventManager.onServerDamageDealt += PyroDamageChecks;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.DotController.AddDot_GameObject_float_DotIndex_float_Nullable1_Nullable1_Nullable1 += DotController_AddDot_GameObject_float_DotIndex_float_Nullable1_Nullable1_Nullable1;
            //On.RoR2.DotController.OnDotStackAddedServer += DotController_OnDotStackAddedServer;
        }

        private void DotController_AddDot_GameObject_float_DotIndex_float_Nullable1_Nullable1_Nullable1(On.RoR2.DotController.orig_AddDot_GameObject_float_DotIndex_float_Nullable1_Nullable1_Nullable1 orig, DotController self, GameObject attackerObject, float duration, DotController.DotIndex dotIndex, float damageMultiplier, uint? maxStacksFromAttacker, float? totalDamage, DotController.DotIndex? preUpgradeDotIndex)
        {
            if (self.victimBody.baseNameToken == "SS2_PYRO_NAME" && (preUpgradeDotIndex == DotController.DotIndex.Burn || preUpgradeDotIndex == DotController.DotIndex.PercentBurn || preUpgradeDotIndex == DotController.DotIndex.StrongerBurn))
            {
                return;
            }

            orig(self, attackerObject, duration, dotIndex, damageMultiplier, maxStacksFromAttacker, totalDamage, preUpgradeDotIndex);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.HasModdedDamageType(FlamethrowerDamageType))
            {
                CharacterBody attackerBody = damageInfo.attacker.transform.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    PyroController pc = attackerBody.GetComponent<PyroController>();
                    if (pc != null)
                    {
                        float distance = Vector3.Distance(damageInfo.position, attackerBody.corePosition);

                        if (distance < 6f)
                        {
                            //Debug.Log("pre damage: " + damageInfo.damage);
                            damageInfo.damage *= 1.5f;
                            damageInfo.force *= 3f;
                            //Debug.Log("post damage: " + damageInfo.damage);
                            damageInfo.damageColorIndex = DamageColorIndex.WeakPoint;
                            EffectManager.SimpleEffect(_hotFireVFX, damageInfo.position, Quaternion.identity, true);
                            if (Util.CheckRoll(75f, attackerBody.master) && pc.heat >= 35f)
                                damageInfo.damageType = DamageType.IgniteOnHit;
                        }
                        else if (Util.CheckRoll(50f, attackerBody.master) && pc.heat >= 65f)
                        {
                            damageInfo.damageType = DamageType.IgniteOnHit;
                        }
                    }
                }
            }

            // ofmg kill urp ast self
            if (self.body.baseNameToken == "SS2_PYRO_NAME")
            {
                //needs redone
                if (self.body.HasBuff(RoR2Content.Buffs.OnFire) || self.body.HasBuff(DLC1Content.Buffs.StrongerBurn))
                {
                    self.body.SetBuffCount(RoR2Content.Buffs.OnFire.buffIndex, 0);
                    self.body.SetBuffCount(DLC1Content.Buffs.StrongerBurn.buffIndex, 0);
                }
            }

            orig(self, damageInfo);
        }

        private void PSTI_OPI(On.RoR2.Projectile.ProjectileSingleTargetImpact.orig_OnProjectileImpact orig, ProjectileSingleTargetImpact self, ProjectileImpactInfo impactInfo)
        {
            //FUCK
            if (self.projectileController.gameObject.name.Contains("PyroFireball"))
            {
                ProjectileExplosion pe = self.projectileController.gameObject.GetComponent<ProjectileExplosion>();
                if (pe)
                {
                    float burnCount = 0;
                    Collider collider = impactInfo.collider;
                    if (collider)
                    {
                        HurtBox component = collider.GetComponent<HurtBox>();
                        {
                            if (component && component.hurtBoxGroup)
                            {
                                HealthComponent healthComponent = component.healthComponent;
                                if (healthComponent)
                                {
                                    CharacterBody body = healthComponent.body;

                                    if (body)
                                    {
                                        burnCount += body.GetBuffCount(RoR2Content.Buffs.OnFire);
                                        burnCount += body.GetBuffCount(DLC1Content.Buffs.StrongerBurn);
                                        burnCount += 1.5f; //we do a little trolling
                                    }

                                    pe.blastRadius += burnCount;
                                    pe.blastDamageCoefficient += burnCount;
                                }
                            }
                            else
                                pe.blastRadius /= 2f;
                        }
                    }

                    pe.Detonate();

                    EffectData effectData = new EffectData()
                    {
                        scale = pe.blastRadius,
                        origin = impactInfo.estimatedPointOfImpact,
                        rotation = Quaternion.identity
                    };
                    EffectManager.SpawnEffect(_fireballExplosionVFX, effectData, true);
                }
            }

            orig(self, impactInfo);
        }

        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }

        public sealed class PyromaniacBuffBehavior : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => _bdPyroManiac;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (hasAnyStacks)
                {
                    args.armorAdd += Math.Max(3f * buffCount, 30f);
                    args.regenMultAdd += Math.Max(0.2f * buffCount, 2f);
                }
            }
        }

        public sealed class PyroJetBuffBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => _bdPyroJet;

            private bool isOverridingSkill;
            private HeatSkillDef overrideSkill = _jetpackOverrideDef;
            private SkillLocator skillLocator;

            public void OnStart()
            {
                skillLocator = characterBody.skillLocator;
            }

            public void FixedUpdate()
            {
                if (skillLocator == null)
                {
                    skillLocator = characterBody.skillLocator;
                }

                if (hasAnyStacks && !isOverridingSkill)
                {
                    isOverridingSkill = true;

                    skillLocator.utility.SetSkillOverride(this, overrideSkill, GenericSkill.SkillOverridePriority.Contextual);
                }

                if (!hasAnyStacks && isOverridingSkill)
                {
                    isOverridingSkill = false;

                    skillLocator.utility.UnsetSkillOverride(this, overrideSkill, GenericSkill.SkillOverridePriority.Contextual);
                }

                if (characterBody.characterMotor.isGrounded && isOverridingSkill)
                {
                    characterBody.SetBuffCount(buffIndex, 0);

                    isOverridingSkill = false;

                    skillLocator.utility.UnsetSkillOverride(this, overrideSkill, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
        }
    }
}