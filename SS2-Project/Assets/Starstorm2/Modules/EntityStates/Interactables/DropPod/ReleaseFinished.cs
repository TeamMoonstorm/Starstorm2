namespace EntityStates.DropPod
{
    public class ReleaseFinished : DropPodBaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Base", "Release");
        }
    }
}
