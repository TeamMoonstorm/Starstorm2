using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using SS2.Survivors;
using UnityEngine.AddressableAssets;

namespace SS2.Orbs
{
	public class ExecutionerTaserOrb : Orb
	{
		public Vector3 targetPosition;
		public Vector3 attackerAimVector;
		public override void Begin()
		{
			base.duration = 0.1f;

			EffectData effectData = new EffectData
			{
				origin = this.origin,
				genericFloat = base.duration,
				start = this.targetPosition
			};
			effectData.SetHurtBoxReference(this.target);
			taserVFXMastery = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ChainLightning/ChainLightningOrbEffect.prefab").WaitForCompletion();
            //if (this.groundHit)
            //{
            //EffectManager.SpawnEffect(Executioner2.taserVFX, effectData, true);
            EffectManager.SpawnEffect(taserVFXMastery, effectData, true);
            //}
            //else
            //{
			//	EffectManager.SpawnEffect(Executioner2.taserVFXFade, effectData, true);
			//}
			
		}

		public override void OnArrival()
		{
			if (this.target)
			{
				HealthComponent healthComponent = this.target.healthComponent;
				if (healthComponent)
				{
					DamageInfo damageInfo = new DamageInfo();
					damageInfo.damage = this.damageValue;
					damageInfo.attacker = this.attacker;
					damageInfo.inflictor = this.inflictor;
					damageInfo.force = Vector3.zero;
					damageInfo.crit = this.isCrit;
					damageInfo.procChainMask = this.procChainMask;
					damageInfo.procCoefficient = this.procCoefficient;
					damageInfo.position = this.target.transform.position;
					damageInfo.damageColorIndex = this.damageColorIndex;
					damageInfo.damageType = this.damageType;
					healthComponent.TakeDamage(damageInfo);
					GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
					GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
				}
				this.failedToKill |= (!healthComponent || healthComponent.alive);
				if (this.bouncesRemaining > 0 || this.bouncesRemaining == -1) //for the repeat whiff
				{
					if (this.bouncedObjects != null)
					{
						this.bouncedObjects.Add(this.target.healthComponent);
					}
					HurtBox hurtBox = this.PickNextTarget(this.target.transform.position, false);
					if (hurtBox && this.bouncesRemaining != -1)
					{
						ExecutionerTaserOrb taserOrb = new ExecutionerTaserOrb();
						taserOrb.search = this.search;
						taserOrb.origin = this.target.transform.position;
						taserOrb.target = hurtBox;
						taserOrb.attacker = this.attacker;
						taserOrb.inflictor = this.inflictor;
						taserOrb.teamIndex = this.teamIndex;
						taserOrb.damageValue = this.damageValue * this.damageCoefficientPerBounce;
						taserOrb.bouncesRemaining = this.bouncesRemaining - 1;
						taserOrb.isCrit = this.isCrit;
						taserOrb.bouncedObjects = this.bouncedObjects;
						taserOrb.procChainMask = this.procChainMask;
						taserOrb.procCoefficient = this.procCoefficient/2;
						taserOrb.damageColorIndex = this.damageColorIndex;
						taserOrb.damageCoefficientPerBounce = this.damageCoefficientPerBounce;
						taserOrb.speed = this.speed;
						taserOrb.range = this.range;
						taserOrb.damageType = this.damageType;
						taserOrb.failedToKill = this.failedToKill;
						OrbManager.instance.AddOrb(taserOrb);
						//Debug.Log("Shot generic hit at " + this.bouncesRemaining);
					}
					else
					{
						hurtBox = this.PickNextTarget(this.target.transform.position, true); //if no other targets, allow a repeat
						if (hurtBox && this.bouncesRemaining >= 3)
						{
							ExecutionerTaserOrb taserOrb = new ExecutionerTaserOrb();
							taserOrb.search = this.search;
							taserOrb.origin = this.target.transform.position;
							taserOrb.target = hurtBox;
							taserOrb.attacker = this.attacker;
							taserOrb.inflictor = this.inflictor;
							taserOrb.teamIndex = this.teamIndex;
							taserOrb.damageValue = this.damageValue * this.damageCoefficientPerBounce;
							taserOrb.bouncesRemaining = -1; // prevents further bounces
							taserOrb.isCrit = this.isCrit;
							taserOrb.bouncedObjects = this.bouncedObjects;
							taserOrb.procChainMask = this.procChainMask;
							taserOrb.procCoefficient = this.procCoefficient / 2;
							taserOrb.damageColorIndex = this.damageColorIndex;
							taserOrb.damageCoefficientPerBounce = this.damageCoefficientPerBounce;
							taserOrb.speed = this.speed;
							taserOrb.range = this.range;
							taserOrb.damageType = this.damageType;
							taserOrb.failedToKill = this.failedToKill;
							OrbManager.instance.AddOrb(taserOrb);
							//Debug.Log("Shot repeat hit at " + this.bouncesRemaining + " | next shot has: " + Math.Max(this.bouncesRemaining - 3, 0));

						}
						else if (attackerAimVector != null && target.healthComponent.body) //and if all else fails, whiff
						{
							var motor = target.healthComponent.body.characterMotor;
							var stupidMotor = target.healthComponent.gameObject.GetComponent<RigidbodyMotor>();
							if ((!motor || motor.isGrounded) && !stupidMotor)
							{
								var aimVector = GenerateValidPosition(attackerAimVector);
								Vector3 axis = Vector3.Cross(Vector3.up, aimVector);

								float x = UnityEngine.Random.Range(0, 5f); //maxspread
								float z = UnityEngine.Random.Range(0f, 360f);
								Vector3 vector = Quaternion.Euler(0f, 0f, z) * (Quaternion.Euler(x, 0f, 0f) * Vector3.forward);
								float y = vector.y;
								vector.y = 0f;

								float angle = (Mathf.Atan2(vector.z, vector.x) * 57.29578f - 90f) * 2.5f; //spreadYawScale
								float angle2 = Mathf.Atan2(y, vector.magnitude) * 57.29578f;
								var finalVec = (Quaternion.AngleAxis(angle, Vector3.up) * (Quaternion.AngleAxis(angle2, axis) * aimVector));

								Vector3 ray = new Ray(this.target.transform.position, finalVec).GetPoint(21);
								var casts = Physics.RaycastAll(new Ray(this.target.transform.position, aimVector), 21, LayerIndex.world.mask | LayerIndex.entityPrecise.mask);

								//bool groundHit = false;
								if (casts.Length > 0)
								{
									ray = casts[0].point;
									//groundHit = true;
									//Debug.Log("Overriding with " + ray + " | " + casts[0].point);
								}

								ExecutionerTaserOrb taserOrb = new ExecutionerTaserOrb();
								taserOrb.search = this.search;
								taserOrb.origin = this.target.transform.position;
								taserOrb.target = null;
								taserOrb.attacker = this.attacker;
								taserOrb.inflictor = this.inflictor;
								taserOrb.teamIndex = this.teamIndex;
								taserOrb.damageValue = this.damageValue * this.damageCoefficientPerBounce;
								taserOrb.bouncesRemaining = 0;
								taserOrb.isCrit = this.isCrit;
								taserOrb.bouncedObjects = this.bouncedObjects;
								taserOrb.procChainMask = this.procChainMask;
								taserOrb.procCoefficient = this.procCoefficient / 2;
								taserOrb.damageColorIndex = this.damageColorIndex;
								taserOrb.damageCoefficientPerBounce = this.damageCoefficientPerBounce;
								taserOrb.speed = this.speed;
								taserOrb.range = this.range;
								taserOrb.damageType = this.damageType;
								taserOrb.failedToKill = this.failedToKill;
								taserOrb.targetPosition = ray;
								taserOrb.attackerAimVector = aimVector;
								//taserOrb.groundHit = groundHit;
								OrbManager.instance.AddOrb(taserOrb);
								Debug.Log("Shot whiff at " + this.bouncesRemaining);
							}
							else
							{
								Debug.Log("Invalid bounce target; " + target.healthComponent.body.name);
							}
						}

					}
					return;
				}
				if (!this.failedToKill)
				{
					Action<ExecutionerTaserOrb> action = ExecutionerTaserOrb.onLightningOrbKilledOnAllBounces;
					if (action == null)
					{
						return;
					}
					action(this);
				}
			}
		}

