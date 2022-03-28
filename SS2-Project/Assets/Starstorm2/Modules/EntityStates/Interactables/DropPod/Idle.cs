namespace EntityStates.DropPod
{
    public class Idle : DropPodBaseState
    {
        public override void OnEnter()
        {
            PlayAnimation("Base", "Idle");
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}