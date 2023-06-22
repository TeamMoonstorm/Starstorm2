using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moonstorm.Starstorm2.Components;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemMerc
{
    public class ShadowStep : BaseSkillState
    {
        public static float teleportYBonus = 1.25f;
        public static float baseDuration = 0.5f;
        public static float teleportBehindDistance = 5f;
        public static float teleportTime = 0.75f;
        public static float smallHopVelocity = 12f;
        public static float hologramDurationCoefficient = 0.5f;
        public static AnimationCurve cameraMovementCurve;

        private float duration;

        private Vector3 teleportTarget;

        private CameraFlipper camera;
        private GameObject target;
        private bool usedGravity;
        private bool targetIsHologram;
        public override void OnEnter()
        {
            base.OnEnter();

            //vfx
            //anim
            //sound
            // overlay material
            //need to fix teleport near walls
         
            

            NemMercTracker tracker = base.GetComponent<NemMercTracker>();
            this.target = tracker.GetTrackingTarget();

            this.targetIsHologram = this.target.GetComponent<NemMercHologram>();

            this.duration = ShadowStep.baseDuration * (this.targetIsHologram ? hologramDurationCoefficient : 1); // movespeedstat ?
            float teleportTime = this.targetIsHologram ? this.duration : this.duration * ShadowStep.teleportTime;
            float distanceBehind = targetIsHologram ? 0 : ShadowStep.teleportBehindDistance;

            Vector3 between = target.transform.position - base.transform.position;
            float distance = between.magnitude;
            Vector3 direction = between.normalized;
            
            this.teleportTarget = direction * (distance + distanceBehind) + base.transform.position;
            this.teleportTarget.y = Mathf.Max(this.target.transform.position.y + teleportYBonus, teleportTarget.y);

            this.camera = base.gameObject.AddComponent<CameraFlipper>();
            this.camera.StartCameraFlip(this.teleportTarget, target, this.duration * teleportTime);
            this.camera.movementCurve = ShadowStep.cameraMovementCurve;

            this.usedGravity = base.characterMotor.useGravity;
            base.characterMotor.useGravity = false;
            base.characterMotor.velocity = Vector3.zero;

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
        }

        private void UpdateTarget()
        {
            if(this.target)
            {
                Vector3 between = target.transform.position - base.transform.position;
                float distance = between.magnitude;
                Vector3 direction = between.normalized;
                float distanceBehind = targetIsHologram ? 0 : ShadowStep.teleportBehindDistance;
                this.teleportTarget = direction * (distance + distanceBehind) + base.transform.position;
                this.teleportTarget.y = Mathf.Max(this.target.transform.position.y + teleportYBonus, teleportTarget.y);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.UpdateTarget();

            if(base.isAuthority && base.fixedAge >= this.duration)
            {
                if(this.camera)
                    this.camera.EndCameraFlip();

                TeleportHelper.TeleportBody(base.characterBody, this.teleportTarget);
                base.characterDirection.forward = base.GetAimRay().direction;
                base.SmallHop(base.characterMotor, ShadowStep.smallHopVelocity);
                this.outer.SetNextStateToMain();
                return;
            }
        
        }
        public override void Update()
        {
            base.Update();
            if (this.camera)
                this.camera.UpdateCamera(Time.deltaTime);
        }
        public override void OnExit()
        {
            base.OnExit();
            if (this.camera)
            {      
                this.camera.EndCameraFlip();
                Destroy(this.camera);
            }

            base.characterMotor.useGravity = this.usedGravity;

            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
        }



        // no point in this being a monobehavior but i stpid
        //orbits the camera around a gameobject while looking at it
        public class CameraFlipper : MonoBehaviour, ICameraStateProvider
        {
            

            public static bool doOffset = true;
            public static float maxOffset = 12;
            public static float endLerpDuration = 0.5f;
            public static float wallCushion = 0.1f;

            public AnimationCurve movementCurve;
            public AnimationCurve fovCurve;

            private Vector3 cameraStartPosition;
            private Vector3 cameraEndPosition;
            private Vector3 cameraPivot;
            private Vector3 orbitDirection;
            private GameObject target;
            private float orbitDistance;
            public bool inOverride;
            private float overrideStopwatch;
            private float overrideDuration;

            public void UpdateCamera(float dt)
            {
                this.overrideStopwatch += dt;

                if (this.target)
                    this.cameraPivot = this.target.transform.position;
            }
            // x^2 curve
            private float EvaluateOffsetCurve(float f)
            {
                return -Mathf.Pow(f - 1, 2) + 1;
            }

            public void StartCameraFlip(Vector3 endPoint, GameObject target, float duration)
            {
                Vector3 idealLocalCameraPos = Vector3.zero;
                foreach (CameraRigController cameraRigController in CameraRigController.readOnlyInstancesList)
                {
                    if (cameraRigController.target == base.gameObject)
                    {
                        //might bug out with spectators? idk
                        this.cameraStartPosition = cameraRigController.currentCameraState.position;
                        idealLocalCameraPos = cameraRigController.targetParams.cameraParams.data.idealLocalCameraPos.value;
                        idealLocalCameraPos.y += cameraRigController.targetParams.cameraParams.data.pivotVerticalOffset.value;
                        cameraRigController.SetOverrideCam(this, 0.25f);
                    }
                    else if (cameraRigController.IsOverrideCam(this))
                    {
                        cameraRigController.SetOverrideCam(null, 0.05f);
                    }
                }
                this.inOverride = true;
                this.overrideStopwatch = 0;
                this.overrideDuration = duration;

                Vector3 between = endPoint - base.transform.position;
                Vector3 pushDirection = base.transform.right * -1f;

                this.target = target;
                this.cameraPivot = this.target.transform.position;
                this.orbitDirection = pushDirection;
                this.orbitDistance = Mathf.Min(between.magnitude / 2f, maxOffset);
                Vector3 endOffset = Util.QuaternionSafeLookRotation(between.normalized * -1) * idealLocalCameraPos;
                this.cameraEndPosition = endPoint + endOffset;

                
            }
            public void EndCameraFlip()
            {
                this.inOverride = false;
                foreach (CameraRigController cameraRigController in CameraRigController.readOnlyInstancesList)
                {
                    if (cameraRigController.target == base.gameObject)
                    {
                        cameraRigController.SetOverrideCam(null, CameraFlipper.endLerpDuration);
                    }
                    else if (cameraRigController.IsOverrideCam(this))
                    {
                        cameraRigController.SetOverrideCam(null, 0.05f);
                    }
                }
            }
            public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
            {
                float t = this.movementCurve.Evaluate(Mathf.Clamp01(overrideStopwatch / overrideDuration));
                Vector3 position = Vector3.Lerp(cameraStartPosition, cameraEndPosition, t);
                float offsetMagnitude = EvaluateOffsetCurve(t * 2) * orbitDistance;
                Vector3 offset = offsetMagnitude * orbitDirection;

                Vector3 position2 = doOffset ? position + offset : position;
                Quaternion lookDirection = Util.QuaternionSafeLookRotation((this.cameraPivot - position2).normalized);

                // wall cushion ][
                Vector3 between = position2 - this.cameraPivot;

                float distanceFromPivot = Raycast(new Ray(this.cameraPivot, between.normalized), between.magnitude, CameraFlipper.wallCushion-0.01f);
                Vector3 finalPosition = (between.normalized * distanceFromPivot) + this.cameraPivot; 

                cameraState.position = finalPosition;
                cameraState.rotation = lookDirection;
                //FOV?
            }
            public float Raycast(Ray ray, float maxDistance, float wallCushion)
            {
                LayerIndex world = LayerIndex.world;
                RaycastHit[] array = Physics.SphereCastAll(ray, wallCushion, maxDistance, world.mask, QueryTriggerInteraction.Ignore);
                float num = maxDistance;
                for (int i = 0; i < array.Length; i++)
                {
                    float distance = array[i].distance;
                    if (distance < num)
                    {
                        Collider collider = array[i].collider;
                        if (collider && !collider.GetComponent<NonSolidToCamera>())
                        {
                            num = distance;
                        }
                    }
                }
                return num;
            }

            public bool IsHudAllowed(CameraRigController cameraRigController)
            {
                return true;
            }

            public bool IsUserControlAllowed(CameraRigController cameraRigController)
            {
                return false;
            }

            public bool IsUserLookAllowed(CameraRigController cameraRigController)
            {
                return false;
            }
        }

    }
}
