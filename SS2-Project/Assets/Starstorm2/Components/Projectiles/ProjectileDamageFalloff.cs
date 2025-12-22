using RoR2.Projectile;
using UnityEngine;

namespace SS2
{
    [RequireComponent(typeof(ProjectileDamage))]
    public class ProjectileDamageFalloff : MonoBehaviour
    {
        public AnimationCurve damageFalloffCurve = AnimationCurve.Constant(0, 1, 1);
        public AnimationCurve forceFalloffCurve = AnimationCurve.Constant(0, 1, 1);

        public ProjectileSimple getLifeTimeFromProjectileSimple;
        public float lifetime = 1;

        private ProjectileDamage projectileDamage;

        float initialDamage;
        float initialForce;
        float lifetimeLived;

        private void Start()
        {
            projectileDamage = GetComponent<ProjectileDamage>();
            if (getLifeTimeFromProjectileSimple)
            {
                lifetime = getLifeTimeFromProjectileSimple.lifetime;
            }
            initialDamage = projectileDamage.damage;
            initialForce = projectileDamage.force;
        }

        private void OnValidate()
        {
            if (getLifeTimeFromProjectileSimple)
            {
                lifetime = getLifeTimeFromProjectileSimple.lifetime;
            }
        }

        private void FixedUpdate()
        {
            lifetimeLived += Time.fixedDeltaTime;

            projectileDamage.damage = initialDamage * damageFalloffCurve.Evaluate(lifetimeLived / lifetime);
            projectileDamage.force = initialForce * forceFalloffCurve.Evaluate(lifetimeLived / lifetime);
        }
    }
}
