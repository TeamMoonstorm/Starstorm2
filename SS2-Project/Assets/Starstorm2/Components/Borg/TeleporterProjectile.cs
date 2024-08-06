using UnityEngine;
using RoR2.Projectile;
using RoR2;
using RoR2.Skills;
using RoR2.Orbs;
using System.Collections.Generic;
using UnityEngine.Networking;
namespace SS2.Components
{
    public class TeleporterProjectile : MonoBehaviour
    {
        private ProjectileController controller;
        private GameObject owner;
		private CharacterBody ownerBody;
        private ProjectileTeleporterOwnership ownership;
		public float teleportRadius = 10f;
		public Transform indicatorStartTransform;
		private float stopwatch;
		private bool hasTeleported;

		private static float funnyNumber = 0.5f;
        void Start()
        {
            this.controller = base.GetComponent<ProjectileController>();
            this.owner = this.controller.owner;
            if(this.owner)
            {
                this.ownership = this.owner.AddComponent<ProjectileTeleporterOwnership>();
                this.ownership.teleporter = this;
				ownerBody = owner.GetComponent<CharacterBody>();
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
        public void OnTeleport(Vector3 startPosition)
        {
			Destroy(base.gameObject);
			if (!this.owner) return;

			if(NetworkServer.active)
            {
				SphereSearch search = new SphereSearch();
				search.radius = teleportRadius;
				search.origin = base.transform.position;
				search.mask = LayerIndex.entityPrecise.mask;
				TeamMask mask = TeamMask.GetUnprotectedTeams(this.controller.teamFilter.teamIndex);
				HurtBox[] hurtBoxes = search.RefreshCandidates().FilterCandidatesByHurtBoxTeam(mask).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

				for(int i = 0; i < hurtBoxes.Length; i++)
                {
					if(hurtBoxes[i].healthComponent)
                    {
						CharacterBody body = hurtBoxes[i].healthComponent.body;
						Vector3 offset = body.footPosition - base.transform.position;
						TeleportHelper.TeleportBody(body, startPosition + (offset*funnyNumber));

					}
						
                }
				
			}

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

            public static bool destroyOnFirstTeleport = true;
            private void Awake()
            {
                this.body = base.GetComponent<CharacterBody>();

                this.teleportSkillDef = SS2Assets.LoadAsset<SkillDef>("Cyborg2Teleport", SS2Bundle.Indev);
                if (this.body)
                {
                    this.skillLocator = body.skillLocator;
                    this.skillLocator.utility.SetSkillOverride(this, teleportSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
            }

            public void DoTeleport(Vector3 startPosition)
            {
                this.teleporter.OnTeleport(startPosition);

                if(destroyOnFirstTeleport)
                    this.UnsetOverride();
            }

            public void UnsetOverride()
            {
                if (this.skillLocator)
                {
                    this.skillLocator.utility.UnsetSkillOverride(this, teleportSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
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
}

