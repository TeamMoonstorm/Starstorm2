using RoR2;
using RoR2.Projectile;
using SS2;
using UnityEngine;

namespace EntityStates.Mimic.Weapon
{
    public class MimicMinigunEnter : MimicMinigunState
    {
        public static float baseDuration;
        public static GameObject spinVFXPrefab;
       
        public static string mecanimPeramater;

        private float duration;
        private bool hasFired;
        //private Transform muzzle;
        private Animator animator;

		private GameObject spinInstanceLeft;
		private GameObject spinInstanceRight;

		private float originalMoveSpeed;

		private bool endedSuccessfully = false;

		public override void OnEnter()
		{
			base.OnEnter();
			duration = baseDuration / this.attackSpeedStat;

			//Util.PlaySound(MinigunSpinUp.sound, base.gameObject);
			//base.GetModelAnimator().SetBool(MinigunSpinUp.WeaponIsReadyParamHash, true);
			SS2Log.Warning("muzzleTransformLeft: " + muzzleTransformLeft);
			if (muzzleTransformLeft && spinVFXPrefab)
			{
				fireVFXInstanceLeft = UnityEngine.Object.Instantiate<GameObject>(spinVFXPrefab, muzzleTransformLeft.position, muzzleTransformLeft.rotation);
				fireVFXInstanceLeft.transform.parent = muzzleTransformLeft;
				//ScaleParticleSystemDuration component = this.chargeInstance.GetComponent<ScaleParticleSystemDuration>();
				//if (component)
				//{
				//	component.newDuration = this.duration;
				//}
			}

			if (muzzleTransformLeft && spinVFXPrefab)
			{
				fireVFXInstanceRight = UnityEngine.Object.Instantiate<GameObject>(spinVFXPrefab, muzzleTransformRight.position, muzzleTransformRight.rotation);
				fireVFXInstanceRight.transform.parent = muzzleTransformRight;
				//ScaleParticleSystemDuration component = this.chargeInstance.GetComponent<ScaleParticleSystemDuration>();
				//if (component)
				//{
				//	component.newDuration = this.duration;
				//}
			}

			PlayCrossfade("Gesture, Override", "MinigunEnter", "MinigunFire.playbackRate", duration, 0.05f);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (fixedAge >= duration && isAuthority)
			{
				endedSuccessfully = true;
				var loop = new MimicMinigunLoop();
				loop.fireVFXInstanceLeft = fireVFXInstanceLeft;
				loop.fireVFXInstanceRight = fireVFXInstanceRight;
				outer.SetNextState(loop);
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			if (!endedSuccessfully)
			{
				PlayAnimation("Gesture, Override", "BufferEmpty");
				if (fireVFXInstanceLeft)
				{
					EntityState.Destroy(fireVFXInstanceLeft);
				}

				if (fireVFXInstanceRight)
				{
					EntityState.Destroy(fireVFXInstanceRight);
				}
			}

		}
    }
}
