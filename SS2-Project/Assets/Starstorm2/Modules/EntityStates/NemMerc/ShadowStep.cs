using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moonstorm.Starstorm2.Components;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Moonstorm.Starstorm2;
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

        public static GameObject blinkPrefab;
        public static GameObject blinkDestinationPrefab;
        public static GameObject blinkArrivalPrefab;

        private GameObject destinationInstance;
        private float duration;

        private Vector3 teleportStartPosition;
        private Vector3 teleportTarget;

        private CameraFlipper camera;
        private GameObject target;
        private CharacterModel characterModel;
        private Transform modelTransform;

        private Vector3 lastKnownTargetPosition;
        private Vector3 targetSafe;
        private GameObject bossEffect;

        [NonSerialized]
        public static bool FUCKERTESTINGXDDDDDDDDDDDDDDDDDDDDD = true;
        public override void OnEnter()
        {
            base.OnEnter();

            this.duration = ShadowStep.baseDuration; //movespeedstat ??

            NemMercTracker tracker = base.GetComponent<NemMercTracker>();
            this.target = tracker.GetTrackingTarget();
            if (!this.target)
            {
                this.outer.SetNextStateToMain();
                this.activatorSkillSlot.AddOneStock();
                return;
            }

            base.StartAimMode();
            base.PlayAnimation("FullBody, Override", "ShadowStep", "Utility.playbackRate", baseDuration);
            Util.PlaySound("Play_nemmerc_utility_enter", base.gameObject);

            

            this.teleportStartPosition = base.transform.position;
            this.UpdateTarget();
            this.lastKnownTargetPosition = this.target.transform.position;
            base.characterDirection.forward = this.teleportTarget - base.transform.position;

            this.camera = base.gameObject.AddComponent<CameraFlipper>();
            this.camera.StartCameraFlip(this.teleportTarget, target, this.duration * ShadowStep.teleportTime);
            this.camera.movementCurve = ShadowStep.cameraMovementCurve;
                          
            base.characterMotor.velocity = Vector3.zero;

            this.modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                this.characterModel = modelTransform.GetComponent<CharacterModel>();
                ChildLocator childLocator = base.GetModelChildLocator();
                if(childLocator)
                {
                    Transform chest = childLocator.FindChild("Chest");
                    if(chest)
                    {
                        Transform bossEffect = chest.Find("NemMercenaryBossEffect(Clone)");
                        if (bossEffect)
                        {
                            this.bossEffect = bossEffect.gameObject;
                            this.bossEffect.SetActive(false);
                        }
                    }
                }
                
            }
            if (this.characterModel)
            {
                this.characterModel.invisibilityCount++;
            }
            if(ShadowStep.blinkPrefab)
            {
                EffectManager.SimpleEffect(blinkPrefab, base.characterBody.corePosition, Util.QuaternionSafeLookRotation(teleportTarget - teleportStartPosition), false);
            }
            if (ShadowStep.blinkDestinationPrefab)
            {
                this.destinationInstance = UnityEngine.Object.Instantiate<GameObject>(ShadowStep.blinkDestinationPrefab, this.teleportTarget, Quaternion.identity);
            }

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
        }

        private void UpdateTarget()
        {
            if (this.target)
            {
                this.lastKnownTargetPosition = this.target.transform.position;
            }
            Vector3 between = this.lastKnownTargetPosition - this.teleportStartPosition;
            float distance = between.magnitude;
            Vector3 direction = between.normalized;
            this.teleportTarget = direction * (distance + ShadowStep.teleportBehindDistance) + base.transform.position;
            if (Physics.Raycast(this.lastKnownTargetPosition, direction, out RaycastHit hit, ShadowStep.teleportBehindDistance, LayerIndex.world.mask))
            {
                this.teleportTarget = hit.point;
            }
            this.teleportTarget.y = Mathf.Max(this.lastKnownTargetPosition.y + teleportYBonus, teleportTarget.y);
            if (this.camera)
                this.camera.cameraEndPosition = this.teleportTarget;
            if (this.destinationInstance)
            {
                this.destinationInstance.transform.position = this.teleportTarget;
            }
        }


        bool s;

        [NonSerialized] // WHY DOESNT NONSERIALIZED WORK ALL OF A SUDDEN ???????????????????????????????????????
        public static float bitch = 0.66f; // I DONT CARE I DONCA RE STFU STFU STFU


        private void FixedUpdateTarget()
        {
            if(this.target)
            {
                this.targetSafe = this.teleportTarget;
            }

        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.FixedUpdateTarget();
            base.characterMotor.velocity = Vector3.zero;

            if (base.fixedAge >= this.duration * bitch && !s) // XDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD
            {
                s = true;
                Util.PlaySound("Play_nemmerc_utility_exit", base.gameObject);
            }

            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                if(this.camera)
                    this.camera.EndCameraFlip();


                if (this.targetSafe.magnitude < 5)
                {
                    SS2Log.Error("SHADOWSTEP WENT TO 0,0");
                    SS2Log.Error("TeleportTarget: " + this.teleportTarget);
                    SS2Log.Error("targetSafe: " + this.targetSafe);
                    SS2Log.Error("target: " + this.target);
                    SS2Log.Error("lastKnownTargetPosition: " + this.lastKnownTargetPosition);
                    SS2Log.Error("NemMercTrackerComponent target: " + base.GetComponent<NemMercTracker>().GetTrackingTarget());
                }


                TeleportHelper.TeleportBody(base.characterBody, this.targetSafe);

                if(blinkArrivalPrefab)
                {
                    EffectManager.SimpleEffect(blinkArrivalPrefab, this.targetSafe, Quaternion.identity, true);
                }

                base.characterDirection.forward = this.lastKnownTargetPosition - base.transform.position;
                base.SmallHop(base.characterMotor, ShadowStep.smallHopVelocity);
                base.StartAimMode();

                this.outer.SetNextStateToMain();
                return;
            }
        
        }
        public override void Update()
        {
            base.Update();


            this.UpdateTarget();
            

            if (this.camera)
                this.camera.UpdateCamera(Time.deltaTime);
        }
        public override void OnExit()
        {
            base.OnExit();
            if(this.bossEffect)
            {
                this.bossEffect.SetActive(true);
            }
            if (this.characterModel)
            {
                this.characterModel.invisibilityCount--;
            }
            if (this.camera)
            {      
                this.camera.EndCameraFlip();
                Destroy(this.camera);
            }

            if (this.modelTransform)
            {
                TemporaryOverlay temporaryOverlay2 = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay2.duration = 0.67f;
                temporaryOverlay2.animateShaderAlpha = true;
                temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay2.destroyComponentOnEnd = true;
                temporaryOverlay2.originalMaterial = SS2Assets.LoadAsset<Material>("matNemergize", SS2Bundle.NemMercenary);
                temporaryOverlay2.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
            }

            base.characterMotor.useGravity = true;

            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
        }



        // no point in this being a monobehavior but i stpid
        //orbits the camera around a gameobject while looking at it
        //continues to look at the object for short period after player regains control
        public class CameraFlipper : MonoBehaviour, ICameraStateProvider
        {
            public static bool userLook = false;
            public static bool userControl = false;
            public static bool doOffset = true;
            public static float maxOffset = 12;
            public static float endLerpDuration = 0.5f;
            public static float wallCushion = 0.1f;
            public static float shitNumber = 0.9f;

            public AnimationCurve movementCurve;
            public AnimationCurve fovCurve;

            private Vector3 cameraStartPosition;
            public Vector3 cameraEndPosition;

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
                foreach (CameraRigController cameraRigController in CameraRigController.readOnlyInstancesList)
                {
                    if (cameraRigController.target == base.gameObject)
                    {
                        //might bug out with spectators? idk
                        this.cameraStartPosition = cameraRigController.currentCameraState.position;
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
                Vector3 f = new Vector3(between.x, 0, between.z).normalized;
                Vector3 pushDirection = Quaternion.Euler(0, -90, 0) * f;

                this.target = target;
                this.cameraPivot = this.target.transform.position;
                this.orbitDirection = pushDirection;
                
                this.orbitDistance = Mathf.Min(between.magnitude / 2f, maxOffset);
                this.cameraEndPosition = endPoint;
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

            //this is so fucking ugly
            public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
            {
                float t = this.movementCurve.Evaluate(Mathf.Clamp01(overrideStopwatch / overrideDuration));
                Vector3 target = this.target ? this.target.transform.position : this.cameraPivot;
                float offsetMagnitude = EvaluateOffsetCurve(t * 2) * orbitDistance;
                Vector3 orbitOffset = offsetMagnitude * orbitDirection;
                Vector3 cameraPosition = Vector3.Lerp(cameraStartPosition, cameraEndPosition, t) + orbitOffset;
                Vector3 direction = target - cameraPosition;

                Vector3 cameraLocalPos = cameraRigController.targetParams.currentCameraParamsData.idealLocalCameraPos.value;
                cameraLocalPos.y += cameraRigController.targetParams.currentCameraParamsData.pivotVerticalOffset.value + shitNumber;
                cameraLocalPos += (cameraRigController.targetParams.cameraPivotTransform ? cameraRigController.targetParams.cameraPivotTransform.localPosition : Vector3.zero);
                cameraPosition += Util.QuaternionSafeLookRotation(direction.normalized) * cameraLocalPos;

                Quaternion lookDirection = Util.QuaternionSafeLookRotation((target - cameraPosition).normalized);
                Vector3 between = cameraPosition - target;
                float distanceFromPivot = Raycast(new Ray(target, between.normalized), between.magnitude, CameraFlipper.wallCushion-0.01f);
                Vector3 finalPosition = (between.normalized * distanceFromPivot) + target;

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
            public float GetPitchFromLookVector(Vector3 lookVector)
            {
                float x = Mathf.Sqrt(lookVector.x * lookVector.x + lookVector.z * lookVector.z);
                return Mathf.Atan2(-lookVector.y, x) * 57.29578f;
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
                return true;
            }
        }

    }
}
