using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using Moonstorm.Starstorm2;
using UnityEngine.AddressableAssets;

namespace EntityStates.MULE
{
    public class MULEJump : BaseSkillState
    {
        public float charge;

        public static float minJumpCoefficient;
        public static float maxJumpCoefficient;
        public static float baseDuration;

        private Animator animator;
        private float jumpCoefficient;
        private float duration;
        private ChildLocator childLocator;

        public override void OnEnter()
        {
            Debug.Log("Jumping " + charge);
            base.OnEnter();
            animator = GetModelAnimator();
            childLocator = GetModelChildLocator();
            duration = baseDuration / moveSpeedStat;
            characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSlow20.buffIndex, 0);
            jumpCoefficient = Util.Remap(charge, 0f, 1f, minJumpCoefficient, maxJumpCoefficient);

            characterMotor.rootMotion += Vector3.down * moveSpeedStat * 25f;

            SmallHop(characterMotor, jumpCoefficient);

            PlayCrossfade("FullBody, Override", "Jump", "Secondary.playbackRate", duration, 0.05f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
