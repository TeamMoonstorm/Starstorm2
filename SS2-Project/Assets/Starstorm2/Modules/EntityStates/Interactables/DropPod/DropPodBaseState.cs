using Moonstorm.Starstorm2.Components;

namespace EntityStates.DropPod
{
    public abstract class DropPodBaseState : EntityState
    {
        protected DropPodController PodController { get; private set; }

        public override void OnEnter()
        {
            base.OnEnter();
            PodController = GetComponent<DropPodController>();
            if (!PodController && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}
