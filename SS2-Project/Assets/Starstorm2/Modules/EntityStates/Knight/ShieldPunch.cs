using SS2;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using MSU;
using UnityEngine.Networking;
using System;
using MSU.Config;
using SS2.Survivors;
using R2API;

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

        private static float testDamage = 4f;

        private static float bounceAwayVelocity = -3f;
        private static float bounceUpVelocity = 2f;

        private static float knockbackForwardVelocity = 19f;
        private static float knockbackUpVelocity = 1.5f;
        private bool hasBounced;

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
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
        public override void MoveKnight()
        {
            if (!hasBounced)
            {
                base.MoveKnight();
            }
        }
        protected override void OnInterrupted()
        {

        }
        protected override void SetNextState()
        {
            outer.SetNextState(new ShieldPunchWindDown());
        }
        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;

            characterBody.isSprinting = true;

            base.OnExit();
        }

        protected override void AuthorityModifyOverlapAttack(OverlapAttack attack)
        {
            base.AuthorityModifyOverlapAttack(attack);

            attack.damageType.AddModdedDamageType(SS2.Survivors.Knight.ExtendedStunDamageType);
            attack.forceVector = forwardDirection * knockbackForwardVelocity + Vector3.up * knockbackUpVelocity;
            attack.physForceFlags = PhysForceFlags.resetVelocity | PhysForceFlags.massIsOne | PhysForceFlags.ignoreGroundStick | PhysForceFlags.disableAirControlUntilCollision;
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();

            if (!hasBounced)
            {
                hasBounced = true;

                Vector3 hAim = new Vector3(inputBank.aimDirection.x, 0, inputBank.aimDirection.z);
                hAim = hAim.normalized;
                Vector3 velocity = hAim * bounceAwayVelocity + Vector3.up * bounceUpVelocity;
                characterMotor.velocity = velocity; // ?
                storedVelocity = velocity;
                
            }

            outer.SetNextState(new ShieldPunchWindDown());
        }
    }
}