using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using RoR2.Orbs;
namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileHealAOE : MonoBehaviour, IProjectileImpactBehavior
    {
        public float radius = 12f;
        public float percentHeal = .2f;
        public float healDamageCoefficient = 1f;
		public float orbTravelDuration = 0.5f;

        private ProjectileController projectileController;
        private TeamFilter teamFilter;
        private ProjectileDamage projectileDamage;

		private bool hasFired;
        private void Awake()
        {
            this.projectileController = base.GetComponent<ProjectileController>();
            this.teamFilter = base.GetComponent<TeamFilter>();
            this.projectileDamage = base.GetComponent<ProjectileDamage>();
        }

        private void Fire()
        {
			this.hasFired = true;
            if (!NetworkServer.active) return;

			float healValue = this.projectileDamage.damage * this.healDamageCoefficient;
			List<HealthComponent> list = new List<HealthComponent>();
			SphereSearch sphereSearch = new SphereSearch();
			TeamMask teamMask = new TeamMask();
			teamMask.AddTeam(this.teamFilter.teamIndex);
			sphereSearch.radius = this.radius;
			sphereSearch.origin = base.transform.position;
			sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Collide;
			sphereSearch.mask = LayerIndex.entityPrecise.mask;
			sphereSearch.RefreshCandidates();
			sphereSearch.FilterCandidatesByDistinctHurtBoxEntities().FilterCandidatesByHurtBoxTeam(teamMask);
			HurtBox[] hurtBoxes = sphereSearch.GetHurtBoxes();
			for (int i = 0; i < hurtBoxes.Length; i++)
			{
				HealthComponent healthComponent = hurtBoxes[i].healthComponent;
				if (!list.Contains(healthComponent))
				{
					list.Add(healthComponent);
				}
			}
			foreach (HealthComponent healthComponent2 in list)
			{
				float finalHealValue = healValue + healthComponent2.fullHealth * percentHeal;
				HealOrb healOrb = new HealOrb();
				healOrb.origin = base.transform.position;
				healOrb.target = healthComponent2.body.mainHurtBox;
				healOrb.healValue = finalHealValue;
				healOrb.overrideDuration = this.orbTravelDuration;
				OrbManager.instance.AddOrb(healOrb);
			}
			//EffectManager.SimpleEffect(EntityStates.AffixEarthHealer.Heal.effectPrefab, base.transform.position, Quaternion.identity, true);
			
		}

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
			if(!this.hasFired)
				this.Fire();
        }
    }
}
