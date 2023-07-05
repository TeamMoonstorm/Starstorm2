using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Components;
using RoR2;
using static Moonstorm.Starstorm2.Interactables.DroneTable;

namespace EntityStates.DroneTable
{
    public abstract class DroneTableBaseState : EntityState
    {
        //protected DropPodController PodController { get; private set; }

        protected RefabricatorInteractionToken refabController;
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
            refabController = GetComponent<RefabricatorInteractionToken>();
            purchaseInter = GetComponent<PurchaseInteraction>();
            purchaseInter.SetAvailable(enableInteraction);
        }
    }
}
