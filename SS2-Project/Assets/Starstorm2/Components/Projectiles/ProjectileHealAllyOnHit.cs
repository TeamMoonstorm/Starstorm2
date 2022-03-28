using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileHealAllyOnHit : MonoBehaviour, IProjectileImpactBehavior
    {
        [Tooltip("If enabled, heals the ally by % specified in Percent Heal. Otherwise, heal is based on projectile's damage and Heal Coefficient.")]
        public bool healByPercent = true;
        public float percentHeal = 5f;
        public float healCoefficient = 1f;
        public string healAllySoundString;
        public bool applyBuff;
        public BuffDef buffToApply;
        public float buffDuration = 1f;
        [Tooltip("Set to 0 to have unlimited buff stacks.")]
        public int buffMaxStacks;

        private ProjectileController projectileController;
        private ProjectileDamage projectileDamage;
        private TeamIndex projectileTeam;
        private List<HealthComponent> hcList = new List<HealthComponent>();

        public void Awake()
        {
            projectileController = base.gameObject.GetComponent<ProjectileController>();
            projectileDamage = base.gameObject.GetComponent<ProjectileDamage>();
        }

        public void Start()
        {
            projectileTeam = projectileController.teamFilter.teamIndex;
        }

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            Collider collider = impactInfo.collider;
            if (collider)
            {
                HurtBox hurtBox = collider.GetComponent<HurtBox>();
                if (hurtBox)
                {
                    HealthComponent healthComponent = hurtBox.healthComponent;
                    if (healthComponent)
                    {
                        if (ShouldHealProceed(healthComponent, projectileTeam))
                        {
                            Util.PlaySound(healAllySoundString, base.gameObject);
                            if (NetworkServer.active)
                            {
                                if (healByPercent) healthComponent.HealFraction(percentHeal, default(ProcChainMask));
                                else
                                {
                                    if (projectileDamage) healthComponent.Heal(projectileDamage.damage * healCoefficient, default(ProcChainMask), true);
                                    else Debug.LogWarningFormat(this, "ProjectileHealAllyOnHit {0} no projectile damage component!", new object[] { this });
                                }

                                if (applyBuff)
                                {
                                    if (!buffToApply) { Debug.LogWarningFormat(this, "ProjectileHealAllyOnHit {0} has applyBuff enabled but has no buff specified", new object[] { this }); return; }

                                    if (buffMaxStacks > 0) healthComponent.body.AddTimedBuff(buffToApply, buffDuration, buffMaxStacks);
                                    else healthComponent.body.AddTimedBuff(buffToApply, buffDuration);
                                }

                                hcList.Add(healthComponent);
                            }
                        }
                    }
                }
            }
        }

        private bool ShouldHealProceed(HealthComponent target, TeamIndex allyTeam)
        {
            return target.body.teamComponent.teamIndex == allyTeam && target.body.gameObject != projectileController.owner.gameObject && target.body.teamComponent.teamIndex != TeamIndex.None && !hcList.Contains(target);
        }
    }
}
