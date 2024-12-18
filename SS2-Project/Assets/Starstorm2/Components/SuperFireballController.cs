using System;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Projectile;
using RoR2.Navigation;
using System.Linq;
namespace SS2
{
	[RequireComponent(typeof(ProjectileController))]
	[RequireComponent(typeof(TeamFilter))]
	public class SuperFireballController : MonoBehaviour, IProjectileImpactBehavior
	{
		private static float stupidUpThing = 3f;
		private float targetSwitchTimer;
		private HurtBox currentTarget;
		private SphereSearch sphereSearch;
		private Rigidbody rigidbody;
		private ProjectileController controller;
		private ProjectileDamage damage;
		private int bounces;
		private float lifetimeStopwatch = 10f;

		public float blastRadius = 12f;
		[Header("Bounce Properties")]
		public int maxBounces = 2;
		public float minBounceDistance = 5f;
		public float maxBounceDistance = 50f;
		public float bounceVelocity = 24f;
		public float targetDuration = 10f;
		public float maxOffsetToTarget = 4f;
		[Header("Child Properties")]
		public int childrenCount = 4;
		public float minChildSpeed = 15f;
		public float maxChildSpeed = 30f;
		public float childUpSpeed = 25f;
		public float childDamageCoefficient = 0.5f;
		[Header("Prefab References")]
		public GameObject childPrefab;
		public GameObject dotZonePrefab;
		public GameObject impactEffectPrefab;
		private void Awake()
		{
			this.controller = base.GetComponent<ProjectileController>();
			this.damage = base.GetComponent<ProjectileDamage>();
			this.rigidbody = base.GetComponent<Rigidbody>();
			this.sphereSearch = new SphereSearch();
			sphereSearch.radius = 128f;
			sphereSearch.mask = LayerIndex.entityPrecise.mask;
		}
        private void Start()
        {
			DoBounce(); //??????
        }
        private void FixedUpdate()
        {
			if(NetworkServer.active)
            {
				targetSwitchTimer -= Time.fixedDeltaTime;
				if (targetSwitchTimer <= 0)
				{
					targetSwitchTimer = targetDuration;
					FindNewTarget();
				}
			}		
			lifetimeStopwatch -= Time.fixedDeltaTime;
			if(lifetimeStopwatch <= 0)
            {
				Destroy(base.gameObject);
            }
        }
		private bool FindNewTarget()
        {
			sphereSearch.origin = base.transform.position;
			TeamMask mask = TeamMask.GetEnemyTeams(controller.teamFilter.teamIndex);
			HurtBox hurtBox = sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(mask).FilterCandidatesByDistinctHurtBoxEntities().OrderCandidatesByDistance().GetHurtBoxes().FirstOrDefault();
			this.currentTarget = hurtBox;
			return this.currentTarget;
        }
		public void DoBounce()
        {
			// get target
			Vector3 origin = base.transform.position;
			Vector3 targetPosition = origin;
			bool hasTarget = true;
			if (!this.currentTarget && !this.FindNewTarget())
				hasTarget = false;
			if (hasTarget)
				targetPosition = currentTarget.transform.position + (UnityEngine.Random.insideUnitSphere * maxOffsetToTarget);
			else
				targetPosition = origin + UnityEngine.Random.insideUnitSphere * maxBounceDistance;
			Vector3 between = targetPosition - origin;
			float distance = Mathf.Min(between.magnitude, maxBounceDistance);
			Vector3 target = between.normalized * distance + origin;
			if (SceneInfo.instance && SceneInfo.instance.groundNodes)
			{
				NodeGraph nodes = SceneInfo.instance.groundNodes;
				if (nodes.GetNodePosition(nodes.FindClosestNode(target, HullClassification.Human), out Vector3 node))
				{
					target = node;
				}
			}
			else if(Physics.Raycast(target, Vector3.down, out RaycastHit raycastHit, 50f, LayerIndex.world.mask))
            {
				target = raycastHit.point;
            }

			// calculat evelocity
			Vector3 point = target;
			Vector3 toTarget = point - origin;
			Vector2 toTargetH = new Vector2(toTarget.x, toTarget.z);
			float hDistance = toTargetH.magnitude;
			Vector2 toTargetHDirection = toTargetH / hDistance;
			hDistance = Mathf.Clamp(hDistance, minBounceDistance, maxBounceDistance);
			float hSpeed = Trajectory.CalculateGroundSpeedToClearDistance(bounceVelocity, hDistance);
			Vector3 velocity = new Vector3(toTargetHDirection.x * hSpeed, bounceVelocity, toTargetHDirection.y * hSpeed);
			float trueSpeed = velocity.magnitude;
			Vector3 aimDirection = velocity.normalized;

			// set velocity
			this.rigidbody.velocity = aimDirection * trueSpeed;
			this.lifetimeStopwatch = 10f;
		}

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
			bounces++;
			bool hitWorld = !impactInfo.collider.GetComponent<HurtBox>();
			if (NetworkServer.active)
			{				
				//mini fireballs
				EffectData effectData = new EffectData
				{
					scale = this.blastRadius,
					origin = base.transform.position
				};
				EffectManager.SpawnEffect(impactEffectPrefab, effectData, true);
				
				Vector3 forward = Vector3.up;
				float startAngle = UnityEngine.Random.Range(0, 360f / childrenCount);
				for (int i = 0; i < childrenCount; i++)
				{
					float hSpeed = UnityEngine.Random.Range(minChildSpeed, maxChildSpeed);
					float childAngle = startAngle + (i * 360f / childrenCount);
					Vector3 rotation = Quaternion.Euler(0, childAngle, 0) * Vector3.forward;
					Vector3 velocity =  rotation * hSpeed;
					velocity.y = childUpSpeed;
					FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
					{
						projectilePrefab = childPrefab,
						position = Vector3.up * stupidUpThing + impactInfo.estimatedPointOfImpact + rotation,
						rotation = Util.QuaternionSafeLookRotation(velocity),
						procChainMask = default(ProcChainMask),
						owner = controller.owner,
						damage = damage.damage * childDamageCoefficient,
						crit = damage.crit,
						force = 200f,
						damageColorIndex = DamageColorIndex.Default,
						speedOverride = velocity.magnitude,
						useSpeedOverride = true
					};
					ProjectileManager.instance.FireProjectile(fireProjectileInfo);
					forward.x += Mathf.Sin(childAngle + UnityEngine.Random.Range(-20f, 20f));
					forward.z += Mathf.Cos(childAngle + UnityEngine.Random.Range(-20f, 20f));
				}
				//dot zone
				if(hitWorld)
                {
					FireProjectileInfo fireProjectileInfo2 = new FireProjectileInfo
					{
						projectilePrefab = dotZonePrefab,
						position = impactInfo.estimatedPointOfImpact,
						rotation = Quaternion.identity,
						procChainMask = default(ProcChainMask),
						owner = controller.owner,
						damage = damage.damage,
						crit = damage.crit,
						force = 0f,
						damageColorIndex = DamageColorIndex.Default,
					};
					ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
				}				
				//impact blastattack				
				BlastAttack blastAttack = new BlastAttack();
				blastAttack.position = base.transform.position;
				blastAttack.baseDamage = damage.damage;
				blastAttack.baseForce = damage.force;
				blastAttack.radius = this.blastRadius;
				blastAttack.attacker = controller.gameObject;
				blastAttack.inflictor = base.gameObject;
				blastAttack.teamIndex = controller.teamFilter.teamIndex;
				blastAttack.crit = damage.crit;
				blastAttack.procChainMask = controller.procChainMask;
				blastAttack.procCoefficient = controller.procCoefficient;
				blastAttack.bonusForce = Vector3.up * damage.force;
				blastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
				blastAttack.damageColorIndex = damage.damageColorIndex;
				blastAttack.damageType = damage.damageType;
				blastAttack.attackerFiltering = AttackerFiltering.Default;
				blastAttack.canRejectForce = true;
				blastAttack.Fire();
			}
			if(bounces > maxBounces) // || !hitWorld)
            {
				Destroy(base.gameObject);
				// effect here
            }
			else
				DoBounce();

		}

        private void Update()
        {
			if(rigidbody.velocity != Vector3.zero)
				base.transform.rotation = Quaternion.LookRotation(rigidbody.velocity);
        }
    }
}
