using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2
{
    // hardcode city 2
    // TODO: visuals and sound
    public class RewardDropper : MonoBehaviour
    {
        [SystemInitializer]
        private static void Init()
        {
            optionPrefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OptionPickup/OptionPickup.prefab").WaitForCompletion();
            dtTier1 = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier1Item.asset").WaitForCompletion();
            dtTier2 = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier2Item.asset").WaitForCompletion();
            dtTier3 = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier3Item.asset").WaitForCompletion();

        }
        private static GameObject optionPrefab;
        private static PickupDropTable dtTier1;
        private static PickupDropTable dtTier2;
        private static PickupDropTable dtTier3;

        public struct RewardInfo
        {
            public GenericPickupController.PickupArtifactFlag flag;
            public GameObject prefabOverride;
            public PickupPickerController.Option[] options;
            public PickupIndex pickupIndex;
        }

        [NonSerialized]
        public RewardDef reward;

        [NonSerialized]
        public Xoroshiro128Plus rng;


        public Transform dropTransform;
        public float maxDropAngle = 360f;
        public float minDropAngle = 20f;
        public float maxDropInterval = 0.5f;
        public float minDropInterval = 0.1f;
        public float waitDuration = 2f;
        public float dropForwardVelocity = 5f;
        public float dropUpVelocity = 20f;

        private bool waiting = true;
        private float dropInterval;
        private float dropAngle;
        private float stopwatch;
        private int numDropped;
        private Queue<RewardInfo> pickupQueue;

        private void Start()
        {
            if (!NetworkServer.active) return;

            GeneratePickups();
            dropInterval = Util.Remap(pickupQueue.Count, 2, 24, maxDropInterval, minDropInterval);
            dropAngle = Util.Remap(pickupQueue.Count, 1, 24, maxDropAngle, minDropAngle);
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active) return;

            if(waiting)
            {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch >= waitDuration)
                {
                    waiting = false;
                    stopwatch = 0;
                }
            }
            else
            {
                stopwatch -= Time.fixedDeltaTime;
                if(stopwatch <= 0)
                {
                    stopwatch += dropInterval;
                    DropSingle();
                }
                if(pickupQueue.Count == 0)
                {
                    Destroy(base.gameObject); 
                }
            }
        }

        private void DropSingle()
        {
            RewardInfo rewardInfo = pickupQueue.Dequeue();

            Vector3 vector = (Vector3.up * dropUpVelocity + Vector3.forward * dropForwardVelocity);
            Quaternion rotation = Quaternion.AngleAxis(dropAngle * numDropped, Vector3.up);

            PickupDropletController.CreatePickupDroplet(new GenericPickupController.CreatePickupInfo
            {
                pickerOptions = rewardInfo.options,
                prefabOverride = rewardInfo.prefabOverride,
                position = this.dropTransform.position,
                rotation = Quaternion.identity,
                artifactFlag = rewardInfo.flag,
                pickupIndex = rewardInfo.pickupIndex,
            }, this.dropTransform.position, rotation * vector);

            numDropped++;
        }
        private void GeneratePickups()
        {
            pickupQueue = new Queue<RewardInfo>();
            PickupIndex[] guaranteedPickups = reward.pickups;
            for(int i = 0; i < guaranteedPickups.Length; i++)
            {
                pickupQueue.Enqueue(new RewardInfo { pickupIndex = guaranteedPickups[i] });
            }

            for(int i = 0; i < reward.white; i++)
            {
                PickupIndex pickupIndex = this.rng.NextElementUniform<PickupIndex>(Run.instance.availableTier1DropList);
                pickupQueue.Enqueue(new RewardInfo { pickupIndex = pickupIndex });
            }
            for (int i = 0; i < reward.whiteOption; i++)
            {
                PickupIndex[] options = dtTier1.GenerateUniqueDrops(3, this.rng);
                pickupQueue.Enqueue(new RewardInfo { prefabOverride = optionPrefab, options = PickupPickerController.GenerateOptionsFromArray(options), pickupIndex = PickupCatalog.FindPickupIndex(ItemTier.Tier1) });
            }
            for(int i = 0; i < reward.whiteCommand; i++)
            {
                pickupQueue.Enqueue(new RewardInfo { pickupIndex = Run.instance.availableTier1DropList[0], flag = GenericPickupController.PickupArtifactFlag.COMMAND });
            }

            for (int i = 0; i < reward.green; i++)
            {
                PickupIndex pickupIndex = this.rng.NextElementUniform<PickupIndex>(Run.instance.availableTier2DropList);
                pickupQueue.Enqueue(new RewardInfo { pickupIndex = pickupIndex });
            }
            for (int i = 0; i < reward.greenOption; i++)
            {
                PickupIndex[] options = dtTier2.GenerateUniqueDrops(3, this.rng);
                pickupQueue.Enqueue(new RewardInfo { prefabOverride = optionPrefab, options = PickupPickerController.GenerateOptionsFromArray(options), pickupIndex = PickupCatalog.FindPickupIndex(ItemTier.Tier2) });
            }
            for (int i = 0; i < reward.greenCommand; i++)
            {
                pickupQueue.Enqueue(new RewardInfo { pickupIndex = Run.instance.availableTier2DropList[0], flag = GenericPickupController.PickupArtifactFlag.COMMAND });
            }

            for (int i = 0; i < reward.red; i++)
            {
                PickupIndex pickupIndex = this.rng.NextElementUniform<PickupIndex>(Run.instance.availableTier3DropList);
                pickupQueue.Enqueue(new RewardInfo { pickupIndex = pickupIndex });
            }
            for (int i = 0; i < reward.redOption; i++)
            {
                PickupIndex[] options = dtTier3.GenerateUniqueDrops(3, this.rng);
                pickupQueue.Enqueue(new RewardInfo { prefabOverride = optionPrefab, options = PickupPickerController.GenerateOptionsFromArray(options), pickupIndex = PickupCatalog.FindPickupIndex(ItemTier.Tier3) });
            }
            for (int i = 0; i < reward.redCommand; i++)
            {
                pickupQueue.Enqueue(new RewardInfo { pickupIndex = Run.instance.availableTier3DropList[0], flag = GenericPickupController.PickupArtifactFlag.COMMAND });
            }
        }
    }
}
