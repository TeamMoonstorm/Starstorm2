using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace SS2
{
    public class RevolverTurretController : NetworkBehaviour
    {
        private static Vector3 positionOffset = new Vector3(-2.5f, 0.3f, 0f);
        private static float offsetMaxHeight = 0.5f;
        private static float offsetMax = 1.5f;
        private static float distanceDampTime = 0.2f;

        private Vector3 velocityDistance;
        private Vector3 randomizedOffset;

        [NonSerialized]
        [SyncVar]
        public GameObject owner;
        [NonSerialized]
        public int shotCount;
        [NonSerialized]
        public float damageValue;
        [NonSerialized]
        public float procCoefficient;
        [NonSerialized]
        public bool crit;
        [NonSerialized]
        public ProcChainMask procChainMask;

        private CharacterDirection ownerDirection;

        public static int GetTurretCountForObject(GameObject gameObject)
        {
            int count = 0;
            var instances = InstanceTracker.GetInstancesList<RevolverTurretController>();
            for (int i = 0; i < instances.Count; i++)
            {
                if (instances[i] && instances[i].owner == gameObject)
                {
                    count++;
                }
            }
            return count;
        }
        private void OnEnable()
        {
            InstanceTracker.Add(this);
        }
        private void OnDisable()
        {
            InstanceTracker.Remove(this);
        }
        public override void OnStartClient()
        {
            if (owner)
            {
                ownerDirection = owner.GetComponent<CharacterDirection>();

                Vector3 offset = UnityEngine.Random.insideUnitSphere * offsetMax;
                offset.y *= offsetMaxHeight;
                randomizedOffset = offset + positionOffset;
            }
        }
        private void Update()
        {
            if (Util.HasEffectiveAuthority(base.gameObject))
            {
                if (owner)
                {
                    Transform targetTransform = owner.transform;
                    Vector3 offset = targetTransform.rotation * randomizedOffset;
                    if (ownerDirection)
                    {
                        offset = Util.QuaternionSafeLookRotation(ownerDirection.forward) * randomizedOffset;
                    }
                    transform.position = Vector3.SmoothDamp(transform.position, targetTransform.position + offset, ref this.velocityDistance, distanceDampTime);
                }
            }
        }
    }
}