using SS2;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using MSU;
using UnityEngine.Networking;
using System;
using MSU.Config;
using SS2.Survivors;

namespace EntityStates.Knight
{
    public class ShieldPunch : BaseKnightDashMelee
    {
        public static float TokenModifier_dmgCoefficient => new ShieldPunch().damageCoefficient;
        public int swingSide;
        [SerializeField]
        public BuffDef shieldBuff;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float swingTimeCoefficient = 2.3f;
        // Movement variables
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testDamage = 2.4f;

        public override void OnEnter()
        {
            damageCoefficient = testDamage;
            base.OnEnter();
            this.attack.damageType.damageSource = DamageSource.Primary | DamageSource.Secondary;
            characterBody.AddTimedBuff(shieldBuff, duration + 0.5f);
        }

        public override void PlayAttackAnimation()
        {
            PlayCrossfade("FullBody, Override", "ShieldPunch", "Primary.playbackRate", duration * swingTimeCoefficient, 0.15f);
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;

            base.OnExit();
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
            outer.SetNextState(new ShieldPunchWindDown());
        }
    }
}