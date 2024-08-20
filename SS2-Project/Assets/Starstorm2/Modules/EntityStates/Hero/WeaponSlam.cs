using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Hero
{
    public class WeaponSlam : BaseState
    {
        public static float duration = 3.5f;
        public static float damageCoefficient = 4f;
        public static float forceMagnitude = 16f;
        public static float upwardForce;
        public static float weaponForce;
        public static float radius = 3f;
        public static string attackSoundString;
        public static string muzzleString;
        public static GameObject slamImpactEffect;
        public static float durationBeforePriorityReduces;
        //public static GameObject blastEffect;
        //public static GameObject weaponHitEffectPrefab;
        private BlastAttack blastAttack;
        private OverlapAttack weaponAttack;
        private Animator modelAnimator;
        private Transform modelTransform;
        private bool hasDoneBlastAttack;
        
        public override void OnEnter()
        {
            base.OnEnter();
            modelAnimator = GetModelAnimator();
            modelTransform = GetModelTransform();
            Util.PlayAttackSpeedSound(attackSoundString, gameObject, attackSpeedStat);
            PlayCrossfade("FullBody Override", "WeaponSlam", "WeaponSlam.playbackRate", duration, 0.1f);
            if (characterDirection)
            {
                characterDirection.moveVector = GetAimRay().direction;
            }
            if (modelTransform)
            {
                AimAnimator aa = modelTransform.GetComponent<AimAnimator>();
                if (aa)
                {
                    aa.enabled = true;
                }
            }
            if (isAuthority)
            {
                OverlapAttack overlapAttack = new OverlapAttack();
                overlapAttack.attacker = gameObject;
                overlapAttack.damage = damageCoefficient * damageStat;
                overlapAttack.damageColorIndex = DamageColorIndex.Default;
                //overlapAttack.hitEffectPrefab = ;
                overlapAttack.hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup hbg) => hbg.groupName == "WeaponBig");
                //overlapAttack.impactSound = ;
                overlapAttack.inflictor = gameObject;
                overlapAttack.procChainMask = default(ProcChainMask);
                overlapAttack.pushAwayForce = weaponForce;
                overlapAttack.procCoefficient = 1f;
                overlapAttack.teamIndex = GetTeam();
                weaponAttack = overlapAttack;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (modelAnimator)
            {
                if (isAuthority && modelAnimator.GetFloat("weaponAttack.hitBoxActive") > 0.5f)
                {
                    weaponAttack.Fire(null);
                }
                if (modelAnimator.GetFloat("blastAttack.hitBoxActive") > 0.5f && !hasDoneBlastAttack)
                {
                    hasDoneBlastAttack = true;
                    //EffectManager.SimpleMuzzleFlash(slamImpactEffect, gameObject, muzzleString, false);
                    if (isAuthority)
                    {
                        if (characterDirection)
                        {
                            characterDirection.moveVector = characterDirection.forward;
                        }

                        if (modelTransform)
                        {
                            Transform transform = FindModelChild(muzzleString);
                            if (transform)
                            {
                                blastAttack = new BlastAttack();
                                blastAttack.attacker = gameObject;
                                blastAttack.inflictor = gameObject;
                                blastAttack.teamIndex = TeamComponent.GetObjectTeam(gameObject);
                                blastAttack.baseDamage = damageStat * damageCoefficient;
                                blastAttack.baseForce = forceMagnitude;
                                blastAttack.position = transform.position;
                                blastAttack.radius = radius;
                                blastAttack.bonusForce = new Vector3(0f, upwardForce, 0f);
                                blastAttack.Fire();
                            }
                        }
                    }
                }
            }
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (fixedAge <= durationBeforePriorityReduces)
            {
                return InterruptPriority.PrioritySkill;
            }
            return InterruptPriority.Skill;
        }
    }
}
