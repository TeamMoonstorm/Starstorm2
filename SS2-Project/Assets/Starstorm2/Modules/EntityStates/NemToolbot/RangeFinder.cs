using RoR2;
using SS2.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.NemToolbot
{
    /// <summary>
    /// Secondary skill for deployed form.
    /// Hold to project a laser from the muzzle. The laser raycasts and shows the
    /// range to the first surface/enemy hit, along with which weapon that range
    /// would select. On release, sets the weapon on NemToolbotController.
    /// </summary>
    public class RangeFinder : BaseSkillState
    {
        public static float maxRange = 200f;
        public static string muzzleString = "Muzzle";
        public static string enterSoundString = "";
        public static string exitSoundString = "";

        private static Material laserMaterial;

        private NemToolbotController controller;
        private GameObject laserObject;
        private LineRenderer lineRenderer;
        private Transform muzzleTransform;
        private float hitDistance;
        private NemToolbotController.WeaponType pendingWeapon;
        private Vector3 lastLaserOrigin;
        private Vector3 lastLaserEndPoint;

        public override void OnEnter()
        {
            base.OnEnter();

            if (!gameObject.TryGetComponent(out controller))
            {
                Debug.LogError("NemToolbot RangeFinder: Failed to get NemToolbotController on " + gameObject.name);
            }

            Util.PlaySound(enterSoundString, gameObject);
            characterBody.SetAimTimer(3f);

            muzzleTransform = FindModelChild(muzzleString);

            if (laserMaterial == null)
            {
                laserMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matTracerBright.mat").WaitForCompletion();
            }

            laserObject = new GameObject("NemToolbotRangeFinderLaser");
            lineRenderer = laserObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = laserMaterial;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.textureMode = LineTextureMode.Tile;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            Ray aimRay = GetAimRay();
            Vector3 endPoint;

            if (Physics.Raycast(aimRay, out RaycastHit hit, maxRange, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore))
            {
                hitDistance = hit.distance;
                endPoint = hit.point;
            }
            else
            {
                hitDistance = maxRange;
                endPoint = aimRay.GetPoint(maxRange);
            }

            pendingWeapon = NemToolbotController.GetWeaponFromRange(hitDistance);

            lastLaserOrigin = aimRay.origin;
            lastLaserEndPoint = endPoint;

            if (isAuthority && !IsKeyDownAuthority())
            {
                Debug.Log($"[NemToolbot] RangeFinder: Released at {hitDistance:F1}m -> selecting {pendingWeapon}");
                if (NetworkServer.active && controller != null)
                {
                    controller.SetWeapon(pendingWeapon);
                }
                outer.SetNextStateToMain();
            }
        }

        public override void Update()
        {
            base.Update();
            UpdateLaserVisuals(lastLaserOrigin, lastLaserEndPoint);
        }

        private void UpdateLaserVisuals(Vector3 origin, Vector3 endPoint)
        {
            if (lineRenderer != null)
            {
                Vector3 start = muzzleTransform != null ? muzzleTransform.position : origin;
                lineRenderer.SetPosition(0, start);
                lineRenderer.SetPosition(1, endPoint);
            }
        }

        public override void OnExit()
        {
            Util.PlaySound(exitSoundString, gameObject);
            if (laserObject != null)
            {
                EntityState.Destroy(laserObject);
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
