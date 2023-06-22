using System;
using System.Collections.Generic;
using System.Linq;
using RoR2;
using EntityStates;
using UnityEngine;
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
            //play anim
        }

        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            base.ModifyBullet(bulletAttack);
            bulletAttack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
