using RoR2;
using SS2;
using UnityEngine.Networking;
using static SS2.Interactables.DroneTable;

namespace EntityStates.CrystalPickup
{
    public abstract class CrystalBaseState : EntityState
    {
        protected SkinCrystal controller;
        protected PurchaseInteraction purchaseInter;

        protected virtual bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            controller = GetComponent<SkinCrystal>();
            purchaseInter = GetComponent<PurchaseInteraction>();
            if (NetworkServer.active)
            {
                purchaseInter.SetAvailable(enableInteraction);
            }
        }
    }
}
