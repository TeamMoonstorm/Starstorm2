using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SS2.Components;
using System.Collections.Generic;
namespace EntityStates.Cyborg2
{
    public class Teleport : BaseSkillState
    {
        public static float exitHopVelocity = 12f;
        public static float baseDuration = 0.33f;
        private float duration;
        private CameraMover camera;
        private TeleporterProjectile.ProjectileTeleporterOwnership teleporterOwnership;
        private Vector3 teleportTarget;

        public static float exitVelocityCoefficient = 400f;
        private static float damageRadius = 10f;
        private static float damageCoefficient = 3f;
        private Vector3 storedVelocity;
        public override void OnEnter()
        {
            base.OnEnter();

            this.teleporterOwnership = base.GetComponent<TeleporterProjectile.ProjectileTeleporterOwnership>();
            if(!this.teleporterOwnership)
            {
                Debug.LogError("Cyborg2 teleport failed! No teleporter component. (howw?????)");
                this.outer.SetNextStateToMain();
                return;
            }

            this.teleportTarget = this.teleporterOwnership.teleporter.GetSafeTeleportPosition();

            this.duration = baseDuration; //movespeed? attackspeed?

            this.camera = base.gameObject.AddComponent<CameraMover>();
            this.camera.lerpTime = this.duration;
            this.camera.position = this.teleportTarget;

            this.storedVelocity = base.characterMotor.velocity;

            if (NetworkServer.active)
            {
                TeleporterOrb teleporterOrb = new TeleporterOrb();
                teleporterOrb.totalDuration = this.duration;
                teleporterOrb.attacker = base.gameObject;
                teleporterOrb.inflictor = base.gameObject;
                teleporterOrb.damageValue = characterBody.damage * 3f;
                teleporterOrb.isCrit = base.RollCrit();
                teleporterOrb.targetObjects = GatherHits(base.teamComponent.teamIndex);
                RoR2.Orbs.OrbManager.instance.AddOrb(teleporterOrb);

                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
        }

        private void UpdateTarget()
        {
            if(this.teleporterOwnership && this.teleporterOwnership.teleporter)
            {
                this.teleportTarget = this.teleporterOwnership.teleporter.GetSafeTeleportPosition();
                if(this.camera)
                    this.camera.position = this.teleportTarget;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.UpdateTarget();

            base.characterMotor.velocity = Vector3.zero;

            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                //for some reason its 0,0,0 on the same frame the teleporter disappears
                if(this.teleportTarget != Vector3.zero)
                {
                    TeleportHelper.TeleportBody(base.characterBody, this.teleportTarget);
                    base.characterDirection.forward = base.GetAimRay().direction;
                    //Vector3 velocity = Vector3.zero;
                    //if(this.teleporterOwnership && this.teleporterOwnership.teleporter)
                    //{
                    //    velocity = this.teleporterOwnership.teleporter.GetComponent<Rigidbody>().velocity;
                    //}
                    //velocity *= Teleport.exitVelocityCoefficient;
                    //base.characterMotor.ApplyForce(velocity);

                    base.SmallHop(base.characterMotor, Teleport.exitHopVelocity);
                }
                
                
                
                this.outer.SetNextStateToMain();
                return;
            }

        }

        public override void OnExit()
        {
            base.OnExit();
            if(this.teleporterOwnership)
            {
                this.teleporterOwnership.DoTeleport();
            }
            if (this.camera)
            {
                Destroy(this.camera);
            }
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
        }

        private List<HealthComponent> GatherHits(TeamIndex teamIndex)
        {
            Vector3 between = this.teleportTarget - base.characterBody.corePosition;
            RaycastHit[] array2 = Physics.SphereCastAll(characterBody.corePosition, damageRadius, between.normalized, between.magnitude, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
            List<HealthComponent> targetList = new List<HealthComponent>();
            for (int j = 0; j < array2.Length; j++)
            {
                HurtBox hurtBox = array2[j].collider.GetComponent<HurtBox>();
                if (!hurtBox) continue;
                HealthComponent healthComponent = hurtBox.healthComponent;
                if (healthComponent && !targetList.Contains(healthComponent) && FriendlyFireManager.ShouldSeekingProceed(healthComponent, teamIndex))
                {
                    targetList.Add(healthComponent);
                }
            }
            return targetList;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        //move camera to position. keep player camera control
        public class CameraMover : MonoBehaviour, ICameraStateProvider
        {
            public static float endLerpTime = 0.2f;
            public float lerpTime;
            public Vector3 position;

            private void OnEnable()
            {
                foreach (CameraRigController cameraRigController in CameraRigController.readOnlyInstancesList)
                {
                    if (cameraRigController.target == base.gameObject)
                    {
                        cameraRigController.SetOverrideCam(this, this.lerpTime);
                    }
                    else if (cameraRigController.IsOverrideCam(this))
                    {
                        cameraRigController.SetOverrideCam(null, 0.05f);
                    }
                }
            }
            private void OnDisable()
            {
                foreach (CameraRigController cameraRigController in CameraRigController.readOnlyInstancesList)
                {
                    if (cameraRigController.target == base.gameObject)
                    {
                        cameraRigController.SetOverrideCam(null, endLerpTime);
                    }
                    else if (cameraRigController.IsOverrideCam(this))
                    {
                        cameraRigController.SetOverrideCam(null, 0.05f);
                    }
                }
            }
            public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
            {
                Vector3 position = this.position;
                Vector3 cameraLocalPos = cameraRigController.targetParams.currentCameraParamsData.idealLocalCameraPos.value;
                cameraLocalPos.y += cameraRigController.targetParams.currentCameraParamsData.pivotVerticalOffset.value + 0.9f;
                cameraLocalPos += (cameraRigController.targetParams.cameraPivotTransform ? cameraRigController.targetParams.cameraPivotTransform.localPosition : Vector3.zero);
                position += cameraState.rotation * cameraLocalPos;
                cameraState.position = position;
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
