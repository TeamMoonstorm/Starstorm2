using RoR2;
using RoR2.UI;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using SS2.Components;

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
        private AimAnimator modelAimAnimator;
        private GameObject effectMuzzleInstance;
        private Animator modelAnimator;
        private float timerSinceComplete;

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

            ncc.SetOrderOverrides();
            
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
            if (isAuthority && !inputBank.skill2.down)
            {
                outer.SetNextStateToMain();
                timerSinceComplete += Time.fixedDeltaTime;
                if (timerSinceComplete >= exitDuration)
                {
                    
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
            ncc.UnsetOrderOverrides();
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
