using RoR2;
using RoR2.Projectile;
using SS2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Mimic.Weapon
{
    public class MimicMinigunEnter : MimicMinigunState
    {
        public static float baseDuration;
		public static GameObject spinVFXPrefab;

        private float duration;

		private bool endedSuccessfully = false;

		public override void OnEnter()
		{
			base.OnEnter();
			duration = baseDuration / this.attackSpeedStat;

			if (muzzleTransformLeft && spinVFXPrefab)
			{
				fireVFXInstanceLeft = UnityEngine.Object.Instantiate<GameObject>(spinVFXPrefab, muzzleTransformLeft.position, muzzleTransformLeft.rotation);
				fireVFXInstanceLeft.transform.parent = muzzleTransformLeft;
			}

			if (muzzleTransformLeft && spinVFXPrefab)
			{
				fireVFXInstanceRight = UnityEngine.Object.Instantiate<GameObject>(spinVFXPrefab, muzzleTransformRight.position, muzzleTransformRight.rotation);
				fireVFXInstanceRight.transform.parent = muzzleTransformRight;
			}

			PlayCrossfade("Gesture, Override", "MinigunEnter", "MinigunFire.playbackRate", duration, 0.05f);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (fixedAge >= duration && isAuthority)
			{
				endedSuccessfully = true;
				var loop = new MimicMinigunLoop { fireVFXInstanceLeft = this.fireVFXInstanceLeft, fireVFXInstanceRight = this.fireVFXInstanceRight };
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
