using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2.Equipments
{
    public sealed class BackThruster : SS2Equipment, IContentPackModifier
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acBackThruster", SS2Bundle.Equipments);

        private const string token = "SS2_EQUIP_BACKTHRUSTER_DESC";

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "How long the Thruster buff lasts, in seconds.")]
        [FormatToken(token)]
        public static float thrustDuration = 8f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Maximum speed bonus from Thruster (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float speedCap = 2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "How long it takes to reach maximum speed, in seconds")]
        public static float accel = 1.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Maximum turning angle before losing built up speed")]
        public static float maxAngle = 15f;

        private static Dictionary<CameraRigController, ParticleSystem> camerasToParticles = new Dictionary<CameraRigController, ParticleSystem>();
        private static GameObject cameraParticlesPrefab;

        public override bool Execute(EquipmentSlot slot)
        {
            var characterMotor = slot.characterBody.characterMotor;
            if (characterMotor)
            {
                slot.characterBody.AddTimedBuff(SS2Content.Buffs.BuffBackThruster, thrustDuration);
                return true;
            }
            return false;
        }

        public override void Initialize()
        {
            cameraParticlesPrefab = SS2Assets.LoadAsset<GameObject>("BackThrusterCameraParticles", SS2Bundle.Equipments);

            CameraRigController.onCameraEnableGlobal += CameraRigController_onCameraEnableGlobal;
            CameraRigController.onCameraDisableGlobal += CameraRigController_onCameraDisableGlobal;

            On.RoR2.CameraRigController.LateUpdate += CameraRigController_LateUpdate;

            // idk what this really means, but vanilla sprint particles do this and ours dont work without it
            SceneCamera.onSceneCameraPreCull += (sceneCam) =>
            {
                if (camerasToParticles.TryGetValue(sceneCam.cameraRigController, out ParticleSystem particleSystem))
                {
                    particleSystem.gameObject.layer = LayerIndex.defaultLayer.intVal;
                }
            };
            SceneCamera.onSceneCameraPostRender += (sceneCam) =>
            {
                if (camerasToParticles.TryGetValue(sceneCam.cameraRigController, out ParticleSystem particleSystem))
                {
                    particleSystem.gameObject.layer = LayerIndex.noDraw.intVal;
                }
            };

        }

        private void CameraRigController_onCameraDisableGlobal(CameraRigController cameraRigController)
        {
            if (!cameraParticlesPrefab)
            {
                return;
            }
            if (camerasToParticles.ContainsKey(cameraRigController))
            {
                camerasToParticles.Remove(cameraRigController);
            }
        }

        private void CameraRigController_onCameraEnableGlobal(CameraRigController cameraRigController)
        {
            if (!cameraParticlesPrefab)
            {
                return;
            }
            if (camerasToParticles.ContainsKey(cameraRigController) == false)
            {
                var particleSystem = GameObject.Instantiate(cameraParticlesPrefab, cameraRigController.sceneCam.transform).GetComponent<ParticleSystem>();
                particleSystem.transform.localPosition = cameraParticlesPrefab.transform.position;
                particleSystem.transform.localRotation = cameraParticlesPrefab.transform.rotation;
                camerasToParticles.Add(cameraRigController, particleSystem);
            }
        }

        private void CameraRigController_LateUpdate(On.RoR2.CameraRigController.orig_LateUpdate orig, CameraRigController self)
        {
            orig(self);

            bool showParticles = self.targetBody && self.targetBody.HasBuff(SS2Content.Buffs.BuffBackThruster);
            if (camerasToParticles.TryGetValue(self, out ParticleSystem particleSystem))
            {
                self.SetParticleSystemActive(showParticles, particleSystem);
            }
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public sealed class BackThrusterBuffBehaviour : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffBackThruster;

            private float stopwatch;
            private float thrust;
            private float watchInterval = 0.15f;
            private float moveAngle;
            private float lastAngle;
            private float accelCoeff = Equipments.BackThruster.speedCap / (Equipments.BackThruster.accel + 0.00001f);   //Fuck people who put 0 in configs
            private float cutoff = maxAngle * Mathf.Deg2Rad;

            private void FixedUpdate()
            {
                if (!hasAnyStacks) return;

                stopwatch += Time.fixedDeltaTime;
                if (stopwatch > watchInterval)
                {
                    stopwatch -= watchInterval;
                    moveAngle = Mathf.Atan2(characterBody.characterMotor.velocity.x, characterBody.characterMotor.velocity.z) + Mathf.PI;
                    if (characterBody.notMovingStopwatch < 0.1f && CheckAngle())
                        thrust = Mathf.Min(thrust + (accelCoeff * watchInterval), Equipments.BackThruster.speedCap);
                    else
                        thrust = Mathf.Max(thrust - (accelCoeff * watchInterval * 3), 0f);
                    characterBody.RecalculateStats();
                    lastAngle = moveAngle;
                }
            }
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedMultAdd += thrust;
            }
            protected override void OnAllStacksLost()
            {
                thrust = 0f;
                lastAngle = 0f;
                characterBody.RecalculateStats();
            }

            private bool CheckAngle()
            {
                float delta = Mathf.Abs(moveAngle - lastAngle);
                if (delta <= cutoff || delta > (2 * Mathf.PI - cutoff))
                    return true;
                return false;
            }
        }
    }
}
