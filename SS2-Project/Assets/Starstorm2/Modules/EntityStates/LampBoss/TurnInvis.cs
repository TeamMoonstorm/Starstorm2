using EntityStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Navigation;
using System.Linq;
using UnityEngine.Networking;
using System;

namespace EntityStates.LampBoss
{
    public class TurnInvis : BaseState
    {
        public static float baseDuration;
        private float duration;
        private bool hasBuffed;
        private Animator animator;
        public static string mecanimParameter;
        private float timer;

        //blink related
        public float blinkDistance = 18f;
        public GameObject particles;
        private Vector3 blinkDestination = Vector3.zero;

        //summon related
        private BullseyeSearch enemySearch;
        public static SpawnCard spawnCard;
        private float summonTimer;
        private int summonCount;
        public static float summonInterval = 0.6f;
        public static int maxSummonCount = 5;

        public override void OnEnter()
        {
            base.OnEnter();
            hasBuffed = false;
            animator = GetModelAnimator();
            duration = baseDuration / attackSpeedStat;

            FindModelChild("GlowParticles").gameObject.SetActive(true);

            CalculateBlinkDestination();

            PlayCrossfade("FullBody, Override", "SecondaryBuff", "Secondary.playbackRate", duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            summonTimer += Time.fixedDeltaTime;
            if (NetworkServer.active && summonTimer > 0f && summonCount < maxSummonCount)
            {
                summonCount++;
                summonTimer -= summonInterval;
                if (isAuthority)
                    SummonFollower();
            }

            if (animator.GetFloat(mecanimParameter) >= 0.5f && !hasBuffed)
            {
                hasBuffed = true;
                FindModelChild("GlowParticles").gameObject.SetActive(false);
                SetPosition(blinkDestination);
                characterBody.AddTimedBuffAuthority(RoR2Content.Buffs.Cloak.buffIndex, 8f);
                characterBody.AddTimedBuffAuthority(RoR2Content.Buffs.CloakSpeed.buffIndex, 8f);
            }

            if (fixedAge >= duration)
                outer.SetNextStateToMain();
        }

        private void SetPosition(Vector3 newPosition)
        {
            if (characterMotor)
            {
                characterMotor.Motor.SetPositionAndRotation(newPosition, Quaternion.identity, true);
            }
        }

        private void CalculateBlinkDestination()
        {
            Vector3 vector = Vector3.zero;
            Ray aimRay = GetAimRay();
            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.searchOrigin = aimRay.origin;
            bullseyeSearch.searchDirection = aimRay.direction;
            bullseyeSearch.maxDistanceFilter = blinkDistance;
            bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
            bullseyeSearch.filterByLoS = false;
            bullseyeSearch.teamMaskFilter.RemoveTeam(TeamIndex.Player);
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
            bullseyeSearch.RefreshCandidates();
            HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
            if (hurtBox)
            {
                vector = hurtBox.transform.position - transform.position;
            }
            blinkDestination = transform.position;
            NodeGraph groundNodes = SceneInfo.instance.groundNodes;
            NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(transform.position + vector, characterBody.hullClassification, float.PositiveInfinity);
            groundNodes.GetNodePosition(nodeIndex, out blinkDestination);
            blinkDestination += transform.position - characterBody.footPosition;
            characterDirection.forward = vector;
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
