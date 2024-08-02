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

        private GameObject _hotFireVFX;
        private GameObject _fireballExplosionVFX;


        public override void Initialize()
        {
            ModifyPrefab();

            FlamethrowerDamageType = ReserveDamageType();
            FireballDamageType = ReserveDamageType();

            _hotFireVFX = AssetCollection.FindAsset<GameObject>("PyroHotFireVFX");
            _fireballExplosionVFX = AssetCollection.FindAsset<GameObject>("PyroFireballExplosionVFX");

            On.RoR2.Projectile.ProjectileSingleTargetImpact.OnProjectileImpact += PSTI_OPI;

            GlobalEventManager.onServerDamageDealt += PyroDamageChecks;
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

            /*if (DamageAPI.HasModdedDamageType(damageInfo, FireballDamageType))
            {
                float burnCount = 0;
                burnCount += victimBody.GetBuffCount(RoR2Content.Buffs.OnFire);
                burnCount += victimBody.GetBuffCount(DLC1Content.Buffs.StrongerBurn);

                if (burnCount > 0)
                {
                    BlastAttack boom = new BlastAttack()
                    {
                        radius = 8f + burnCount,
                        procCoefficient = 1f,
                        position = damageInfo.position,
                        teamIndex = attackerBody.teamComponent.teamIndex,
                        crit = Util.CheckRoll(attackerBody.crit, attackerBody.master),
                        baseDamage = attackerBody.damage + 4f + burnCount,
                        damageColorIndex = DamageColorIndex.Default,
                        falloffModel = BlastAttack.FalloffModel.SweetSpot,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        damageType = DamageType.IgniteOnHit,
                    };
                    boom.AddModdedDamageType(FireballImpactDamageType);
                    boom.Fire();

                    EffectData effectData = new EffectData()
                    {
                        scale = 8f + burnCount,
                        origin = damageInfo.position,
                        rotation = Quaternion.identity
                    };

                    EffectManager.SpawnEffect(_fireballExplosionVFX, effectData, true);
                    //EffectManager.SimpleEffect(_fireballExplosionVFX, damageInfo.position, Quaternion.identity, true);
                }
            }*/
        }

        private void PSTI_OPI(On.RoR2.Projectile.ProjectileSingleTargetImpact.orig_OnProjectileImpact orig, ProjectileSingleTargetImpact self, ProjectileImpactInfo impactInfo)
        {
            //FUCK
            if (self.projectileController.gameObject.name.Contains("PyroFireball"))
            {
                ProjectileExplosion pe = self.projectileController.gameObject.GetComponent<ProjectileExplosion>();
                if (pe)
                {
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
                                    float burnCount = 0;
                                    CharacterBody body = healthComponent.body;

                                    if (body)
                                    {
                                        burnCount += body.GetBuffCount(RoR2Content.Buffs.OnFire);
                                        burnCount += body.GetBuffCount(DLC1Content.Buffs.StrongerBurn);
                                    }

                                    pe.blastRadius += burnCount;
                                    pe.blastDamageCoefficient += burnCount;
                                }
                            }
                        }
                    }
                    pe.Detonate();
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

    }
}