using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Components;
using RoR2;

namespace EntityStates.Pickups.Extinction
{
    public class ExtinctionBaseState : EntityState
    {
        private protected ExtinctionController extinctionController { get; set; }
        private GenericOwnership genericOwnership;
        private SimpleLeash simpleLeash;
        private RadialForce radial;
        private MemoizedGetComponent<CharacterBody> bodyGetComponent;

        protected CharacterBody ownerBody
        {
            get
            {
                GenericOwnership genericOwnership = this.genericOwnership;
                return this.bodyGetComponent.Get((genericOwnership != null) ? genericOwnership.ownerObject : null);
            }
        }
        protected virtual bool shouldFollow
        {
            get
            {
                return true;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.genericOwnership = base.GetComponent<GenericOwnership>();
            this.simpleLeash = base.GetComponent<SimpleLeash>();
            this.extinctionController = base.GetComponent<ExtinctionController>();
            this.radial = base.GetComponent<RadialForce>();
        }
        public override void Update()
        {
            base.Update();
            if (this.ownerBody && this.shouldFollow)
            {
                this.simpleLeash.leashOrigin = this.ownerBody.corePosition;
            }
            //radial.radius += GetScale();
        }
        protected float GetScale() //TODO: Store it as cache or some shit, this only gets used for when the hole dies, meaning the player has died
        {
            float scale = 1f; //The prefab root and scale is set to 1 by default
            if (this.ownerBody)
            {
                if (this.ownerBody.inventory)
                {
                    scale *= (float)this.ownerBody.inventory.GetItemCount(SS2Content.Items.RelicOfExtinction);
                }
            }
            return scale;
        }
    }
}
