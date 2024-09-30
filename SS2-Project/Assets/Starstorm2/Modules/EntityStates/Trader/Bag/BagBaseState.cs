using RoR2;
using SS2.Components;

namespace EntityStates.Trader.Bag
{
    public class BagBaseState : BaseState
    {
        protected PickupPickerController pickupPickerController;
        protected TraderController traderController;

        protected virtual bool enableInteraction
        {
            get
            {
                return true;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            pickupPickerController = GetComponent<PickupPickerController>();
            traderController = GetComponent<TraderController>();
            pickupPickerController.SetAvailable(enableInteraction);
        }
    }
}
