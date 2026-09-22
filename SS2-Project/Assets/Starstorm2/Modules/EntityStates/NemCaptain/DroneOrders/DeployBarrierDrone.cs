using SS2.Components;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemCaptain.Weapon
{
    public class DeployBarrierDrone : BaseBuffOrder
    {
        [SerializeField]
        public GameObject auraPrefab;

        private BarrierAuraController barrierAura;

        public override void OnOrderEffect()
        {
            base.OnOrderEffect();

            GameObject gameObject = Object.Instantiate(auraPrefab, base.transform.position, Quaternion.identity);
            barrierAura = gameObject.GetComponent<BarrierAuraController>();
            barrierAura.Networkowner = base.gameObject;

            NetworkServer.Spawn(gameObject);
        }
    }
}
