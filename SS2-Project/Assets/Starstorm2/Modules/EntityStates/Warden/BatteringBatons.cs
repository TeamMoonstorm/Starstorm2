﻿using MSU;
using RoR2;
using UnityEngine;

namespace EntityStates.Warden
{
    public class BatteringBatons : BaseWardenMeleeAttack
    {
        public static float swingTimeCoefficient;
        [FormatToken("SS2_KNIGHT_PRIMARY_SWORD_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static GameObject beamProjectile;

        public static float baseDurationBeforeInterruptable;
        public static float comboFinisherBaseDurationBeforeInterruptable;
        public static float comboFinisherhitPauseDuration;
        public static float comboFinisherDamageCoefficient;

        public new float baseDuration = 1f;
        public new float duration = 1f;

        private bool isComboFinisher => swingIndex == 2;
        private string animationStateName = "SwingSword0";


        private void SetupHitbox()
        {
            switch (swingIndex)
            {
                case 0:
                    animationStateName = "SwingSword0";
                    muzzleString = "SwingRight";
                    hitboxGroupName = "SwordHitbox";
                    break;
                case 1:
                    animationStateName = "SwingSword1";
                    muzzleString = "SwingLeft";
                    hitboxGroupName = "SwordHitbox";
                    break;
                default:
                    animationStateName = "SwingSword0";
                    muzzleString = "SwingLeft";
                    hitboxGroupName = "SwordHitbox";
                    break;
            }


            damageType = DamageType.Generic;
            procCoefficient = 0.7f;
            bonusForce = Vector3.zero;
            baseDuration = 1f;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0.25f;
            attackEndPercentTime = 0.7f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.8f;

            swingSoundString = "NemmandoSwing";
            hitSoundString = "";
            playbackRateParam = "Primary.Hitbox";
            hitEffectPrefab = SS2.Survivors.Knight.KnightHitEffect;
        }

        public override void OnEnter()
        {
            if (base.isAuthority)
            {
                SetupHitbox();
                base.OnEnter();
            }
        }

        public override void PlayAttackAnimation()
        {
            //if (base.isGrounded & !base.GetModelAnimator().GetBool("isMoving"))
            //{
            //    PlayCrossfade("FullBody, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.08f);
            //}
            //else
            //{
            //    PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.08f);
            //}
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
