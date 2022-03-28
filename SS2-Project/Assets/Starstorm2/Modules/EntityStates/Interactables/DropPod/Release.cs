using Moonstorm.Starstorm2.Components;
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
                switch (PodController.GetRewardType())
                {
                    case DropPodController.RewardType.Item:
                        Debug.Log("Item");
                        PodController.SpawnItem(cLocator.FindChild("ExitPos"));
                        spawnedRandom = true;
                        break;
                    case DropPodController.RewardType.Enemy:
                        Debug.Log("enemy");
                        PodController.SpawnEnemy(cLocator.FindChild("ExitPos"));
                        spawnedRandom = true;
                        break;
                    case DropPodController.RewardType.Corpse:
                        Debug.Log("Corpse");
                        PodController.SpawnCorpse(cLocator.FindChild("ExitPos"));
                        spawnedRandom = true;
                        break;
                    case DropPodController.RewardType.Nothing:
                        Debug.Log("Nothing");
                        spawnedRandom = true;
                        break;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && spawnedRandom)
            {
                outer.SetNextState(new ReleaseFinished());
            }
        }
    }
}
