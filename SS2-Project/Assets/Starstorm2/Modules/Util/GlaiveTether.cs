using RoR2;
using RoR2.Projectile;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.PlayerLoop;

namespace SS2.Components
{
    [RequireComponent(typeof(ProjectileController))]
    public class GlaiveTether : NetworkBehaviour//, IProjectileImpactBehavior
    {
        private void Awake()
        {
            this.rigidbody = base.GetComponent<Rigidbody>();
            this.projectileController = base.GetComponent<ProjectileController>();
            this.projectileDamage = base.GetComponent<ProjectileDamage>();
            if (this.projectileController && this.projectileController.owner)
            {
                this.ownerTransform = this.projectileController.owner.transform;
            }
            this.maxFlyStopwatch = this.charge * this.distanceMultiplier;
        }

        private void Start()
        {
            float num = this.charge * 7f;
            if (num < 1f)
            {
                num = 1f;
            }
            Vector3 localScale = new Vector3(num * base.transform.localScale.x, num * base.transform.localScale.y, num * base.transform.localScale.z);
            Vector3 localRotation = Vector3.zero;
            base.transform.localScale = localScale;
            base.transform.localRotation = Quaternion.Euler(localRotation);

            var ghosttrans = base.gameObject.GetComponent<ProjectileController>().ghost.transform;

            ghosttrans.localScale = localScale;
            ghosttrans.localRotation = Quaternion.Euler(localRotation);
            base.GetComponent<ProjectileDotZone>().damageCoefficient *= num;
        }

        //public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        //{
        //    if (!this.canHitWorld)
        //    {
        //        return;
        //    }
        //    this.NetworkglaiveState = BoomerangProjectile.GlaiveState.FlyBack;
        //    UnityEvent unityEvent = this.onFlyBack;
        //    if (unityEvent != null)
        //    {
        //        unityEvent.Invoke();
        //    }
        //    EffectManager.SimpleImpactEffect(this.impactSpark, impactInfo.estimatedPointOfImpact, -base.transform.forward, true);
        //}

        private bool Reel()
        {
            Vector3 vector = this.projectileController.owner.transform.position - base.transform.position;
            Vector3 normalized = vector.normalized;
            return vector.magnitude <= 2f;
        }

        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                if (!this.setScale)
                {
                    this.setScale = true;
                }
                if (!this.projectileController.owner)
                {
                    UnityEngine.Object.Destroy(base.gameObject);
                    return;
                }
                switch (this.glaiveState)
                {
                    case GlaiveState.Hold:
                        if (NetworkServer.active)
                        {
                            this.rigidbody.velocity = new Vector3(0,0,0);
                            this.stopwatch += Time.fixedDeltaTime;
                            if (this.stopwatch >= this.maxFlyStopwatch)
                            {
                                this.stopwatch = 0f;
                                this.NetworkglaiveState = GlaiveState.Transition;
                                return;
                            }
                        }
                        break;
                    case GlaiveState.Transition:
                        {
                            this.stopwatch += Time.fixedDeltaTime;
                            float num = this.stopwatch / this.transitionDuration;
                            Vector3 a = this.CalculatePullDirection();
                            this.rigidbody.velocity = Vector3.Lerp(Vector3.zero, this.travelSpeed * a, num);
                            if (num >= 1f)
                            {
                                this.NetworkglaiveState = GlaiveState.FlyBack;
                                UnityEvent unityEvent = this.onFlyBack;
                                if (unityEvent == null)
                                {
                                    return;
                                }
                                unityEvent.Invoke();
                                return;
                            }
                            break;
                        }
                    case GlaiveState.FlyBack:
                        {
                            bool flag = this.Reel();
                            if (NetworkServer.active)
                            {
                                this.canHitWorld = false;
                                Vector3 a2 = this.CalculatePullDirection();
                                this.rigidbody.velocity = this.travelSpeed * a2;
                                if (flag)
                                {
                                    UnityEngine.Object.Destroy(base.gameObject);
                                }
                            }
                            break;
                        }
                    default:
                        return;
                }
            }
        }
        private Vector3 CalculatePullDirection()
		{
			if (this.projectileController.owner)

            {
                return (this.projectileController.owner.transform.position - base.transform.position).normalized;
            }
			return base.transform.forward;
        }

        private void UNetVersion()
        {
        }

        private GlaiveState NetworkglaiveState
        {
            get
            {
                return this.glaiveState;
            }
            [param: In]
            set
            {
                ulong newValueAsUlong = (ulong)((long)value);
                ulong fieldValueAsUlong = (ulong)((long)this.glaiveState);
                base.SetSyncVarEnum<GlaiveState>(value, newValueAsUlong, ref this.glaiveState, fieldValueAsUlong, 1U);
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write((int)this.glaiveState);
                return true;
            }
            bool flag = false;
            if ((base.syncVarDirtyBits & 1U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.Write((int)this.glaiveState);
            }
            if (!flag)
            {
                writer.WritePackedUInt32(base.syncVarDirtyBits);
            }
            return flag;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                this.glaiveState = (GlaiveState)reader.ReadInt32();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                this.glaiveState = (GlaiveState)reader.ReadInt32();
            }
        }

        public override void PreStartClient()
        {
        }

        public float travelSpeed = 40f;

        public float charge;

        public float transitionDuration;

        private float maxFlyStopwatch;

        public GameObject impactSpark;

        public GameObject crosshairPrefab;

        public bool canHitCharacters;

        public bool canHitWorld;

        private ProjectileController projectileController;

        [SyncVar]
        private GlaiveState glaiveState;

        private Transform ownerTransform;

        private ProjectileDamage projectileDamage;

        private Rigidbody rigidbody;

        private float stopwatch;

        private float fireAge;

        private float fireFrequency;

        public float distanceMultiplier = 2f;

        public UnityEvent onFlyBack;

        private bool setScale;

        private enum GlaiveState
        {
            Hold,
            Transition,
            FlyBack
        }
    }
}
