using SS2;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using MSU;
using UnityEngine.Networking;

namespace EntityStates.Knight
{
    class ShieldPunch : BasicMeleeAttack
    {
        public float initialSpeedCoefficient = 6f;
        public float finalSpeedCoefficient = 2.5f;

        //public static string dodgeSoundString = "HenryRoll";

        private float rollSpeed;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;

        public static float swingTimeCoefficient = 1f;
        public static float TokenModifier_dmgCoefficient => new ShieldPunch().damageCoefficient;
        public int swingSide;

        public float hopVelocity = 14f;
        public float airControl = 0.30f;
        public float upwardVelocity = 0.6f;
        public float forwardVelocity = 18f;
        public float minimumY = 0.05f;
        public float aimVelocity = 1f;

        private float shieldPunchCooldownDuration = 1.5f;

        private bool hasShieldPunch = false;

        public override void OnEnter()
        {
            base.OnEnter();

            if (!characterBody.HasBuff(SS2Content.Buffs.bdKnightShieldCooldown) && !hasShieldPunch && isAuthority && inputBank && characterDirection)
            {

                forwardDirection = (inputBank.moveVector == Vector3.zero ? characterDirection.forward : inputBank.moveVector).normalized;
                
                RecalculateRollSpeed();

                if (characterMotor && characterDirection)
                {
                    characterMotor.velocity.y = 0f;
                    characterMotor.velocity = forwardDirection * rollSpeed;
                }

                Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
                previousPosition = transform.position - b;

                if (!isGrounded)
                {
                    SmallHop(characterMotor, hopVelocity);
                }
            }
        }

        private void RecalculateRollSpeed()
        {
            rollSpeed = moveSpeedStat * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && !characterBody.HasBuff(SS2Content.Buffs.bdKnightShieldCooldown) && !hasShieldPunch)
            {
                RecalculateRollSpeed();

                if (characterDirection) characterDirection.forward = forwardDirection;
                if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(SS2.Survivors.Knight.dodgeFOV, 60f, fixedAge / duration);

                Vector3 normalized = (transform.position - previousPosition).normalized;
                if (characterMotor && characterDirection && normalized != Vector3.zero)
                {
                    Vector3 vector = normalized * rollSpeed;
                    float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                    vector = forwardDirection * d;
                    vector.y = 0f;

                    characterMotor.velocity = vector;
                }
                previousPosition = transform.position;
            }
            

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("Gesture, Override", "ShieldPunch", "Primary.playbackRate", duration * swingTimeCoefficient, 0.15f);
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            skillLocator.primary.UnsetSkillOverride(skillLocator.primary, Shield.shieldBashSkillDef, GenericSkill.SkillOverridePriority.Contextual);
            
            if (!hasShieldPunch)
            {
                characterBody.AddTimedBuff(SS2Content.Buffs.bdKnightShieldCooldown, shieldPunchCooldownDuration);
                hasShieldPunch = true;
            }
            
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType = DamageType.Stun1s;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(forwardDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            forwardDirection = reader.ReadVector3();
        }
    }
}