		public HurtBox PickNextTarget(Vector3 position, bool repeatsAllowed)
		{
			if (this.search == null)
				this.search = new BullseyeSearch();

			this.search.searchOrigin = position;
			this.search.searchDirection = Vector3.zero;
			this.search.teamMaskFilter = TeamMask.all;
			this.search.teamMaskFilter.RemoveTeam(this.teamIndex);
			this.search.filterByLoS = false;
			this.search.sortMode = BullseyeSearch.SortMode.Distance;
			this.search.maxDistanceFilter = this.range;
			this.search.RefreshCandidates();

			HurtBox hurtBox = (from v in this.search.GetResults() where repeatsAllowed == this.bouncedObjects.Contains(v.healthComponent) && v.healthComponent != this.target.healthComponent select v).FirstOrDefault<HurtBox>();

			if (hurtBox)
            {
                if (!repeatsAllowed)
                {
					Debug.Log("newTarget");
					this.bouncedObjects.Add(hurtBox.healthComponent);
				}
				Debug.Log("hitting original guy ");
				
            }
            else if(repeatsAllowed)
            {
				if(this.target.healthComponent.alive)
                {
					Debug.Log("No options, hitting self");
					hurtBox = this.target;
                }
				//hurtBox = (from v in this.search.GetResults() where repeatsAllowed == this.bouncedObjects.Contains(v.healthComponent) && v.healthComponent.alive select v).FirstOrDefault<HurtBox>(); //should i just do this.target? what if it's dead?
			}

			return hurtBox;
		}


