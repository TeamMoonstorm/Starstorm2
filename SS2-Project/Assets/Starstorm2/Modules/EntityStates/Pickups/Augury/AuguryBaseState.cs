using SS2.Components;

namespace EntityStates.Pickups.Augury
{
    public abstract class AuguryBaseState : BaseBodyAttachmentState
    {
        protected AuguryEffectController auguryEffect;
        protected SS2.Items.Augury.AuguryBehavior auguryBehavior;

        public override void OnEnter()
        {
            base.OnEnter();
            ChildLocator cLoc = GetComponent<ChildLocator>();
            if (!cLoc)
                return;
            auguryEffect = cLoc.FindChild("Center")?.GetComponent<AuguryEffectController>();
            auguryBehavior = attachedBody.GetComponent<SS2.Items.Augury.AuguryBehavior>();
        }
    }
}