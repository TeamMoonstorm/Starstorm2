using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using SS2;

namespace EntityStates.NemBandit
{
    public class NemBanditFireEmptyGun : BaseState
    {
		public override void OnEnter()
		{
			Util.PlaySound(fireSoundString, gameObject);
			minimumDuration = tempMinimumBaseDuration / attackSpeedStat;
			duration = baseDuration / attackSpeedStat;
			base.OnEnter();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (fixedAge >= duration)
			{
				outer.SetNextState(EntityStateCatalog.InstantiateState(ref outer.mainStateType));
			}
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
		public float baseDuration;
		protected float duration;
		[SerializeField]
		public float minimumBaseDuration;
		protected float minimumDuration;
		[SerializeField]
		public string fireSoundString;

		public static float tempMinimumBaseDuration = 0.3f;
	}
}
