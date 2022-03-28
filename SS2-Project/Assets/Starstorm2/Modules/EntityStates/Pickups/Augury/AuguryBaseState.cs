using Moonstorm.Starstorm2.Components;

namespace EntityStates.Pickups.Augury
{
    public abstract class AuguryBaseState : BaseBodyAttachmentState
    {
        protected AuguryEffectController auguryEffect;
        protected Moonstorm.Starstorm2.Items.Augury.Behavior auguryBehavior;

        public override void OnEnter()
        {
            base.OnEnter();
            ChildLocator cLoc = GetComponent<ChildLocator>();
            if (!cLoc)
                return;
            auguryEffect = cLoc.FindChild("Center")?.GetComponent<AuguryEffectController>();
            auguryBehavior = attachedBody.GetComponent<Moonstorm.Starstorm2.Items.Augury.Behavior>();
        }
    }
}