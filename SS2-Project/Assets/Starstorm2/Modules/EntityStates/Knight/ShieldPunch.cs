using Moonstorm;
using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.DamageTypes;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Knight
{
    class ShieldPunch : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        [TokenModifier("SS2_KNIGHT_SHIELD_BASH_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static GameObject beamProjectile;
        public static GameObject bigBeamProjectile;
        private GameObject beam;
        public static float TokenModifier_dmgCoefficient => new ShieldPunch().damageCoefficient;
        public int swingSide;
        private bool hasFiredBeam = false;

        public override void OnEnter()
        {
            base.OnEnter();

            skillLocator.primary.UnsetSkillOverride(skillLocator.primary, Shield.skillDef, GenericSkill.SkillOverridePriority.Contextual);

            animator = GetModelAnimator();
        }

        public override void FixedUpdate()
        {
            if (animator.GetFloat(mecanimHitboxActiveParameter) > 0.5f && fixedAge >= duration * 0.1f)
            {
                if (isAuthority && !hasFiredBeam)
                {
                    hasFiredBeam = true;

                    float damage = damageStat * damageCoefficient * 2f;
                    beam = beamProjectile;
                    if (characterBody.HasBuff(SS2Content.Buffs.bdKnightCharged))
                    {
                        damage *= 1.5f;
                        beam = bigBeamProjectile;
                    }

                    ProjectileManager.instance.FireProjectile(
                    beam,
                    GetAimRay().origin,
                    Util.QuaternionSafeLookRotation(GetAimRay().direction),
                    gameObject,
                    damage,
                    0f,
                    RollCrit(),
                    DamageColorIndex.Default,
                    null,
                    80f);
                }
            }
            base.FixedUpdate();
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("Gesture, Override", "ShieldPunch", "Primary.playbackRate", duration * swingTimeCoefficient, 0.15f);
        }

        public override void OnExit()
        {
            if (characterBody.HasBuff(SS2Content.Buffs.bdKnightCharged))
                characterBody.SetBuffCount(SS2Content.Buffs.bdKnightCharged.buffIndex, 0);
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
            if (characterBody.HasBuff(SS2Content.Buffs.bdKnightCharged))
                overlapAttack.damage *= 2f;
        }
    }
}