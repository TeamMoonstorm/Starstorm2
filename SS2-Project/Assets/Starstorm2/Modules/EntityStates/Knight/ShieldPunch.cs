using SS2;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using MSU;
using UnityEngine.Networking;
using System;
using MSU.Config;

namespace EntityStates.Knight
{
    public class ShieldPunch : BaseKnightDashMelee
    {
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float swingTimeCoefficient = 1.63f;

        // Movement variables
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testbaseDuration = 0.69f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testminSpeedCoefficient = 2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testmaxSpeedCoefficient = 4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testinterruptSpeedCoefficient = 0.0f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testearlyexit = 0.1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testfuckingattackendwhyisitnotgettingitfromtheESC = 1f;

        public override void OnEnter()
        {
            baseDuration = testbaseDuration;
            minSpeedCoefficient = testminSpeedCoefficient;
            maxSpeedCoefficient = testmaxSpeedCoefficient;
            interruptSpeedCoefficient = testinterruptSpeedCoefficient;
            attackEndTimeFraction = testfuckingattackendwhyisitnotgettingitfromtheESC;

            earlyExitPercentTime = testearlyexit;

            base.OnEnter();
        }

        public static float TokenModifier_dmgCoefficient => new ShieldPunch().damageCoefficient;
        public int swingSide;

        public override void PlayAttackAnimation()
        {
            PlayCrossfade("FullBody, Override", "ShieldPunch", "Primary.playbackRate", duration * swingTimeCoefficient, 0.15f);
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            skillLocator.primary.UnsetSkillOverride(skillLocator.primary, Shield.shieldBashSkillDef, GenericSkill.SkillOverridePriority.Contextual);

            base.OnExit();
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
            outer.SetNextState(new ShieldPunchWindDown());
        }
    }
}