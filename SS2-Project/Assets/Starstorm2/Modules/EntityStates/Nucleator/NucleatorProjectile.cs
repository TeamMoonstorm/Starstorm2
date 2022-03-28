using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Nucleator
{
    // Token: 0x02000603 RID: 1539
    [RequireComponent(typeof(ProjectileController))]
    [RequireComponent(typeof(ProjectileDamage))]
    public class NucleatorProjectile : MonoBehaviour
    {
        private ProjectileController projectileController;
        private ProjectileImpactExplosion projectileImpactExplosion;
        private ProjectileDamage projectileDamage;

        private float duration;
        private float fixedAge;

        private Rigidbody rigidbody;
        private float frequency = 14.0f;  // Speed of sine movement
        private float magnitude = 0.25f;   // Size of sine movement
        private Vector3 axis;
        private Vector3 pos;

        public TeamIndex teamIndex;
        public float baseDuration = 2f;
        public float acceleration = 50f;
        public GameObject rotateObject;
        public float removalTime = 1f;
        public float baseRadius = 0f;
        public float baseDamage = 0f;
        public float charge;

        private void Awake()
        {
            projectileController = GetComponent<ProjectileController>();
            projectileDamage = GetComponent<ProjectileDamage>();
            projectileImpactExplosion = GetComponent<ProjectileImpactExplosion>();
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.useGravity = false;
            duration = baseDuration;
            fixedAge = 0f;
            baseDamage = projectileDamage.damage;
            projectileImpactExplosion.lifetime = baseDuration;
            acceleration = (1 - charge) * (50f - 20f) + 20f;

            pos = transform.position;
            axis = transform.up * -1;  // May or may not be the axis you want
        }

        private void FixedUpdate()
        {
            fixedAge += Time.deltaTime;
            var lifetimeCoef = fixedAge / baseDuration;

            if (fixedAge < baseDuration)
            {
                projectileImpactExplosion.blastRadius = baseRadius + lifetimeCoef * (baseRadius * 1.25f - 1) + 1;
                projectileDamage.damage = baseDamage + lifetimeCoef * (baseDamage * 1.25f - 1) + 1;

                pos += transform.forward * Time.deltaTime * acceleration;
                transform.position = pos + axis * Mathf.Sin(Time.time * frequency) * magnitude;
            }


        }


    }
}