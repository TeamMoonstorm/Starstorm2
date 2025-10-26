using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using RoR2;

namespace SS2
{
    [RequireComponent(typeof(GenericOwnership))]
    public class BeamController : NetworkBehaviour
    {
        [Serializable]
        public class OnTickUnityEvent : UnityEvent<BeamController>
        {
        }
        public OnTickUnityEvent onTickServer;

        public Transform startPointTransform;
        public Transform endPointTransform;
        public float tickInterval = 1f;
        public bool tickOnStart = false;
        public bool breakOnTargetFullyHealed;
        public LineRenderer lineRenderer;
        public float lingerAfterBrokenDuration;
        [SyncVar(hook = nameof(OnSyncTarget))]
        private SS2HurtBoxReference netTarget;
        private float stopwatchServer;
        private bool broken;
        private SS2HurtBoxReference previousHurtBoxReference;
        private HurtBox cachedHurtBox;
        private float scaleFactorVelocity;
        private float maxLineWidth = 0.3f;
        private float smoothTime = 0.1f;
        private float scaleFactor;

        public GenericOwnership ownership { get; private set; }
        public HurtBox target
        {
            get
            {
                return this.cachedHurtBox;
            }
            [Server]
            set
            {
                this.netTarget = SS2HurtBoxReference.FromHurtBox(value);
                this.UpdateCachedHurtBox();
            }
        }
        private void Awake()
        {
            this.ownership = base.GetComponent<GenericOwnership>();
            this.startPointTransform.SetParent(null, true);
            this.endPointTransform.SetParent(null, true);
            if (this.tickOnStart)
            {
                this.stopwatchServer = this.tickInterval;
            }
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            this.UpdateCachedHurtBox();
        }
        private void OnDestroy()
        {
            if (this.startPointTransform)
            {
                Destroy(this.startPointTransform.gameObject);
            }
            if (this.endPointTransform)
            {
                Destroy(this.endPointTransform.gameObject);
            }
        }
        private void OnEnable()
        {
            InstanceTracker.Add<BeamController>(this);
        }
        private void OnDisable()
        {
            InstanceTracker.Remove<BeamController>(this);
        }
        private void LateUpdate()
        {
            this.UpdateBeamVisuals();
        }
        private void OnSyncTarget(SS2HurtBoxReference newValue)
        {
            this.netTarget = newValue;
            this.UpdateCachedHurtBox();
        }
        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                this.FixedUpdateServer();
            }
        }
        private void FixedUpdateServer()
        {
            if (!this.cachedHurtBox)
            {
                this.BreakServer();
                return;
            }
            if (this.tickInterval > 0f)
            {
                this.stopwatchServer += Time.fixedDeltaTime;
                while (this.stopwatchServer >= this.tickInterval)
                {
                    this.stopwatchServer -= this.tickInterval;
                    this.OnTickServer();
                }
            }
        }
        private void OnTickServer()
        {
            if (!this.cachedHurtBox || !this.cachedHurtBox.healthComponent)
            {
                this.BreakServer();
                return;
            }
            onTickServer?.Invoke(this);
        }
        private void UpdateCachedHurtBox()
        {
            if (!this.previousHurtBoxReference.Equals(this.netTarget))
            {
                this.cachedHurtBox = this.netTarget.ResolveHurtBox();
                this.previousHurtBoxReference = this.netTarget;
            }
        }
        public static bool BeamAlreadyExists(GameObject owner, HurtBox target)
        {
            return BeamController.BeamAlreadyExists(owner, target.healthComponent);
        }
        public static bool BeamAlreadyExists(GameObject owner, HealthComponent targetHealthComponent)
        {
            List<BeamController> instancesList = InstanceTracker.GetInstancesList<BeamController>();
            int i = 0;
            int count = instancesList.Count;
            while (i < count)
            {
                BeamController BeamController = instancesList[i];
                HurtBox target = BeamController.target;
                if (((target != null) ? target.healthComponent : null) == targetHealthComponent && BeamController.ownership.ownerObject == owner)
                {
                    return true;
                }
                i++;
            }
            return false;
        }
        public static int GetBeamCountForOwner(GameObject owner)
        {
            int num = 0;
            List<BeamController> instancesList = InstanceTracker.GetInstancesList<BeamController>();
            int i = 0;
            int count = instancesList.Count;
            while (i < count)
            {
                if (instancesList[i].ownership.ownerObject == owner)
                {
                    num++;
                }
                i++;
            }
            return num;
        }
        private void UpdateBeamVisuals()
        {
            float target = this.target ? 1f : 0f;
            this.scaleFactor = Mathf.SmoothDamp(this.scaleFactor, target, ref this.scaleFactorVelocity, this.smoothTime);
            Vector3 localScale = new Vector3(this.scaleFactor, this.scaleFactor, this.scaleFactor);
            this.startPointTransform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
            this.startPointTransform.localScale = localScale;
            if (this.cachedHurtBox)
            {
                this.endPointTransform.position = this.cachedHurtBox.healthComponent.body.corePosition;
            }
            this.endPointTransform.localScale = localScale;
            this.lineRenderer.widthMultiplier = this.scaleFactor * this.maxLineWidth;
        }
        [Server]
        public void BreakServer()
        {
            if (this.broken)
            {
                return;
            }
            this.broken = true;
            this.target = null;
            base.transform.SetParent(null);
            this.ownership.ownerObject = null;
            Destroy(base.gameObject, this.lingerAfterBrokenDuration);
        }
    }
}
