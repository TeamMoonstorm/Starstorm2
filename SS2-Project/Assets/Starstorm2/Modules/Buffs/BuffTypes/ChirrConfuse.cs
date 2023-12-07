using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;
using System;
using UnityEngine;
using RoR2.CharacterAI;
using System.Linq;

namespace Moonstorm.Starstorm2.Buffs
{
    //[DisabledContent]
    public sealed class ChirrConfuse : BuffBase, IBodyStatArgModifier
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffChirrConfuse", SS2Bundle.Chirr);

        public static float slowAmount = 0.3f;

        

        public override void Initialize()
        {
            base.Initialize();
        }

        public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.moveSpeedMultAdd -= slowAmount;
        }


        // DEBUFFF PLAYER SOMEHOW
        public sealed class ChirrConfuseBuffBehavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation(useOnClient = false, useOnServer = true)]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffChirrConfuse;
            public static float resetTargetInterval = 3f;
            public static float targetSearchDistance = 30f;
            private float resetTargetStopwatch;

            private BaseAI ai;

            private BullseyeSearch enemySearch;

            private TeamIndex oldTeamIndex;

            private void Awake()
            {
                if(!base.body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless) && !body.isPlayerControlled)
                {
                    this.ai = base.body.master.aiComponents[0];
                }

                this.OnBuffStart();
            }
            private void OnBuffStart()
            {
                this.oldTeamIndex = base.body.teamComponent.teamIndex;
                base.body.teamComponent.teamIndex = TeamIndex.None;
                if (this.ai)
                {
                    this.ai.targetRefreshTimer = 3.5f;
                    this.ai.currentEnemy.Reset();
                    this.ai.customTarget.Reset();
                    this.ai.buddy.Reset();
                }
            }
            private void OnBuffEnd()
            {
                base.body.teamComponent.teamIndex = this.oldTeamIndex;
                if (this.ai)
                {
                    this.ai.currentEnemy.Reset();
                    this.ai.customTarget.Reset();
                    this.ai.buddy.Reset();
                    this.ai.ForceAcquireNearestEnemyIfNoCurrentEnemy();
                }
            }

            private void OnDestroy()
            {
                this.OnBuffEnd();
            }

            private void FixedUpdate()
            {
                this.resetTargetStopwatch -= Time.fixedDeltaTime;
                if(this.ai && this.resetTargetStopwatch <= 0f)
                {
                    this.resetTargetStopwatch += resetTargetInterval;
                    this.ai.targetRefreshTimer = 3.5f;
                    this.ai.currentEnemy.Reset();
                    this.ai.customTarget.Reset();
                    this.ai.buddy.Reset();

                    HurtBox hurtBox = this.FindEnemyHurtBox();
                    if (!hurtBox) hurtBox = this.ai.FindEnemyHurtBox(float.PositiveInfinity, true, true);

                    if(hurtBox && hurtBox.healthComponent)
                    {
                        this.ai.currentEnemy.gameObject = hurtBox.healthComponent.gameObject;
                        this.ai.currentEnemy.bestHurtBox = hurtBox;
                    }                                      
                }
            }

            private HurtBox FindEnemyHurtBox()
            {
                if (!this.body)
                {
                    return null;
                }
                this.enemySearch.viewer = this.body;
                this.enemySearch.teamMaskFilter = TeamMask.all;
                this.enemySearch.sortMode = BullseyeSearch.SortMode.Distance;
                this.enemySearch.minDistanceFilter = 0f;
                this.enemySearch.maxDistanceFilter = targetSearchDistance;
                this.enemySearch.searchOrigin = this.body.corePosition;
                this.enemySearch.searchDirection = this.body.inputBank ? this.body.inputBank.aimDirection : base.transform.forward;
                this.enemySearch.maxAngleFilter = 180f;
                this.enemySearch.filterByLoS = true;
                this.enemySearch.RefreshCandidates();
                return this.enemySearch.GetResults().FirstOrDefault<HurtBox>();
            }
        }
    }
}
