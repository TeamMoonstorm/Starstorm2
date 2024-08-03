using Mono.Cecil.Cil;
using MonoMod.Cil;
using SS2.Components;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using MSU;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using R2API;
using RoR2.ContentManagement;
using static R2API.DamageAPI;
using RoR2.Projectile;

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

            On.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact += PSTI_OPI;

            //GlobalEventManager.onServerDamageDealt += PyroDamageChecks;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            //On.RoR2.DotController.OnDotStackAddedServer += DotController_OnDotStackAddedServer;
        }


        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void PyroDamageChecks(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;

            if (DamageAPI.HasModdedDamageType(damageInfo, FlamethrowerDamageType))
            {
                PyroController pc = attackerBody.GetComponent<PyroController>();
                if (pc == null)
                    return;

                float distance = Vector3.Distance(victimBody.corePosition, attackerBody.corePosition);

                if (distance > 17.5f)
                {
                    //Debug.Log("pre damage: " + damageInfo.damage);
                    damageInfo.damage *= 1.5f;
                    //Debug.Log("post damage: " + damageInfo.damage);
                    damageInfo.damageColorIndex = DamageColorIndex.WeakPoint;
                    EffectManager.SimpleEffect(_hotFireVFX, victimBody.transform.position, Quaternion.identity, true);
                    if (Util.CheckRoll(50f, attackerBody.master) && pc.heat >= 30f)
                        damageInfo.damageType = DamageType.IgniteOnHit;
                }
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.HasModdedDamageType(FlamethrowerDamageType))
            {
                CharacterBody attackerBody = damageInfo.attacker.transform.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    PyroController pc = attackerBody.GetComponent<PyroController>();
                    if (pc == null)
                        return;

                    float distance = Vector3.Distance(damageInfo.position, attackerBody.corePosition);

                    if (distance > 17.5f)
                    {
                        //Debug.Log("pre damage: " + damageInfo.damage);
                        damageInfo.damage *= 1.5f;
                        //Debug.Log("post damage: " + damageInfo.damage);
                        damageInfo.damageColorIndex = DamageColorIndex.WeakPoint;
                        EffectManager.SimpleEffect(_hotFireVFX, damageInfo.position, Quaternion.identity, true);
                        if (Util.CheckRoll(50f, attackerBody.master) && pc.heat >= 30f)
                            damageInfo.damageType = DamageType.IgniteOnHit;
                    }
                }
            }

            if (self.body.baseNameToken == "SS2_PYRO_NAME")
            {
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
                if (HasAnyStacks)
                {
                    args.armorAdd += Math.Max(2f * BuffCount, 10f);
                    args.regenMultAdd += Math.Max(0.1f * BuffCount, 0.5f);
                }
            }
        }

        public sealed class PyroJetBuffBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => _bdPyroJet;

            public void FixedUpdate()
            {
                if (HasAnyStacks)
                {
                    CharacterBody.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y * 0.2f;
                }
            }

        }
    }
}