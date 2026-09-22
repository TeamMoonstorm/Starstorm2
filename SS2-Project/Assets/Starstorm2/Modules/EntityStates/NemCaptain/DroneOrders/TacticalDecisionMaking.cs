using SS2.Components;
using SS2;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemCaptain.Weapon
{
    public class TacticalDecisionMaking : BaseBuffOrder
    {
        private NemCaptainController nemCaptainController;
        public override void OnOrderEffect()
        {
            base.OnOrderEffect();
            nemCaptainController = GetComponent<NemCaptainController>();

            nemCaptainController.GrantFreeOrders(2);
            nemCaptainController.CycleAllOrders();
        }
    }
}
