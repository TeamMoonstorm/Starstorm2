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
    public class HologramShadowStep : BaseSkillState
    {
        public static float baseDuration = 0.25f;
        public static float smallHopVelocity = 12f;
        public static float cameraLerpTime = 0.5f;

        

        private float duration;
        private Vector3 teleportTarget;
        public GameObject target;
        private Vector3 startPosition;

        private CameraLooker camera;

        public override void OnEnter()
        {
            base.OnEnter();

            //vfx
            //anim
            //sound
            // overlay material

            GameObject cameraTarget = null;

            if (!this.target)
            {
                NemMercTracker tracker = base.GetComponent<NemMercTracker>();
                this.target = tracker.GetTrackingTarget();                
            }
          
            if(this.target)
            {
                NemMercHologram hologram = this.target.GetComponent<NemMercHologram>();
                if(hologram && hologram.target)
                {
                    cameraTarget = hologram.target.gameObject;
                }    
            }
            else
            {
                this.outer.SetNextStateToMain();
                return;
            }


            this.duration = HologramShadowStep.baseDuration;

            this.teleportTarget = this.target.transform.position;

            base.characterMotor.velocity = Vector3.zero;

            this.camera = base.gameObject.AddComponent<CameraLooker>();
            this.camera.position = this.teleportTarget;
            this.camera.initialLookDirection = base.GetAimRay().direction;
            this.camera.target = cameraTarget;
            this.camera.startLerpDuration = this.duration * cameraLerpTime;
            this.camera.StartLook();

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
        }

        private void UpdateTarget()
        {
            if (this.target)
            {
                Vector3 between = target.transform.position - base.transform.position;
                float distance = between.magnitude;
                Vector3 direction = between.normalized;
                this.teleportTarget = direction * distance + base.transform.position;
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.UpdateTarget();

            base.characterMotor.velocity = Vector3.zero;

            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                TeleportHelper.TeleportBody(base.characterBody, this.teleportTarget);
                base.characterDirection.forward = base.GetAimRay().direction;
                //base.skillLocator.primary.stock = 2; /////////////////////
                this.outer.SetNextStateToMain();
                return;
            }

        }
        public override void OnExit()
        {
            base.OnExit();


            if (this.camera)
            {
                this.camera.StopLook();
                Destroy(this.camera);
            }
                

            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
        }


        // look at GameObject
        public class CameraLooker : MonoBehaviour, ICameraStateProvider
        {
            public Vector3 position;
            public Vector3 initialLookDirection;
            public GameObject target;
            public float startLerpDuration;
            public static float endLerpDuration = 0.4f;
            public void StartLook()
            {
                foreach (CameraRigController cameraRigController in CameraRigController.readOnlyInstancesList)
                {
                    if (base.gameObject && cameraRigController.target == base.gameObject)
                    {
                        cameraRigController.SetOverrideCam(this, this.startLerpDuration);
                    }
                    else if (cameraRigController.IsOverrideCam(this))
                    {
                        cameraRigController.SetOverrideCam(null, 0.05f);
                    }
                }
            }
            public void StopLook()
            {
                foreach (CameraRigController cameraRigController in CameraRigController.readOnlyInstancesList)
                {
                    if (base.gameObject && cameraRigController.target == base.gameObject)
                    {
                        cameraRigController.SetOverrideCam(null, endLerpDuration);
                    }
                    else if (cameraRigController.IsOverrideCam(this))
                    {
                        cameraRigController.SetOverrideCam(null, 0.05f);
                    }
                }
            }
            public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
            {

                Vector3 cameraLocalPos = cameraRigController.targetParams.currentCameraParamsData.idealLocalCameraPos.value;
                cameraLocalPos.y += cameraRigController.targetParams.currentCameraParamsData.pivotVerticalOffset.value + ShadowStep.CameraFlipper.shitNumber;
                cameraLocalPos += (cameraRigController.targetParams.cameraPivotTransform ? cameraRigController.targetParams.cameraPivotTransform.localPosition : Vector3.zero);

                if (this.target)
                {
                    Vector3 target = this.target.transform.position;
                    Vector3 between = target - this.position;

                    Quaternion rotation = Util.QuaternionSafeLookRotation(between.normalized);
                    cameraState.rotation = rotation;

                    cameraState.position = this.position + (rotation * cameraLocalPos);
                }
                else
                {
                    Quaternion rotation = Util.QuaternionSafeLookRotation(this.initialLookDirection);
                    cameraState.rotation = rotation;
                    cameraState.position = this.position + (rotation * cameraLocalPos);
                }
                
            }

            private void OnDestroy()
            {
                this.StopLook();
            }
            public bool IsHudAllowed(CameraRigController cameraRigController)
            {
                return true;
            }

            public bool IsUserControlAllowed(CameraRigController cameraRigController)
            {
                return true;
            }

            public bool IsUserLookAllowed(CameraRigController cameraRigController)
            {
                return false;
            }
        }
    }
}
