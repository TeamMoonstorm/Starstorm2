using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using RoR2;
namespace SS2.Components
{
	// haha.
	public class DamageTrail2 : MonoBehaviour
	{
		public DamageType damageType;
		public float procCoefficient = 0.1f;

		// Token: 0x060029A5 RID: 10661 RVA: 0x0001E6B2 File Offset: 0x0001C8B2
		[MSU.AsyncAssetLoad]
		private static void Init()
		{
			RoR2Application.onUpdate += DamageTrail.UpdateOptimizedDamageUpdateInterval;
		}

		// Token: 0x060029A6 RID: 10662 RVA: 0x0001E6C5 File Offset: 0x0001C8C5
		private void Awake()
		{
			this.pointsList = new List<DamageTrail2.TrailPoint>();
			this.transform = base.transform;
		}

		// Token: 0x060029A7 RID: 10663 RVA: 0x0001E6DE File Offset: 0x0001C8DE
		private void Start()
		{
			this.localTime = 0f;
			this.AddPoint();
			this.AddPoint();
		}

		// Token: 0x060029A9 RID: 10665 RVA: 0x000E4CBC File Offset: 0x000E2EBC
		private void OnDisable()
		{
			if (EffectManager.UsePools)
			{
				for (int i = this.pointsList.Count - 1; i >= 0; i--)
				{
					if (this.pointsList[i].segmentTransform)
					{
						GameObject gameObject = this.pointsList[i].segmentTransform.gameObject;
						EffectManagerHelper component = gameObject.GetComponent<EffectManagerHelper>();
						if (component != null && component.OwningPool != null)
						{
							component.OwningPool.ReturnObject(component);
						}
						else
						{
							Debug.LogFormat("DamageTrail.OnDestroy: No EFH on {0} ({1})", new object[]
							{
								gameObject.name,
								gameObject.GetInstanceID()
							});
						}
					}
					this.pointsList.RemoveAt(i);
				}
			}
		}

		private void FixedUpdate()
		{
			this.localTime += Time.fixedDeltaTime;
			if (this.localTime >= this.nextTrailPointUpdate)
			{
				this.nextTrailPointUpdate += this.pointUpdateInterval;
				this.UpdateTrail(this.active);
			}
			if (this.localTime >= this.nextTrailDamageUpdate)
			{
				this.nextTrailDamageUpdate += DamageTrail2.optimizedDamageUpdateinterval;
				this.DoDamage(DamageTrail2.optimizedDamageUpdateinterval);
			}
			if (this.pointsList.Count > 0)
			{
				DamageTrail2.TrailPoint trailPoint = this.pointsList[this.pointsList.Count - 1];
				if (this.active) trailPoint.position = this.transform.position; // added active check so this point doesnt follow while inactive
				trailPoint.localEndTime = this.localTime + this.pointLifetime;
				this.pointsList[this.pointsList.Count - 1] = trailPoint;
				if (this.active && trailPoint.segmentTransform) // ^^ same here
				{
					trailPoint.segmentTransform.position = this.transform.position;
				}
				if (this.lineRenderer)
				{
					this.lineRenderer.SetPosition(this.pointsList.Count - 1, trailPoint.position);
				}
			}
			if (this.segmentPrefab)
			{
				Vector3 position = this.transform.position;
				for (int i = this.pointsList.Count - 1; i >= 0; i--)
				{
					Transform segmentTransform = this.pointsList[i].segmentTransform;
					if (segmentTransform)
					{
						segmentTransform.LookAt(position, Vector3.up);
						Vector3 a = this.pointsList[i].position - position;
						segmentTransform.position = position + a * 0.5f;
						float num = Mathf.Clamp01(Mathf.InverseLerp(this.pointsList[i].localStartTime, this.pointsList[i].localEndTime, this.localTime));
						Vector3 localScale = new Vector3(this.radius * (1f - num), this.radius * (1f - num), a.magnitude);
						segmentTransform.localScale = localScale;
						position = this.pointsList[i].position;
					}
				}
			}
		}

		private void UpdateTrail(bool addPoint)
		{
			while (this.pointsList.Count > 0 && this.pointsList[0].localEndTime <= this.localTime)
			{
				this.RemovePoint(0);
			}
			if (addPoint)
			{
				this.AddPoint();
			}
			if (this.lineRenderer)
			{
				this.UpdateLineRenderer(this.lineRenderer);
			}
		}

		private void DoDamage(float damageInterval)
		{
			if (!NetworkServer.active || this.pointsList.Count == 0)
			{
				return;
			}
			Vector3 b = this.pointsList[this.pointsList.Count - 1].position;
			this.ignoredObjects.Clear();
			TeamIndex attackerTeamIndex = TeamIndex.Neutral;
			float damage = this.damagePerSecond * damageInterval;
			if (this.owner)
			{
				this.ignoredObjects.Add(this.owner);
				attackerTeamIndex = TeamComponent.GetObjectTeam(this.owner);
			}
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.attacker = this.owner;
			damageInfo.inflictor = base.gameObject;
			damageInfo.crit = false;
			damageInfo.damage = damage;
			damageInfo.damageColorIndex = DamageColorIndex.Item;
			damageInfo.damageType = damageType;
			damageInfo.force = Vector3.zero;
			damageInfo.procCoefficient = procCoefficient;
			for (int i = this.pointsList.Count - 2; i >= 0; i--)
			{
				Vector3 position = this.pointsList[i].position;
				Vector3 forward = position - b;
				Vector3 halfExtents = new Vector3(this.radius, this.height, forward.magnitude);
				Vector3 center = Vector3.Lerp(position, b, 0.5f);
				Quaternion orientation = Util.QuaternionSafeLookRotation(forward);
				Collider[] array;
				int num = HGPhysics.OverlapBox(out array, center, halfExtents, orientation, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);
				for (int j = 0; j < num; j++)
				{
					HurtBox component = array[j].GetComponent<HurtBox>();
					if (component)
					{
						HealthComponent healthComponent = component.healthComponent;
						if (healthComponent)
						{
							GameObject gameObject = healthComponent.gameObject;
							if (!this.ignoredObjects.Contains(gameObject) && FriendlyFireManager.ShouldSplashHitProceed(healthComponent, attackerTeamIndex))
							{
								this.ignoredObjects.Add(gameObject);
								damageInfo.position = array[j].transform.position;
								healthComponent.TakeDamage(damageInfo);
								GlobalEventManager.instance.OnHitEnemy(damageInfo, gameObject);
							}
						}
					}
				}
				HGPhysics.ReturnResults(array);
				b = position;
			}
		}