		public Vector3 GenerateValidPosition(Vector3 aimVec)
		{
			float x = aimVec.x + (UnityEngine.Random.value > 0.5f ? UnityEngine.Random.Range(.2f, .4f) : UnityEngine.Random.Range(-.4f, -.2f));
			float z = aimVec.z + (UnityEngine.Random.value > 0.5f ? UnityEngine.Random.Range(.2f, .4f) : UnityEngine.Random.Range(-.4f, -.2f));
			float y = UnityEngine.Random.Range(-.25f, -.1f);

			if (x >= 1)
				x -= 2;
			else if (x <= -1)
				x += 2;

			if (z >= 1)
				z -= 2;
			else if (z <= -1)
				z += 2;

			return new Vector3(x, y, z);
		}

		public static event Action<ExecutionerTaserOrb> onLightningOrbKilledOnAllBounces;

		public float speed = 75f;

		public float damageValue;

		public GameObject attacker;

		public GameObject inflictor;

		public int bouncesRemaining;

		public List<HealthComponent> bouncedObjects;

		public TeamIndex teamIndex;

		public bool isCrit;

		public ProcChainMask procChainMask;

		public float procCoefficient = .7f;

		public DamageColorIndex damageColorIndex;

		public float range = 21;

		public float damageCoefficientPerBounce = 1f;

		public int targetsToFindPerBounce = 1;

		public DamageType damageType;

		private bool canBounceOnSameTarget;

		private bool failedToKill;

		private BullseyeSearch search;

		public bool groundHit;

		private GameObject taserVFXMastery;

        public class TaserWhiffComponent : MonoBehaviour
		{
			public int reduction = 2;

		}
	}
}
