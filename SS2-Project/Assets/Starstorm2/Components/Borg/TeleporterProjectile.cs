using UnityEngine;
using RoR2.Projectile;
using RoR2;
using RoR2.Skills;
using RoR2.Orbs;
using System.Collections.Generic;
namespace SS2.Components
{
    public class TeleporterProjectile : MonoBehaviour
    {
        private ProjectileController controller;
        private GameObject owner;
        private ProjectileTeleporterOwnership ownership;
		public float damageRadius = 10f;
		public Transform indicatorStartTransform;
		public float duration = 3f;
		private float stopwatch;
		private bool hasTeleported;
        void Start()
        {
            this.controller = base.GetComponent<ProjectileController>();
            this.owner = this.controller.owner;

            if(this.owner)
            {
                this.ownership = this.owner.AddComponent<ProjectileTeleporterOwnership>();
                this.ownership.teleporter = this;
                
            }
        }
        private void FixedUpdate()
        {
			stopwatch += Time.fixedDeltaTime;
			if(!hasTeleported && owner && stopwatch >= duration)
            {
				hasTeleported = true;
				EntityStateMachine body = EntityStateMachine.FindByCustomName(owner, "Body");
				if(body)
                {
					body.SetInterruptState(new EntityStates.Cyborg2.Teleport(), EntityStates.InterruptPriority.Vehicle);
                }
            }
        }
        private void Update()
        {
            if(indicatorStartTransform && owner)
            {
				indicatorStartTransform.position = owner.transform.position;
            }
        }
        public Vector3 GetSafeTeleportPosition()
        {
            //lol
            return base.transform.position;
        }
        public void OnTeleport()
        {
			Destroy(base.gameObject);
			if (!this.owner) return;
			//vfx
			//sound
			//teleporter anim??
        }

		
        public class ProjectileTeleporterOwnership : MonoBehaviour
        {
            public TeleporterProjectile teleporter;
            private SkillDef teleportSkillDef;
            private CharacterBody body;
            private SkillLocator skillLocator;

            public static bool destroyOnFirstTeleport = false;
            private void Awake()
            {
                //this.body = base.GetComponent<CharacterBody>();

                //this.teleportSkillDef = SS2Assets.LoadAsset<SkillDef>("Cyborg2Teleport", SS2Bundle.Indev);
                //if(this.body)
                //{
                //    this.skillLocator = body.skillLocator;
                //    this.skillLocator.utility.SetSkillOverride(this, teleportSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                //}
            }

            public void DoTeleport()
            {
                this.teleporter.OnTeleport();

                if(destroyOnFirstTeleport)
                    this.UnsetOverride();
            }

            public void UnsetOverride()
            {
                //if(this.skillLocator)
                //{
                //    this.skillLocator.utility.UnsetSkillOverride(this, teleportSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                //}
                Destroy(this);
            }

            private void FixedUpdate()
            {
                if(!this.teleporter)
                {
                    this.UnsetOverride();
                }
            }
        }
    }

	public class TeleporterOrb : Orb
	{
		public override void Begin()
		{
			base.duration = 0.1f;
			this.target = PickNextTarget(origin);
			if(this.target)
            {
				this.targetObjects.Remove(this.target.healthComponent);
				// this.totalDuration / this.targetObjects.Count;
				EffectData effectData = new EffectData
				{
					origin = this.origin,
					genericFloat = base.duration
				};
				effectData.SetHurtBoxReference(this.target);
				EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("TeleporterDamageOrbEffect", SS2Bundle.Indev), effectData, true);
			}			
		}

		public override void OnArrival()
		{
			if (this.target)
			{
				HealthComponent healthComponent = this.target.healthComponent;
				if (healthComponent && healthComponent.alive)
				{
					DamageInfo damageInfo = new DamageInfo();
					damageInfo.damage = this.damageValue;
					damageInfo.attacker = this.attacker;
					damageInfo.inflictor = this.inflictor;
					damageInfo.force = Vector3.zero;
					damageInfo.crit = this.isCrit;
					damageInfo.procChainMask = this.procChainMask;
					damageInfo.procCoefficient = 1f;
					damageInfo.position = this.target.transform.position;
					damageInfo.damageColorIndex = DamageColorIndex.Default;
					damageInfo.damageType = DamageType.Shock5s;					
					healthComponent.TakeDamage(damageInfo);
					GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
					GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
					healthComponent.body.AddBuff(SS2Content.Buffs.BuffCyborgPrimed);
				}

				HurtBox hurtBox = this.PickNextTarget(this.target.transform.position);
				if (hurtBox)
				{
					TeleporterOrb lightningOrb = new TeleporterOrb();
					lightningOrb.origin = this.target.transform.position;
					lightningOrb.target = hurtBox;
					lightningOrb.attacker = this.attacker;
					lightningOrb.inflictor = this.inflictor;
					lightningOrb.teamIndex = this.teamIndex;
					lightningOrb.isCrit = this.isCrit;
					lightningOrb.targetObjects = this.targetObjects;
					lightningOrb.procChainMask = default(ProcChainMask);
					lightningOrb.procCoefficient = 1f;
					lightningOrb.damageColorIndex = DamageColorIndex.Default;
					lightningOrb.damageType = DamageType.Shock5s;
					OrbManager.instance.AddOrb(lightningOrb);
				}
			}
		}

		public HurtBox PickNextTarget(Vector3 position)
		{
			if (this.targetObjects == null || this.targetObjects.Count == 0)
			{
				return null;
			}

			//sort by distance
			HurtBox hurtBox = targetObjects[0].body.mainHurtBox;
			float maxDistance = Mathf.Infinity;
			for(int i = 0; i < targetObjects.Count; i++)
            {
				Vector3 between = targetObjects[i].transform.position - position;
				if (between.magnitude < maxDistance)
                {
					hurtBox = targetObjects[i].body.mainHurtBox;
					maxDistance = between.magnitude;
                }
            }
			return hurtBox;
		}
		public float totalDuration;

		public float damageValue;

		public GameObject attacker;

		public GameObject inflictor;

		public List<HealthComponent> targetObjects;

		public TeamIndex teamIndex;

		public bool isCrit;

		public ProcChainMask procChainMask;

		public float procCoefficient = 1f;

		public DamageColorIndex damageColorIndex;

		public DamageType damageType;
	}
}

