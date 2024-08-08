﻿using MSU;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.Knight
{
    class SpinUtility : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        [FormatToken("SS2_KNIGHT_SPECIAL_SPIN_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float TokenModifier_dmgCoefficient => new SpinUtility().damageCoefficient;
        public static SkillDef buffedSkillRef;
        
        private bool hasSpun;
        private int _origLayer;

        public float hopVelocity = 35;
        public float airControl = 0.25f;
        public float upwardVelocity = 4f;
        public float forwardVelocity = 8f;
        public float minimumY = 0.3f;
        public float aimVelocity = 1f;


        public override void OnEnter()
        {
            base.OnEnter();

            if (characterMotor)
            {
                _origLayer = characterMotor.capsuleCollider.gameObject.layer;
                characterMotor.capsuleCollider.gameObject.layer = LayerIndex.fakeActor.intVal;
                characterMotor.Motor.RebuildCollidableLayers();
            }

            hasSpun = false;

            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            animator = GetModelAnimator();

            Vector3 direction = GetAimRay().direction;

            // Launch Knight where they are aiming
            if (isAuthority)
            {
                characterBody.isSprinting = true;
                direction.y = Mathf.Max(direction.y, minimumY);
                Vector3 a = direction.normalized * aimVelocity * moveSpeedStat;
                Vector3 b = Vector3.up * upwardVelocity;
                Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * forwardVelocity;
                characterMotor.Motor.ForceUnground();
                characterMotor.velocity = a + b + b2;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (animator.GetFloat("Utility") >= 0.5f && !hasSpun)
            {
                hasSpun = true;
                if (!isGrounded)
                {
                    SmallHop(characterMotor, hopVelocity);
                }
            }
        }


        public override void OnExit()
        {
            if (characterMotor) //Nasty fucking hack
            {
                characterMotor.capsuleCollider.gameObject.layer = _origLayer;
                characterMotor.Motor.RebuildCollidableLayers();
            }

            characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            base.OnExit();
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("FullBody, Override", "Utility", "Utility.playbackRate", duration * swingTimeCoefficient, 0.15f);   
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}