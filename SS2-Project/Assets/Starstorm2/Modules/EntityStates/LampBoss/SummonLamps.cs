using UnityEngine;
using RoR2;
using RoR2.Navigation;
using System.Linq;
using UnityEngine.Networking;
using System;

namespace EntityStates.LampBoss
{
    public class SummonLamps : BaseState
    {
        private static float baseDuration = 2.4f;
        private static float startTime = 0.5f;
        private float duration;
        private Animator animator;

        //summon related
        private BullseyeSearch enemySearch;
        public static SpawnCard spawnCard;
        private float summonTimer;
        private int summonCount;
        private static float summonInterval = 0.3f;
        private static int maxSummonCount = 4;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            duration = baseDuration / attackSpeedStat;

            FindModelChild("GlowParticles").gameObject.SetActive(true);
            PlayCrossfade("FullBody, Override", "SecondaryBuff", "Secondary.playbackRate", duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration * startTime)
            {
                summonTimer += Time.fixedDeltaTime;
                if (NetworkServer.active && summonTimer > 0f && summonCount < maxSummonCount)
                {
                    summonCount++;
                    summonTimer -= summonInterval;
                    SummonFollower();
                }
            }
            

            if (fixedAge >= duration && isAuthority)
                outer.SetNextStateToMain();
        }

        private void SummonFollower()
        {
            Transform transform = characterBody.coreTransform;
            if (transform)
            {
                DirectorSpawnRequest request = new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    minDistance = 3f,
                    maxDistance = 20f,
                    spawnOnTarget = transform
                }, RoR2Application.rng);
                request.summonerBodyObject = gameObject;
                DirectorSpawnRequest request2 = request;
                request2.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine
                    (request2.onSpawnedServer, new Action<SpawnCard.SpawnResult>(delegate (SpawnCard.SpawnResult spawnResult)
                    {
                        if (spawnResult.success && spawnResult.spawnedInstance && characterBody)
                        {
                            Inventory inv = spawnResult.spawnedInstance.GetComponent<Inventory>();
                            if (inv)
                            {
                                inv.CopyEquipmentFrom(characterBody.inventory);
                            }
                        }
                    }));
                DirectorCore instance = DirectorCore.instance;
                if (instance == null)
                    return;
                instance.TrySpawnObject(request);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
