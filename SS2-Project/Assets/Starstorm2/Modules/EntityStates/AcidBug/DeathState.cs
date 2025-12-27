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
        private static string deathSoundString = "Play_jellyfish_death";

        private static float extraForce = 140f;
        private static float forceMultiplier = 3f;
        private static float wallSplatAngle = 30f;
        private static float groundNormalOffset = .074f;

        private static float deathDuration = 8f;
        private static float extraGravity = 0.33f;

        public override bool shouldAutoDestroy => false;

        private RigidbodyCollisionListener rigidbodyCollisionListener;
        public class RigidbodyCollisionListener : MonoBehaviour
        {
            private void OnCollisionEnter(Collision collision)
            {
                deathState.OnImpact(collision.GetContact(0).point, collision.GetContact(0).normal);
            }
            public DeathState deathState;
        }

        private EffectData _effectData;
        private EffectManagerHelper _emh_deathEffect;
        public override void Reset()
        {
            base.Reset();
            if (_effectData != null)
            {
                _effectData.Reset();
            }
            _emh_deathEffect = null;

        }
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(initialSoundString, gameObject);
            if (rigidbodyMotor)
            {
                rigidbodyMotor.enabled = false;
                rigidbody.useGravity = true;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rigidbodyCollisionListener = gameObject.AddComponent<DeathState.RigidbodyCollisionListener>();
                if (rigidbodyCollisionListener)
                {
                    rigidbodyCollisionListener.deathState = this;
                }

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

            Transform wingFx = FindModelChild("WingFX");
            if (wingFx)
            {
                wingFx.gameObject.SetActive(false);
            }

            Transform wingMesh = FindModelChild("WingMesh");
            if (wingMesh)
            {
                wingMesh.gameObject.SetActive(true);
            }

            if (healthComponent.killingDamageType == DamageType.OutOfBounds)
            {
                DestroyModel();
                DestroyBodyAsapServer();
            }
        }

        private void OnImpact(Vector3 hitPoint, Vector3 hitNormal)
        {
            modelLocator.enabled = false; // stop body from updating model position so we can stick it to the ground
            var position = hitPoint + (hitNormal * groundNormalOffset);
            var rotation = Quaternion.FromToRotation(Vector3.up, hitNormal) * cachedModelTransform.rotation;
            cachedModelTransform.SetPositionAndRotation(position, rotation);

            bool wallSplat = Vector3.Angle(transform.forward, Vector3.up) > wallSplatAngle;
            if (wallSplat)
            {
                PlayAnimation("Body", "SplatWall");
            }
            else
            {
                PlayAnimation("Body", "SplatGround");
            }

            if (deathExplosionEffect)
            {
                GameObject deathEffect = GameObject.Instantiate<GameObject>(deathExplosionEffect, characterBody.corePosition, Util.QuaternionSafeLookRotation(hitNormal));
                deathEffect.transform.parent = cachedModelTransform;
            }

            Explode();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (isAuthority)
            {
                if (rigidbody)
                {
                    rigidbody.AddForce(Physics.gravity * extraGravity, ForceMode.Acceleration);
                    Quaternion modelDirection = Util.QuaternionSafeLookRotation(-rigidbody.velocity);
                    rigidbody.MoveRotation(modelDirection);
                }

                if (fixedAge > deathDuration)
                {
                    Explode();
                }
            }
        }

        public void Explode()
        {
            CleanupInitialEffect();
            Destroy(gameObject);
        }
        public void CleanupInitialEffect()
        {
            if (_emh_deathEffect != null && _emh_deathEffect.OwningPool != null)
            {
                _emh_deathEffect.OwningPool.ReturnObject(_emh_deathEffect);
                _emh_deathEffect = null;
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
