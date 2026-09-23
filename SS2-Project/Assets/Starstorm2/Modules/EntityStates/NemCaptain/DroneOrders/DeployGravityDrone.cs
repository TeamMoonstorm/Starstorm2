using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemCaptain.Weapon
{
    public class DeployGravityDrone : BaseDroneStrike
    {
        [SerializeField]
        public GameObject dronePrefab;

        public override void OnOrderEffect()
        {
            base.OnOrderEffect();
            Ray aimRay = GetAimRay();
            RaycastHit raycastHit;
            Physics.Raycast(aimRay, out raycastHit, maxDistance, LayerIndex.CommonMasks.bullet);

            GameObject droneObject = Object.Instantiate(dronePrefab, raycastHit.point, Quaternion.identity);
            droneObject.GetComponent<TeamFilter>().teamIndex = teamComponent.teamIndex;
            droneObject.GetComponent<GenericOwnership>().ownerObject = gameObject;

            NetworkServer.Spawn(droneObject);
        }
    }
}
