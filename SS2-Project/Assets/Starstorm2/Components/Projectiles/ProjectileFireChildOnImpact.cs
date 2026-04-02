using System;
using System.Collections.Generic;
using HG;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    [RequireComponent(typeof(ProjectileController))]
    internal class ProjectileFireChildOnImpact : MonoBehaviour, IProjectileImpactBehavior
    {
        public GameObject childPrefab;
        public bool randomYRotation = true;

        public float childDamageCoefficient = 1f;
        public bool childInheritDamageType = true;

        private ProjectileController projectileController;
        private ProjectileDamage projectileDamage;
        private void Awake()
        {
            projectileController = GetComponent<ProjectileController>();
            projectileDamage = GetComponent<ProjectileDamage>();
        }

        // need this method to be able to disable component in editor
        private void FixedUpdate()
        {
            
        }

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            if (!enabled)
            {
                return;
            }
            FireChild(impactInfo.estimatedPointOfImpact, impactInfo.estimatedImpactNormal, impactInfo.collider);
        }

        protected void FireChild(Vector3 hitPosition, Vector3 hitNormal, Collider hitCollider)
        {
            Quaternion childRotation;
            if (randomYRotation)
            {
                childRotation = Quaternion.Euler(0f, 360f, 0f) * Util.QuaternionSafeLookRotation(hitNormal);
            }
            else
            {
                childRotation = Util.QuaternionSafeLookRotation(hitNormal);
            }
            GameObject childObject = UnityEngine.Object.Instantiate<GameObject>(childPrefab, transform.position, childRotation);

            childObject.GetComponent<TeamFilter>().teamIndex = GetComponent<TeamFilter>().teamIndex;

            ProjectileController childProjectileController = childObject.GetComponent<ProjectileController>();
            if (childProjectileController)
            {
                childProjectileController.procChainMask = projectileController.procChainMask;
                childProjectileController.procCoefficient = projectileController.procCoefficient;
                childProjectileController.owner = projectileController.owner;
            }

            ProjectileDamage childProjectileDamage = childObject.GetComponent<ProjectileDamage>();
            if (childProjectileDamage)
            {
                childProjectileDamage.damage = projectileDamage.damage * childDamageCoefficient;
                childProjectileDamage.crit = projectileDamage.crit;
                childProjectileDamage.force = projectileDamage.force;
                childProjectileDamage.damageColorIndex = projectileDamage.damageColorIndex;
                if (childInheritDamageType)
                {
                    childProjectileDamage.damageType = projectileDamage.damageType;
                }
            }

            ProjectileStickOnImpact childStickOnImpact = childObject.GetComponent<ProjectileStickOnImpact>();
            if (childStickOnImpact)
            {
                childStickOnImpact.TrySticking(hitCollider, hitNormal);
            }


            NetworkServer.Spawn(childObject);
        }
    }
}
