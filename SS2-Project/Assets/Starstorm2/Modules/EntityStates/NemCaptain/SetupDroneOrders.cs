using System.Collections;
using System.Collections.Generic;
using System;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;
using Moonstorm.Starstorm2.Components;

namespace EntityStates.NemCaptain.Weapon
{
    public class SetupDroneOrders : BaseState
    {
        public static GameObject crosshairOverridePrefab;
        public static string enterSoundString;
        public static string exitSoundString;
        public static GameObject effectMuzzlePrefab;
        public static string effectMuzzleString;
        public static float baseExitDuration;
        public static float maxPlacementDistance;
        public static GameObject blueprintPrefab;
        public static float normalYThreshold;

        public PlacementInfo currentPlacementInfo;

        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;
        private GenericSkill primarySkillSlot;
        private AimAnimator modelAimAnimator;
        private GameObject effectMuzzleInstance;
        private Animator modelAnimator;
        private float timerSinceComplete;
        private bool beginExit;
        private GenericSkill originalPrimarySkill;
        private GenericSkill originalSecondarySkill;
        private GenericSkill originalUtilitySkill;
        private GenericSkill originalSpecialSkill;
        private BlueprintController blueprints;
        private CameraTargetParams.AimRequest aimRequest;
        private NemCaptainController ncc;

        public struct PlacementInfo
        {
            public bool ok;
            public Vector3 position;
            public Quaternion rotation;
            public void Serialize(NetworkWriter writer)
            {
                writer.Write(ok);
                writer.Write(position);
                writer.Write(rotation);
            }

            public void Deserialize(NetworkReader reader)
            {
                ok = reader.ReadBoolean();
                position = reader.ReadVector3();
                rotation = reader.ReadQuaternion();
            }
        }

        private float exitDuration
        {
            get
            {
                return baseExitDuration / attackSpeedStat;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            modelAnimator = GetModelAnimator();
            //play animations
            //vfx, muzzleflash
            ncc = characterBody.GetComponent<NemCaptainController>();

            Debug.Log("setting drone orders...");

            if (crosshairOverridePrefab)
            {
                //crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
            }

            Util.PlaySound(enterSoundString, gameObject);

            //blueprints = UnityEngine.Object.Instantiate<GameObject>(blueprintPrefab, currentPlacementInfo.position, currentPlacementInfo.rotation).GetComponent<BlueprintController>();

            if (cameraTargetParams)
                aimRequest = cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);

            //store original skills
            originalPrimarySkill = skillLocator.primary;
            Debug.Log("originalPrimarySkill : " + skillLocator.primary.name);
            originalSecondarySkill = skillLocator.secondary;
            Debug.Log("originalSecondarySkill : " + skillLocator.secondary.name);
            originalUtilitySkill = skillLocator.utility;
            Debug.Log("originalUtilitySkill : " + skillLocator.utility.name);
            originalSpecialSkill = skillLocator.special;
            Debug.Log("originalSpecialSkill : " + skillLocator.special.name);

            ncc.cachedPrimary = skillLocator.primary.skillDef;
            Debug.Log("cachedPrimaryDef : " + skillLocator.primary.skillDef.skillNameToken);

            Debug.Log("stored original skills");

            //set to orders
            skillLocator.primary = ncc.hand1;
            skillLocator.secondary = ncc.hand2;
            skillLocator.utility = ncc.hand3;
            skillLocator.special = ncc.hand4;

            Debug.Log("hand1 : " + ncc.hand1.name);
            Debug.Log("hand2 : " + ncc.hand2.name);
            Debug.Log("hand3 : " + ncc.hand3.name);
            Debug.Log("hand4 : " + ncc.hand4.name);

            skillLocator.primary.stock = 0;
            skillLocator.secondary.stock = 0;
            skillLocator.utility.stock = 0;
            skillLocator.special.stock = 0;
        }

        public static PlacementInfo GetPlacementInfo(Ray aimRay, GameObject gameObject)
        {
            float num = 0f;
            CameraRigController.ModifyAimRayIfApplicable(aimRay, gameObject, out num);
            Vector3 vector = -aimRay.direction;
            Vector3 vector2 = Vector3.up;
            Vector3 lhs = Vector3.Cross(vector2, vector);
            PlacementInfo result = default(PlacementInfo);
            result.ok = false;
            RaycastHit raycastHit;
            if (Physics.Raycast(aimRay, out raycastHit, maxPlacementDistance, LayerIndex.world.mask) && raycastHit.normal.y > normalYThreshold)
            {
                vector2 = raycastHit.normal;
                vector = Vector3.Cross(lhs, vector2);
                result.ok = true;
            }
            result.rotation = Util.QuaternionSafeLookRotation(vector, vector2);
            Vector3 point = raycastHit.point;
            result.position = point;
            return result;
        }

        public override void Update()
        {
            base.Update();
            currentPlacementInfo = GetPlacementInfo(GetAimRay(), gameObject);
            if (blueprints)
            {
                blueprints.PushState(currentPlacementInfo.position, currentPlacementInfo.rotation, currentPlacementInfo.ok);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (characterDirection)
            {
                characterDirection.moveVector = GetAimRay().direction;
            }
            if (isAuthority && beginExit)
            {
                timerSinceComplete += Time.fixedDeltaTime;
                if (timerSinceComplete >= exitDuration)
                {
                    outer.SetNextStateToMain();
                }
            }
        }

        public override void OnExit()
        {
            Debug.Log("exiting drone orders!");
            if (outer.destroying)
            {
                Util.PlaySound(exitSoundString, gameObject);
            }
            //destroy muzzle instance
            CrosshairUtils.OverrideRequest overrideRequest = crosshairOverrideRequest;
            if (overrideRequest != null)
            {
                overrideRequest.Dispose();
            }
            skillLocator.primary = originalPrimarySkill;
            skillLocator.secondary = originalSecondarySkill;
            skillLocator.utility = originalUtilitySkill;
            skillLocator.special = originalSpecialSkill;
            Debug.Log("reset skills");
            if (blueprints)
            {
                Destroy(blueprints.gameObject);
                blueprints = null;
            }
            CameraTargetParams.AimRequest aimRequest = this.aimRequest;
            if (aimRequest != null)
            {
                aimRequest.Dispose();
            }
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
