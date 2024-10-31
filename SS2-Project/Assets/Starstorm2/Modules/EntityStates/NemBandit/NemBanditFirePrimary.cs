using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using SS2;

namespace EntityStates.NemBandit
{
    public class NemBanditFirePrimary : GenericBulletBaseState
    {	
		public override void OnEnter()
		{
			minimumDuration = tempMinimumBaseDuration / attackSpeedStat;
			base.OnEnter();
		}

		public override void ModifyBullet(BulletAttack bulletAttack)
		{
			base.ModifyBullet(bulletAttack);
			bulletAttack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			if (fixedAge <= minimumDuration && inputBank.skill1.wasDown)
			{
				return InterruptPriority.PrioritySkill;
			}
			return InterruptPriority.Any;
		}

		[SerializeField]
		public float minimumBaseDuration;
		protected float minimumDuration;
		[SerializeField]
		public float additiveMashBloomValue;

		public static float tempMinimumBaseDuration = 0.3f;
	}
}
