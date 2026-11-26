using SS2.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.DropPod
{
    public class Release : DropPodBaseState
    {
        public static float ejectionSpeed = 20;

        private bool spawnedRandom;

        private ChildLocator cLocator;

        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Base", "Release");
            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                cLocator = modelTransform.GetComponent<ChildLocator>();
                cLocator.FindChild("Door").gameObject.SetActive(false);
                cLocator.FindChild("ReleaseExhaustFX").gameObject.SetActive(true);
            }
            if (PodController && NetworkServer.active)
            {
                PodController.DropReward();
                outer.SetNextState(new ReleaseFinished());
            }
        }
    }
}
