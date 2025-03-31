﻿using RoR2;
using UnityEngine;
using R2API;

namespace EntityStates.NemMerc
{
    public class FireShotgun : GenericBulletBaseState
    {
        public static float selfKnockbackForce = 750f;
        public override void OnEnter()
        {
            base.OnEnter();

            if(!base.isGrounded && base.isAuthority && base.characterMotor)
            {
                Vector3 direction = base.GetAimRay().direction * -1f;
                base.characterMotor.ApplyForce(direction * FireShotgun.selfKnockbackForce, true);
            }
            

        }

        public override void DoFireEffects()
        {
            base.DoFireEffects();
            //vfx
            //sound
        }
        public override void PlayFireAnimation()
        {
            base.PlayFireAnimation();
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            base.PlayAnimation("Gesture, Override", "FireShotgun");
        }

        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            base.ModifyBullet(bulletAttack);
            bulletAttack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
            bulletAttack.damageType.damageSource = DamageSource.Primary;
            bulletAttack.AddModdedDamageType(SS2.Survivors.NemMerc.damageType);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
