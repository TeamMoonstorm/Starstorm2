using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.NemCaptain
{
    public class Punch : BasicMeleeAttack
    {
        private bool hasPunched = false;
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnMeleeHitAuthority()
        {
            base.OnMeleeHitAuthority();
            ShakeEmitter se = ShakeEmitter.CreateSimpleShakeEmitter(transform.position, new Wave() { amplitude = 3f, cycleOffset = 0f, frequency = 4f }, 0.25f, 20f, true);
            se.transform.parent = transform;
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType = DamageType.Stun1s;
        }
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (fixedAge >= duration * 0.3f && !hasPunched)
			{
				BeginMeleeAttackEffect();
                hasPunched = true;
			}
			if (isAuthority)
			{
				AuthorityFixedUpdate();
			}
		}
	}
}
