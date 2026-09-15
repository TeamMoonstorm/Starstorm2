using System.Collections.Generic;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    [RequireComponent(typeof(ProjectileController))]
    public class LancerSpearProjectile : NetworkBehaviour, IProjectileImpactBehavior
    {
        public enum SpearProjectileState : byte
        {
            Flying = 0,
            Stuck = 1,
            Returning = 2
        }

        public float travelSpeed = 80f;
        public float returnSpeed = 60f;
        public float returnAcceleration = 5f;
        public string stickSoundString = "";
        public float catchDistance = 3f;

        public static float ionFieldPassThroughCoefficient = 1f;

        private ProjectileController projectileController;
        private Rigidbody rigidbody;
        private ProjectileDamage projectileDamage;
        private bool hasRegisteredWithOwner;

        // Stick data
        private Transform stuckTransform;
        private Vector3 localStuckPosition;
        private Quaternion localStuckRotation;
        private CharacterBody stuckBody;

        // Return pass-through tracking
        private HashSet<HealthComponent> returnHitTargets = new HashSet<HealthComponent>();

        [SyncVar]
        private SpearProjectileState _state;

        private void Awake()
        {
            if (!TryGetComponent(out projectileController))
                Debug.LogError("LancerSpearProjectile: Failed to get ProjectileController.");
            if (!TryGetComponent(out rigidbody))
                Debug.LogError("LancerSpearProjectile: Failed to get Rigidbody.");
            if (!TryGetComponent(out projectileDamage))
                Debug.LogError("LancerSpearProjectile: Failed to get ProjectileDamage.");
        }

        private void Start()
        {
            if (!projectileController.owner)
            {
                if (NetworkServer.active)
                    Object.Destroy(gameObject);
                return;
            }

            if (projectileController.owner.TryGetComponent(out LancerController lancerController))
            {
                lancerController.SetSpearProjectile(gameObject);
                hasRegisteredWithOwner = true;
            }
            else
            {
                Debug.LogError("LancerSpearProjectile: Owner missing LancerController.");
            }
        }

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            if (_state != SpearProjectileState.Flying)
                return;

            if (impactInfo.collider)
            {
                HurtBox hurtBox = impactInfo.collider.GetComponent<HurtBox>();
                if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.gameObject == projectileController.owner)
                    return;
            }

            if (NetworkServer.active)
            {
                HurtBox hurtBox = impactInfo.collider?.GetComponent<HurtBox>();

                // Deal damage on hit (replaces removed ProjectileSingleTargetImpact)
                if (hurtBox && hurtBox.healthComponent)
                {
                    DamageInfo damageInfo = new DamageInfo();
                    if (projectileDamage)
                    {
                        damageInfo.damage = projectileDamage.damage;
                        damageInfo.crit = projectileDamage.crit;
                        damageInfo.attacker = projectileController.owner;
                        damageInfo.inflictor = gameObject;
                        damageInfo.position = impactInfo.estimatedPointOfImpact;
                        damageInfo.force = projectileDamage.force * transform.forward;
                        damageInfo.procChainMask = projectileController.procChainMask;
                        damageInfo.procCoefficient = projectileController.procCoefficient;
                        damageInfo.damageColorIndex = projectileDamage.damageColorIndex;
                        damageInfo.damageType = projectileDamage.damageType;
                    }
                    hurtBox.healthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, hurtBox.healthComponent.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, hurtBox.healthComponent.gameObject);

                    // Apply ion field to enemy hit
                    if (hurtBox.healthComponent.body)
                    {
                        hurtBox.healthComponent.body.AddTimedBuff(Survivors.Lancer.bdIonField, Survivors.Lancer.ionFieldDuration);
                    }
                }

                Stick(impactInfo.collider, impactInfo.estimatedImpactNormal);
            }
        }

        private void Stick(Collider collider, Vector3 normal)
        {
            Transform hitTransform = collider.transform;
            HurtBox hurtBox = collider.GetComponent<HurtBox>();
            if (hurtBox && hurtBox.healthComponent)
            {
                stuckBody = hurtBox.healthComponent.body;
                hitTransform = collider.transform;
            }

            stuckTransform = hitTransform;
            localStuckPosition = hitTransform.InverseTransformPoint(transform.position);
            localStuckRotation = Quaternion.Inverse(hitTransform.rotation) * transform.rotation;

            rigidbody.velocity = Vector3.zero;
            rigidbody.isKinematic = true;
            rigidbody.detectCollisions = false;

            _state = SpearProjectileState.Stuck;

            if (!string.IsNullOrEmpty(stickSoundString))
                Util.PlaySound(stickSoundString, gameObject);
        }

        public void BeginReturn()
        {
            if (!NetworkServer.active)
                return;

            rigidbody.isKinematic = false;
            rigidbody.detectCollisions = false;
            stuckTransform = null;
            stuckBody = null;
            returnHitTargets.Clear();

            _state = SpearProjectileState.Returning;
        }

        private void FixedUpdate()
        {
            switch (_state)
            {
                case SpearProjectileState.Flying:
                    break;

                case SpearProjectileState.Stuck:
                    UpdateStuck();
                    break;

                case SpearProjectileState.Returning:
                    UpdateReturn();
                    break;
            }
        }

        private void UpdateStuck()
        {
            if (stuckTransform)
            {
                transform.SetPositionAndRotation(
                    stuckTransform.TransformPoint(localStuckPosition),
                    stuckTransform.rotation * localStuckRotation
                );
            }
            else if (_state == SpearProjectileState.Stuck)
            {
                // Stuck target was destroyed — begin returning to owner
                if (NetworkServer.active)
                {
                    BeginReturn();
                }
            }
        }

        private void UpdateReturn()
        {
            if (!projectileController.owner)
            {
                if (NetworkServer.active)
                    Object.Destroy(gameObject);
                return;
            }

            Vector3 direction = (projectileController.owner.transform.position - transform.position).normalized;
            rigidbody.velocity = direction * returnSpeed;

            // Apply ion field to enemies along return path
            if (NetworkServer.active)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, 2f, LayerIndex.entityPrecise.mask);
                foreach (Collider col in colliders)
                {
                    HurtBox hurtBox = col.GetComponent<HurtBox>();
                    if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.body)
                    {
                        if (returnHitTargets.Contains(hurtBox.healthComponent))
                            continue;

                        TeamComponent victimTeam = hurtBox.healthComponent.body.teamComponent;
                        TeamIndex ownerTeam = projectileController.teamFilter ? projectileController.teamFilter.teamIndex : TeamIndex.None;
                        if (victimTeam && victimTeam.teamIndex != ownerTeam)
                        {
                            hurtBox.healthComponent.body.AddTimedBuff(Survivors.Lancer.bdIonField, Survivors.Lancer.ionFieldDuration);
                            returnHitTargets.Add(hurtBox.healthComponent);
                        }
                    }
                }
            }

            float distToOwner = Vector3.Distance(transform.position, projectileController.owner.transform.position);
            if (distToOwner <= catchDistance)
            {
                OnCaughtByOwner();
            }
        }

        private void OnCaughtByOwner()
        {
            if (!projectileController.owner)
                return;

            if (projectileController.owner.TryGetComponent(out LancerController lancerController))
            {
                if (NetworkServer.active)
                {
                    lancerController.SetSpearState(LancerController.SpearState.Held);
                }
                lancerController.SetSpearProjectile(null);
            }
            else
            {
                Debug.LogError("LancerSpearProjectile: Owner missing LancerController on catch.");
            }

            if (NetworkServer.active)
            {
                Object.Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (hasRegisteredWithOwner && projectileController && projectileController.owner)
            {
                if (projectileController.owner.TryGetComponent(out LancerController lancerController))
                {
                    if (lancerController.GetSpearProjectile() == gameObject)
                    {
                        lancerController.SetSpearProjectile(null);
                    }
                }
            }
        }

    }
}
