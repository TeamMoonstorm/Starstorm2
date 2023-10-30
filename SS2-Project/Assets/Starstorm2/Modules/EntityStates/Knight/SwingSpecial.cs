using Moonstorm;
using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.DamageTypes;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Knight
{
    class SwingSpecial : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        [TokenModifier("SS2_KNIGHT_SPECIAL_SPIN_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float TokenModifier_dmgCoefficient => new SwingSpecial().damageCoefficient;
        public static GameObject buffWard;
        public static float hopVelocity;
        private bool hasBuffed;
        private bool hasSpun;
        private GameObject wardInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            hasBuffed = false;
            hasSpun = false;

            animator = GetModelAnimator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (animator.GetFloat("BuffActive") >= 0.5f && !hasBuffed)
            {
                hasBuffed = true;
                wardInstance = Object.Instantiate(buffWard);
                wardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                wardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
                Util.PlaySound("CyborgUtility", gameObject);
            }

            if (animator.GetFloat("SpecialSwing") >= 0.5f && !hasSpun)
            {
                hasSpun = true;
                if (!isGrounded)
                {
                    SmallHop(characterMotor, hopVelocity);
                }
            }
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("Body", "SwingSpecial", "Special.playbackRate", duration * swingTimeCoefficient, 0.15f);   
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}