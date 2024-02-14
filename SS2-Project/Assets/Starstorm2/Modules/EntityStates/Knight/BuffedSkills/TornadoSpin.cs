using EntityStates;
using EntityStates.Knight;
using Moonstorm;
using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.DamageTypes;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Starstorm2.Modules.EntityStates.Knight.BuffedSkills
{
    class TornadoSpin : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        [TokenModifier("SS2_KNIGHT_SPECIAL_SPIN_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float TokenModifier_dmgCoefficient => new SpinUtility().damageCoefficient;
        public static GameObject buffWard;
        public static float hopVelocity;
        public static float airControl;
        public static float upwardVelocity;
        public static float forwardVelocity;
        public static float minimumY;
        public static float aimVelocity;
        public static GameObject beamProjectile;
        public static SkillDef originalSkillRef;

        private bool hasBuffed;
        private bool hasSpun;
        private GameObject wardInstance;
        private bool hasFiredBeam = true;


        public override void OnEnter()
        {
            Debug.Log("DEBUGGER The tornado spin was entered!!");
            base.OnEnter();
            hasBuffed = false;
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


        public override void OnExit()
        {
            Debug.Log("DEBUGGER The passive was exited!!");

            ProjectileManager.instance.FireProjectile(
                beamProjectile,
                GetAimRay().origin,
                Util.QuaternionSafeLookRotation(GetAimRay().direction),
                gameObject,
                damageStat * damageCoefficient,
                0f,
                RollCrit(),
                DamageColorIndex.Default,
                null,
                80f);

            characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;

            GenericSkill originalUtilitySkill = skillLocator.utility;
            originalUtilitySkill.UnsetSkillOverride(gameObject, SpinUtility.buffedSkillRef, GenericSkill.SkillOverridePriority.Contextual);

            outer.SetNextStateToMain();
            base.OnExit();
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