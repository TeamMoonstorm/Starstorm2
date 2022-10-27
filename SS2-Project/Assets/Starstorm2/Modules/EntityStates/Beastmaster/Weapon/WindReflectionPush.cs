using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Beastmaster.Weapon
{
    class WindReflectionPush : GenericProjectileBaseState
	{
		[SerializeField]
		public string enterSoundString;
		[SerializeField]
		public float reflectDamageMultiplier;
		[Tooltip("Modify the origin of the aimray used for the projectile reflection.")]
		[SerializeField]
		public float backupDistance;
		[SerializeField]
		public int maxReflectDistance;
		[SerializeField]
		public GameObject reflectTracerEffectPrefab;

        private Ray aimRay;

        public override void OnEnter()
		{
			base.OnEnter();
			aimRay = base.GetAimRay();
			Util.PlaySound(this.enterSoundString, base.gameObject);
			Vector3 origin = this.aimRay.origin;
			this.aimRay.origin = this.aimRay.origin - this.aimRay.direction * this.backupDistance;
			if (NetworkServer.active)
			{
				this.ReflectProjectiles();
			}
		}
		public override void PlayAnimation(float duration)
		{
		}
		public override Ray ModifyProjectileAimRay(Ray aimRay)
		{
			//Flatten
			aimRay.direction.Scale(new Vector3(1, 0, 1));
			return aimRay;
		}
		private void ReflectProjectiles()
		{
			Vector3 vector = base.characterBody ? base.characterBody.corePosition : Vector3.zero;
			TeamIndex teamIndex = base.characterBody ? base.characterBody.teamComponent.teamIndex : TeamIndex.None;
			float squareMaxDistance = this.maxReflectDistance * this.maxReflectDistance;
			//Get all projectiles... are we sure this wont be laggy?
			List<ProjectileController> allDamnProjectileControllers = InstanceTracker.GetInstancesList<ProjectileController>();
			List<ProjectileController> projectilesWithinRange = new List<ProjectileController>();

			//Index the counts, for performance.
			//Apparently a for loop is faster than foreach if you need to access the array
			int i = 0;
			int count = allDamnProjectileControllers.Count;
			while (i < count)
			{
				ProjectileController projectileController = allDamnProjectileControllers[i];
				if (projectileController.teamFilter.teamIndex != teamIndex && (projectileController.transform.position - vector).sqrMagnitude < squareMaxDistance)
				{
					projectilesWithinRange.Add(projectileController);
				}
				i++;
			}

			//Do the same
			int j = 0;
			int count2 = projectilesWithinRange.Count;
			while (j < count2)
			{
				ProjectileController projectileController2 = projectilesWithinRange[j];
				if (projectileController2)
				{
					Vector3 position = projectileController2.transform.position;
					Vector3 start = vector;
					if (reflectTracerEffectPrefab)
					{
						EffectData effectData = new EffectData
						{
							origin = position,
							start = start
						};
						EffectManager.SpawnEffect(this.reflectTracerEffectPrefab, effectData, true);
					}
					CharacterBody component = projectileController2.owner.GetComponent<CharacterBody>();
					projectileController2.IgnoreCollisionsWithOwner(false);
					projectileController2.Networkowner = base.gameObject;
					projectileController2.teamFilter.teamIndex = base.characterBody.teamComponent.teamIndex;
					ProjectileDamage component2 = projectileController2.GetComponent<ProjectileDamage>();
					if (component2)
					{
						component2.damage *= reflectDamageMultiplier;
					}
					Rigidbody component3 = projectileController2.GetComponent<Rigidbody>();
					if (component3)
					{
						Vector3 vector2 = component3.velocity * -1f;
						if (component)
						{
							vector2 = component.corePosition - component3.transform.position;
						}
						component3.transform.forward = vector2;
						component3.velocity = Vector3.RotateTowards(component3.velocity, vector2, float.PositiveInfinity, 0f);
					}
				}
				j++;
			}
		}

	}
}