		private void UpdateLineRenderer(LineRenderer lineRenderer)
		{
			lineRenderer.positionCount = this.pointsList.Count;
			for (int i = 0; i < this.pointsList.Count; i++)
			{
				lineRenderer.SetPosition(i, this.pointsList[i].position);
			}
		}

		private void AddPoint()
		{
			DamageTrail2.TrailPoint item = new DamageTrail2.TrailPoint
			{
				position = this.transform.position,
				localStartTime = this.localTime,
				localEndTime = this.localTime + this.pointLifetime
			};
			if (this.segmentPrefab)
			{
				if (!EffectManager.ShouldUsePooledEffect(this.segmentPrefab))
				{
					item.segmentTransform = UnityEngine.Object.Instantiate<GameObject>(this.segmentPrefab, this.transform).transform;
				}
				else
				{
					EffectManagerHelper andActivatePooledEffect = EffectManager.GetAndActivatePooledEffect(this.segmentPrefab, this.transform, true);
					item.segmentTransform = andActivatePooledEffect.gameObject.transform;
				}
			}
			this.pointsList.Add(item);
		}

		private void RemovePoint(int pointIndex)
		{
			if (this.destroyTrailSegments)
			{
				if (this.pointsList[pointIndex].segmentTransform)
				{
					if (!EffectManager.UsePools)
					{
						UnityEngine.Object.Destroy(this.pointsList[pointIndex].segmentTransform.gameObject);
					}
					else
					{
						GameObject gameObject = this.pointsList[pointIndex].segmentTransform.gameObject;
						EffectManagerHelper component = gameObject.GetComponent<EffectManagerHelper>();
						if (component != null && component.OwningPool != null)
						{
							component.OwningPool.ReturnObject(component);
						}
						else
						{
							Debug.LogFormat("DamageTrail.RemovePoint: No EFH on {0} ({1})", new object[]
							{
								gameObject.name,
								gameObject.GetInstanceID()
							});
							UnityEngine.Object.Destroy(gameObject);
						}
					}
				}
			}
			else if (EffectManager.UsePools && this.pointsList[pointIndex].segmentTransform)
			{
				this.pointsList[pointIndex].segmentTransform.gameObject.transform.SetParent(null);
			}
			this.pointsList.RemoveAt(pointIndex);
		}

		// Token: 0x040032FE RID: 13054
		[FormerlySerializedAs("updateInterval")]
		[Tooltip("How often to drop a new point onto the trail.")]
		public float pointUpdateInterval = 0.2f;

		// Token: 0x040032FF RID: 13055
		[Tooltip("How often the damage trail should deal damage.")]
		public float damageUpdateInterval = 0.2f;

		// Token: 0x04003300 RID: 13056
		[Tooltip("How large the radius, or width, of the damage detection should be.")]
		public float radius = 0.5f;

		// Token: 0x04003301 RID: 13057
		[Tooltip("How large the height of the damage detection should be.")]
		public float height = 0.5f;

		// Token: 0x04003302 RID: 13058
		[Tooltip("How long a point on the trail should last.")]
		public float pointLifetime = 3f;

		// Token: 0x04003303 RID: 13059
		[Tooltip("The line renderer to use for display.")]
		public LineRenderer lineRenderer;

		// Token: 0x04003304 RID: 13060
		public bool active = true;

		// Token: 0x04003305 RID: 13061
		[Tooltip("Prefab to use per segment.")]
		public GameObject segmentPrefab;

		// Token: 0x04003306 RID: 13062
		public bool destroyTrailSegments;

		// Token: 0x04003307 RID: 13063
		public float damagePerSecond;

		// Token: 0x04003308 RID: 13064
		public GameObject owner;

		// Token: 0x04003309 RID: 13065
		private HashSet<GameObject> ignoredObjects = new HashSet<GameObject>();

		// Token: 0x0400330A RID: 13066
		private TeamIndex teamIndex;

		// Token: 0x0400330B RID: 13067
		private new Transform transform;

		// Token: 0x0400330C RID: 13068
		private List<DamageTrail2.TrailPoint> pointsList;

		// Token: 0x0400330D RID: 13069
		private float localTime;

		// Token: 0x0400330E RID: 13070
		private float nextTrailPointUpdate;

		// Token: 0x0400330F RID: 13071
		private float nextTrailDamageUpdate;

		// Token: 0x04003310 RID: 13072
		private static float optimizedDamageUpdateinterval;

		// Token: 0x020007CB RID: 1995
		private struct TrailPoint
		{
			// Token: 0x04003311 RID: 13073
			public Vector3 position;

			// Token: 0x04003312 RID: 13074
			public float localStartTime;

			// Token: 0x04003313 RID: 13075
			public float localEndTime;

			// Token: 0x04003314 RID: 13076
			public Transform segmentTransform;
		}
	}
}
