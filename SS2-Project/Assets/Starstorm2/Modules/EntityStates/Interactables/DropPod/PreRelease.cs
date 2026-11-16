using RoR2;
using UnityEngine;

namespace EntityStates.DropPod
{
    public class PreRelease : DropPodBaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Base", "IdleToRelease");
            Util.PlaySound("Play_UI_podBlastDoorOpen", gameObject);
            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                modelTransform.GetComponent<ChildLocator>().FindChild("InitialExhaustFX").gameObject.SetActive(true);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Animator modelAnimator = GetModelAnimator();
            if (modelAnimator)
            {
                int layerIndex = modelAnimator.GetLayerIndex("Base");
                if (layerIndex != -1 && modelAnimator.GetCurrentAnimatorStateInfo(layerIndex).IsName("IdleToReleaseFinished"))
                {
                    outer.SetNextState(new Release());
                }
            }
        }
    }
}
