using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.Chirr.Weapon
{
    class FireTriScatter : BaseSkillState
    {
        float duration
        {
            get
            {
				return baseTargetDuration / base.attackSpeedStat;
            }
        }
		float delayBetweenScatterShots
		{
			get
			{
				return duration / baseTargetAmountOfShots;
			}
		}

		[Tooltip("How much the duration should last.")]
		[SerializeField]
		public float baseTargetDuration;
		[Tooltip("Amount of shots to do before finishing.")]
		[SerializeField]
		public float baseTargetAmountOfShots;
		[Tooltip("Amount of projectiles to spawn per shot.")]
		[SerializeField]
		public float baseAmountOfScatterPerShot;
		[SerializeField]
		public float baseProjectileDamageCoefficient;
		[SerializeField]
		public GameObject projectilePrefab;
		[SerializeField]
		public string[][] projectileOrigins;
		[SerializeField]
		public SerializableEntityStateType comboFinalizerState;


        private float countdownSinceLastShot;
        private int shotCount;

        public override void OnEnter()
		{
			base.OnEnter();
			base.StartAimMode(duration, false);
		}
		public override void OnExit()
		{
			base.OnExit();
		}
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			countdownSinceLastShot -= Time.fixedDeltaTime;
			if (shotCount < baseTargetAmountOfShots && this.countdownSinceLastShot <= 0f)
			{
				this.shotCount++;
				this.countdownSinceLastShot += this.delayBetweenScatterShots;
                for (int i = 0; i < baseAmountOfScatterPerShot; i++)
                {
					this.FireScatterShot(shotCount, i);
                }
			}
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

        private void FireScatterShot(int currentShot, int currentScatterInShot)
        {
            throw new NotImplementedException();
        }
    }
}
