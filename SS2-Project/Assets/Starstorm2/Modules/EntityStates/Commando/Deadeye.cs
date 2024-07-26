using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Commando
{
    public class Deadeye : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        [SerializeField]
        public static float damageCoeff = 1.65f;

        [SerializeField]
        public static float procCoeff = 0.7f;

        public float baseDuration = 0.5f;
        private float duration;

        public GameObject hitEffectPrefab = FireBarrage.hitEffectPrefab;
        public GameObject tracerEffectPrefab = FireBarrage.tracerEffectPrefab;

        private int pistolSide = 0;
        private string pistolEffectMuzzleString = "FirePistol, Right";
        private string pistolEffectAnimationString = "Gesture Additive, Right";

        private void PlayPistolAnimation()
        {
            base.PlayAnimation(pistolEffectAnimationString, pistolEffectMuzzleString);
            if (FireBarrage.effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(FireBarrage.effectPrefab, base.gameObject, "MuzzleLeft", false);
            }
            
        }

        void SteppedSkillDef.IStepSetter.SetStep(int i)
        {
            pistolSide = i;
            pistolEffectMuzzleString = (pistolSide == 0) ? "FirePistol, Left" : "FirePistol, Right";
            pistolEffectAnimationString = (pistolSide == 0) ? "Gesture Additive, Left" : "Gesture Additive, Right";
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)pistolSide);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            pistolSide = (int)reader.ReadByte();
        }


        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / base.attackSpeedStat;
            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);

            PlayPistolAnimation();
            Util.PlaySound(FireBarrage.fireBarrageSoundString, base.gameObject);
            base.AddRecoil(-0.6f, 0.6f, -0.6f, 0.6f);

            // TODO: This is manually set to true for testing, undo this in prod
            var isCrit = true; //base.RollCrit();

            if (base.isAuthority)
            {
                if (isCrit)
                {
                    RaycastHit hit;
                    // Does the ray intersect any objects excluding the player layer
                    if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, LayerIndex.world.mask))
                    {
                        if (hit.point != null)
                        {
                            SphereSearch sphereSearch = new SphereSearch();
                            sphereSearch.radius = 20;
                            sphereSearch.origin = hit.point;
                            sphereSearch.mask = LayerIndex.entityPrecise.mask;
                            sphereSearch.RefreshCandidates();
                            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();

                            HurtBox[] hurtBoxes = sphereSearch.GetHurtBoxes();
                            int maxExtraBullets = 5;
                            int numHurtboxes = Math.Min(maxExtraBullets, hurtBoxes.Length);

                            for (int i = 0; i < numHurtboxes; i++)
                            {
                                var enemyHurtbox = hurtBoxes[i];

                                if (enemyHurtbox != null && base.isAuthority)
                                {
                                    Vector3 enemyPosition = enemyHurtbox.transform.position;
                                    Vector3 critBulletDirection = enemyPosition - hit.point;

                                    var critBullet = new BulletAttack
                                    {
                                        owner = base.gameObject,
                                        weapon = base.gameObject,
                                        origin = hit.point,
                                        aimVector = critBulletDirection,
                                        minSpread = 0f,
                                        maxSpread = 0f,
                                        radius = 5,
                                        bulletCount = 1U,
                                        procCoefficient = procCoeff,
                                        damage = base.characterBody.damage * damageCoeff,
                                        force = 3,
                                        falloffModel = BulletAttack.FalloffModel.None,
                                        tracerEffectPrefab = this.tracerEffectPrefab,
                                        hitEffectPrefab = this.hitEffectPrefab,
                                        isCrit = isCrit,
                                        HitEffectNormal = false,
                                        smartCollision = true,
                                        maxDistance = 300f
                                    };

                                    critBullet.Fire();
                                }
                            }
                        }
                    }
                }

                var bullet = new BulletAttack
                {
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    maxSpread = base.characterBody.spreadBloomAngle,
                    bulletCount = 1U,
                    procCoefficient = procCoeff,
                    damage = base.characterBody.damage * damageCoeff,
                    force = 3,
                    radius = 5,
                    falloffModel = BulletAttack.FalloffModel.None,
                    tracerEffectPrefab = this.tracerEffectPrefab,
                    muzzleName = "MuzzleRight",
                    hitEffectPrefab = this.hitEffectPrefab,
                    isCrit = isCrit,
                    HitEffectNormal = false,
                    smartCollision = true,
                    maxDistance = 300f
                };

                bullet.Fire();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
