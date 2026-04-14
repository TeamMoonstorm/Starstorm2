using RoR2;
using SS2.Components;
using UnityEngine;
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
        public static GameObject laserPrefab;

        private NemToolbotController controller;
        private GameObject laserInstance;
        private Transform muzzleTransform;
        private float hitDistance;
        private NemToolbotController.WeaponType pendingWeapon;

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
            if (laserPrefab != null && muzzleTransform != null)
            {
                laserInstance = Object.Instantiate(laserPrefab, muzzleTransform.position, muzzleTransform.rotation);
            }
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

            UpdateLaserVisuals(aimRay.origin, endPoint);

            if (isAuthority && !IsKeyDownAuthority())
            {
                if (NetworkServer.active && controller != null)
                {
                    controller.SetWeapon(pendingWeapon);
                }
                outer.SetNextStateToMain();
            }
        }

        private void UpdateLaserVisuals(Vector3 origin, Vector3 endPoint)
        {
            if (laserInstance != null)
            {
                laserInstance.transform.position = origin;
                laserInstance.transform.rotation = Quaternion.LookRotation(endPoint - origin);
                laserInstance.transform.localScale = new Vector3(1f, 1f, Vector3.Distance(origin, endPoint));
            }
        }

        public override void OnExit()
        {
            Util.PlaySound(exitSoundString, gameObject);
            if (laserInstance != null)
            {
                EntityState.Destroy(laserInstance);
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
