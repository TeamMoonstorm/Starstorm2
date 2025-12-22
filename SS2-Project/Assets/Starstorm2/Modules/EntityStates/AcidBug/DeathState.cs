using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.AcidBug

{
    public class DeathState : GenericCharacterDeath
    {
        public static GameObject initialExplosionEffect;
        public static GameObject deathExplosionEffect;
        private static string initialSoundString;
        private static string deathSoundString;

        private static float extraForce = 60f;
        private static float forceMultiplier = 2f;

        private static float deathDuration = 8f;
        private static float extraGravity = 0.33f;

        private RigidbodyCollisionListener rigidbodyCollisionListener;

        public class RigidbodyCollisionListener : MonoBehaviour
        {
            private void OnCollisionEnter(Collision collision)
            {
                deathState.OnImpactServer(collision.GetContact(0).point, collision.GetContact(0).normal);
                deathState.Explode();
            }

            public DeathState deathState;
        }

        public override bool shouldAutoDestroy => false;

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(initialSoundString, gameObject);
            if (rigidbodyMotor)
            {
                rigidbodyMotor.enabled = false;
                rigidbody.useGravity = true;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

                if (NetworkServer.active && TryGetComponent(out SS2.Components.AcidBugForceCollector forceCollector))
                {
                    var physForceFlags = PhysForceInfo.Create();
                    physForceFlags.resetVelocity = true;
                    physForceFlags.force = forceCollector.force * forceMultiplier + forceCollector.force.normalized * extraForce;
                    healthComponent.TakeDamageForce(physForceFlags);
                }
            }
            if (rigidbodyDirection)
            {
                rigidbodyDirection.enabled = false;
            }
            if (initialExplosionEffect && characterBody)
            {
                EffectManager.SpawnEffect(initialExplosionEffect, new EffectData
                {
                    origin = characterBody.corePosition,
                }, false);
            }
            if (isAuthority)
            {
                rigidbodyCollisionListener = gameObject.AddComponent<DeathState.RigidbodyCollisionListener>();
                if (rigidbodyCollisionListener)
                {
                    rigidbodyCollisionListener.deathState = this;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (rigidbody)
            {
                rigidbody.AddForce(Physics.gravity * extraGravity, ForceMode.Acceleration);
            }
            if (NetworkServer.active)
            {
                Vector3 direction = rigidbody.velocity.normalized;
                if (Physics.Raycast(characterBody.corePosition, direction, out RaycastHit raycastHit, characterBody.radius + 1f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    OnImpactServer(raycastHit.point, raycastHit.normal);
                    Explode();
                }
                else if (fixedAge > deathDuration)
                {
                    Explode();
                }
            }
        }

        public void Explode()
        {
            Destroy(gameObject);
        }

        public virtual void OnImpactServer(Vector3 contactPoint, Vector3 contactNormal)
        {
            if (deathExplosionEffect)
            {
                EffectManager.SpawnEffect(deathExplosionEffect, new EffectData
                {
                    origin = characterBody.corePosition,
                    rotation = Util.QuaternionSafeLookRotation(contactNormal),
                }, true);
            }
        }

        public override void OnExit()
        {
            if (rigidbodyCollisionListener)
            {
                Destroy(rigidbodyCollisionListener);
            }
            Util.PlaySound(deathSoundString, gameObject);
            base.OnExit();
        }

        
    }
}
