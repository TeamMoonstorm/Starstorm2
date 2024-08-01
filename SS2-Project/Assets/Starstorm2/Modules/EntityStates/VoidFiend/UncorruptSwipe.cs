using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.VoidFiend
{
    public class UncorruptSwipe : BasicMeleeAttack
    {
        public float baseDuration = 2f;
        private float duration;

		//protected override bool allowExitFire
		//{
		//	get
		//	{
		//		return base.characterBody && !base.characterBody.isSprinting;
		//	}
		//}

		public override void OnEnter()
		{
			duration = baseDuration;
			base.OnEnter();
			swingEffectPrefab = null;
			base.characterDirection.forward = base.GetAimRay().direction;
		}

		// Token: 0x060004C8 RID: 1224 RVA: 0x00014784 File Offset: 0x00012984
		public override void OnExit()
		{
			base.OnExit();
		}

		// Token: 0x060004C9 RID: 1225 RVA: 0x0001478C File Offset: 0x0001298C
		public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
		{
			base.AuthorityModifyOverlapAttack(overlapAttack);
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x00014795 File Offset: 0x00012995
		public override void PlayAnimation()
		{ //ther are 3 (SwingMelee1, SwingMelee2,SwingMelee3)
			base.PlayAnimation(this.animationLayerName, this.animationStateName, this.animationPlaybackRateParameter, this.duration);
		}

		// Token: 0x060004CB RID: 1227 RVA: 0x000147B5 File Offset: 0x000129B5
		public override void OnMeleeHitAuthority()
		{
			base.OnMeleeHitAuthority();
			base.characterBody.AddSpreadBloom(this.bloom);
		}

		// Token: 0x060004CC RID: 1228 RVA: 0x000147CE File Offset: 0x000129CE
		public override void BeginMeleeAttackEffect()
		{
			base.AddRecoil(-0.1f * this.recoilAmplitude, 0.1f * this.recoilAmplitude, -1f * this.recoilAmplitude, 1f * this.recoilAmplitude);
			base.BeginMeleeAttackEffect();
		}

		// Token: 0x060004CD RID: 1229 RVA: 0x0000B89B File Offset: 0x00009A9B
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		[SerializeField]
		public float recoilAmplitude;

		// Token: 0x04000562 RID: 1378
		[SerializeField]
		public float bloom;

		// Token: 0x04000563 RID: 1379
		[SerializeField]
		public string animationLayerName;

		// Token: 0x04000564 RID: 1380
		[SerializeField]
		public string animationStateName;

		// Token: 0x04000565 RID: 1381
		[SerializeField]
		public string animationPlaybackRateParameter;

	}
}
