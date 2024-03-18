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
    class SwingSword : BasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        public static float swingTimeCoefficient = 1.42f;
        [TokenModifier("SS2_KNIGHT_PRIMARY_SWORD_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static GameObject beamProjectile;
        public static SkillDef buffedSkillRef;
        public static float TokenModifier_dmgCoefficient => new SwingSword().damageCoefficient;
        public int swingSide;

        public override void OnEnter()
        {
            base.OnEnter();

            animator = GetModelAnimator();
        }

        public override void PlayAnimation()
        {
           string animationStateName = (swingSide == 0) ? "SwingSword1" : "SwingSword2";
           PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.05f);             
        }

        void SteppedSkillDef.IStepSetter.SetStep(int i)
        {
            swingSide = i;
            swingEffectMuzzleString = (swingSide == 0) ? "SwingLeft" : "SwingRight";
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)swingSide);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            swingSide = (int)reader.ReadByte();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}