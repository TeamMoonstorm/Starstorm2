using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
namespace EntityStates.FlowerTurret
{
	public class AimFlowerTurret : BaseFlowerTurretState
	{
		public static float minDuration = 0f;
		public static float maxEnemyDistanceToStartFiring = 25f;
		public static float searchInterval = 0.25f;
		private BullseyeSearch enemyFinder;
		private float searchRefreshTimer;
		public override void OnEnter()
		{
			base.OnEnter();
			if (base.isAuthority)
			{
				this.enemyFinder = new BullseyeSearch();
				this.enemyFinder.teamMaskFilter = TeamMask.GetEnemyTeams(this.body.teamComponent.teamIndex);
				this.enemyFinder.maxDistanceFilter = maxEnemyDistanceToStartFiring;
				this.enemyFinder.maxAngleFilter = float.MaxValue;
				this.enemyFinder.filterByLoS = true;
				this.enemyFinder.sortMode = BullseyeSearch.SortMode.Distance;
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && base.fixedAge > minDuration)
			{
				this.searchRefreshTimer -= Time.fixedDeltaTime;
				if (this.searchRefreshTimer < 0f)
				{
					this.searchRefreshTimer = searchInterval;
					this.enemyFinder.searchOrigin = this.muzzleTransform.position;
					this.enemyFinder.searchDirection = this.body.transform.forward;
					this.enemyFinder.RefreshCandidates();
					using (IEnumerator<HurtBox> enumerator = this.enemyFinder.GetResults().GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							HurtBox targetHurtBox = enumerator.Current;
							this.outer.SetNextState(new FireFlowerTurret { targetHurtBox = targetHurtBox });
						}
					}
				}
			}
		}

		
	}
}
