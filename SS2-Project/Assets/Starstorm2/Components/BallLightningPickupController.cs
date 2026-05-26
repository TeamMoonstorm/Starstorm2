using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using SS2.Items;

namespace SS2.Components
{
    [RequireComponent(typeof(TeamFilter))]
    public class BallLightningPickupController : MonoBehaviour
    {
        public GameObject spawnEffectPrefab;
        public GameObject lifetimeExpiredEffectPrefab;

        public float lifetime = 24f;
        public float spawnHeight = 2.5f;
        public float minForwardSpeed = 3f;
        public float maxForwardSpeed = 20f;
        public float yVelocityCoefficient = 0.33f;
        private Rigidbody rigidbody;

        private float stopwatch;
        public void Awake()
        {
            if (spawnEffectPrefab)
            {
                EffectManager.SimpleEffect(spawnEffectPrefab, transform.position, Quaternion.identity, false);
            }

            transform.position += spawnHeight * Vector3.up;

            GetComponent<TeamFilter>().teamIndex = TeamIndex.Player;
            rigidbody = GetComponent<Rigidbody>();

            if (NetworkServer.active)
            {
                Vector3 randomDirection = UnityEngine.Random.insideUnitSphere;
                randomDirection.y *= yVelocityCoefficient;
                randomDirection = randomDirection.normalized;

                float randomSpeed = UnityEngine.Random.Range(minForwardSpeed, maxForwardSpeed);

                rigidbody.velocity = (randomSpeed * randomDirection);
            }
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                stopwatch += Time.fixedDeltaTime;

                if (stopwatch >= lifetime)
                {
                    if (lifetimeExpiredEffectPrefab)
                    {
                        EffectManager.SimpleEffect(lifetimeExpiredEffectPrefab, transform.position, Quaternion.identity, true);
                        Destroy(gameObject);
                    }
                }
            }
            


        }


    }
}
