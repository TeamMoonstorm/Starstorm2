
using RoR2.Projectile;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.ArtifactShell;
using Moonstorm.Starstorm2.Components;

namespace EntityStates.Pyro
{
    public class HeatWave : BaseState  //based on Enforcer's shield deflect code https://github.com/GnomeModder/EnforcerMod/blob/master/EnforcerMod_VS/Secondary.cs
    {
        public override void OnEnter()
        {
            base.OnEnter();
            this.reflectedProjectile = false;
            this.hasHeat = true;
            //this.heatLevel = FireBlast.heatCost;
            heatController = base.GetComponent<PyroHeatComponent>();
            float cost = HeatWave.heatCost * (100f / (100f + HeatWave.backupMagFuelReduction * (base.skillLocator.secondary.stock - 1)));
            this.heatLevel = Mathf.Min(heatController.GetHeat(), cost);
            if (heatLevel < cost)
            {
                if (heatLevel == 0f)
                {
                    this.hasHeat = false;
                }
                else
                {
                    heatController.ConsumeHeat(heatLevel);
                }

            }
            else
            {
                heatController.ConsumeHeat(cost);
            }

            if (!HeatWave.effectPrefab)
            {
                HeatWave.effectPrefab = (Instantiate(typeof(EntityStates.Treebot.Weapon.FireSonicBoom)) as EntityStates.Treebot.Weapon.FireSonicBoom).fireEffectPrefab;
            }
            EffectManager.SimpleMuzzleFlash(HeatWave.effectPrefab, base.gameObject, "MuzzleLeft", false);  //TODO: need separate effects for heated/unheated

            Util.PlaySound(HeatWave.attackSoundString, base.gameObject);

            this.aimRay = base.GetAimRay();
            this.childLocator = base.GetModelTransform().GetComponent<ChildLocator>();

            base.StartAimMode(aimRay, 2f, false);

            if (base.characterBody && base.characterMotor && !base.characterMotor.isGrounded)
            {
                if (base.characterMotor.velocity.y < 0f)
                {
                    base.characterMotor.velocity.y = 0f;
                }
                base.characterMotor.ApplyForce(-aimRay.direction * HeatWave.selfForce, true, false);
            }

            if (NetworkServer.active)
            {
                PushEnemiesServer();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (NetworkServer.active && base.fixedAge < HeatWave.reflectWindowDuration)
            {
                DeflectServer();
            }

            if (base.fixedAge > HeatWave.baseDuration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        private void DeflectServer()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            Ray aimRay = base.GetAimRay();

            bool reflected = false;

            Collider[] array = Physics.OverlapBox(base.transform.position + aimRay.direction * HeatWave.hitboxOffset, HeatWave.hitboxDimensions, Quaternion.LookRotation(aimRay.direction, Vector3.up), LayerIndex.projectile.mask);
            for (int i = 0; i < array.Length; i++)
            {
                ProjectileController pc = array[i].GetComponentInParent<ProjectileController>();
                if (pc)
                {
                    if (pc.owner != base.gameObject)
                    {
                        Vector3 aimSpot = (aimRay.origin + 100 * aimRay.direction) - pc.gameObject.transform.position;

                        pc.owner = base.gameObject;

                        FireProjectileInfo info = new FireProjectileInfo()
                        {
                            projectilePrefab = reflectProjectilePrefab,
                            position = pc.gameObject.transform.position,
                            rotation = base.characterBody.transform.rotation * Quaternion.FromToRotation(new Vector3(0, 0, 1), aimSpot),
                            owner = base.characterBody.gameObject,
                            damage = base.characterBody.damage * HeatWave.reflectDamageCoefficient,
                            force = 3000f,
                            crit = base.RollCrit(),
                            damageColorIndex = DamageColorIndex.Default,
                            target = null,
                            speedOverride = 90f,
                            useSpeedOverride = true
                        };
                        ProjectileManager.instance.FireProjectile(info);

                        Destroy(pc.gameObject);

                        if (!reflected)
                        {
                            reflected = true;
                            Util.PlaySound(HeatWave.reflectSoundString, base.gameObject);
                        }
                    }
                }
            }
        }

        private void PushEnemiesServer()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            List<HealthComponent> hcList = new List<HealthComponent>();
            Collider[] array = Physics.OverlapBox(base.transform.position + aimRay.direction * HeatWave.hitboxOffset, HeatWave.hitboxDimensions, Quaternion.LookRotation(aimRay.direction, Vector3.up), LayerIndex.entityPrecise.mask);
            for (int i = 0; i < array.Length; i++)
            {
                HurtBox hurtBox = array[i].GetComponent<HurtBox>();
                if (hurtBox)
                {
                    HealthComponent healthComponent = hurtBox.healthComponent;
                    ProjectileController pc = array[i].GetComponentInParent<ProjectileController>();
                    if (healthComponent && !pc && !hcList.Contains(healthComponent))
                    {
                        hcList.Add(healthComponent);
                        TeamComponent component2 = healthComponent.GetComponent<TeamComponent>();
                        if (component2.teamIndex != base.teamComponent.teamIndex)
                        {
                            CharacterBody cb = healthComponent.body;
                            if (cb)
                            {
                                Vector3 forceVector = HeatWave.force * aimRay.direction;
                                Rigidbody rb = cb.rigidbody;
                                if (rb)
                                {
                                    forceVector *= Mathf.Min(Mathf.Max(rb.mass / 100f, 1f), maxForceScale);
                                }

                                healthComponent.TakeDamageForce(new DamageInfo
                                {
                                    attacker = base.gameObject,
                                    inflictor = base.gameObject,
                                    damage = 0f,
                                    damageColorIndex = DamageColorIndex.Default,
                                    damageType = DamageType.NonLethal,
                                    crit = false,
                                    dotIndex = DotController.DotIndex.None,
                                    force = forceVector,
                                    position = base.transform.position,
                                    procChainMask = default(ProcChainMask),
                                    procCoefficient = 0f
                                }, true, true);

                                if (this.hasHeat)   //only deal damage if there is sufficient heat
                                {
                                    float heatMult = this.heatLevel / HeatWave.heatCost;
                                    healthComponent.TakeDamage(new DamageInfo
                                    {
                                        attacker = base.gameObject,
                                        inflictor = base.gameObject,
                                        damage = this.damageStat * HeatWave.damageCoefficient * heatMult,
                                        damageColorIndex = DamageColorIndex.Default,
                                        damageType = DamageType.IgniteOnHit,
                                        crit = base.RollCrit(),
                                        dotIndex = DotController.DotIndex.None,
                                        force = Vector3.zero,
                                        position = base.transform.position,
                                        procChainMask = default(ProcChainMask),
                                        procCoefficient = 1f
                                    });

                                    GlobalEventManager.instance.OnHitEnemy(new DamageInfo
                                    {
                                        attacker = base.gameObject,
                                        inflictor = base.gameObject,
                                        damage = this.damageStat * HeatWave.damageCoefficient * heatMult,
                                        damageColorIndex = DamageColorIndex.Default,
                                        damageType = DamageType.IgniteOnHit,
                                        crit = base.RollCrit(),
                                        dotIndex = DotController.DotIndex.None,
                                        force = Vector3.zero,
                                        position = base.transform.position,
                                        procChainMask = default(ProcChainMask),
                                        procCoefficient = 1f
                                    }, healthComponent.gameObject);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public static string attackSoundString = "Play_treeBot_shift_shoot";
        public static string reflectSoundString = "Play_loader_m1_swing";
        public static float baseDuration = 0.75f;
        public static float damageCoefficient = 4f;
        public static float reflectWindowDuration = 0.125f;
        public static Vector3 hitboxDimensions = new Vector3(7.5f, 3.75f, 12f);
        public static float force = 2700f;
        public static float selfForce = 2700f;
        public static float heatCost = 0.3f;
        public static GameObject effectPrefab = null;
        public static float maxForceScale = 20f;
        public static float backupMagFuelReduction = 15f;

        public static GameObject reflectProjectilePrefab;
        public static float reflectDamageCoefficient = 4f;

        private ChildLocator childLocator;
        private Ray aimRay;
        private PyroHeatComponent heatController;
        private bool reflectedProjectile;
        private bool hasHeat;

        private float heatLevel;

        private static float hitboxOffset = (HeatWave.hitboxDimensions.z / 2f - 0.5f);
    }
}