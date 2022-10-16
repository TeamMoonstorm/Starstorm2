using EntityStates;
using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Starstorm2.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Starstorm2/SkillDef/BeastmasterSkillDef")]
    internal class BeastmasterSkillDef : SkillDef
    {
        [Header("Beastmaster Parameters")]
        [Header("Beastmaster To-Tame Parameters")]
        [Tooltip("Entity state to use whenever the target is NOT a friend.")]
        public SerializableEntityStateType tameStateType;

        [Tooltip("This is the indicator prefab that the skill defaults to.")]
        public GameObject tameIndicatorPrefab;

        public Sprite tameSkillIcon;
        public string tameSkillNameToken;
        public string tameSkillDescToken;

        public float tameMaxTrackingDistance;
        public float tameMaxTrackingAngle;

        [Header("Beastmaster Friend Parameters")]
        [Tooltip("Entity state to use whenever the target is a friend.")]
        public SerializableEntityStateType friendStateType;

        public GameObject friendIndicatorPrefab;
        public Sprite friendSkillIcon;
        public string friendSkillNameToken;
        public string friendSkillDescToken;

        [Tooltip("Max distance for a friend to be considered available for friendStateType, set zero or less to disable.")]
        /// <summary>
        /// Max distance for a friend to be considered available for friendStateType, set zero or less to disable.
        /// </summary>
        public float friendMaxAvailableDistance;

        public override SkillDef.BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new BeastmasterSkillDef.InstanceData()
            {
                indicator = new Indicator(skillSlot.characterBody.gameObject, tameIndicatorPrefab),
                maxTrackingAngle = tameMaxTrackingAngle,
                maxTrackingDistance = tameMaxTrackingDistance,
            };
        }

        public override void OnUnassigned(GenericSkill skillSlot)
        {
            if (((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData) != null)
            {
                ((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData).Dispose();
            }
            base.OnUnassigned(skillSlot);
        }

        public override Sprite GetCurrentIcon(GenericSkill skillSlot)
        {
            if ((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData != null && ((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData).currentlyTrackingOrTamedTarget != null)
            {
                return IsTargetFriend(skillSlot) ? friendSkillIcon : tameSkillIcon;
            }
            return base.GetCurrentIcon(skillSlot);
        }

        public override string GetCurrentNameToken(GenericSkill skillSlot)
        {
            if ((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData != null && ((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData).currentlyTrackingOrTamedTarget != null)
            {
                return IsTargetFriend(skillSlot) ? friendSkillNameToken : tameSkillNameToken;
            }
            return base.GetCurrentNameToken(skillSlot);
        }

        public override string GetCurrentDescriptionToken(GenericSkill skillSlot)
        {
            if ((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData != null && ((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData).currentlyTrackingOrTamedTarget != null)
            {
                return IsTargetFriend(skillSlot) ? friendSkillDescToken : tameSkillDescToken;
            }
            return base.GetCurrentDescriptionToken(skillSlot);
        }

        /// <summary>
        /// Whenever it needs to check if the carrier has an available friend. Make sure that the target is not null before using this.
        /// </summary>
        /// <returns>True if the target is friend, false if not.</returns>
        public virtual bool IsTargetFriend(GenericSkill skillSlot)
        {
            if (((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData).currentlyTrackingOrTamedTarget == null)
            {
                throw new ArgumentNullException("BeastmasterSkillDef::IsTargetFriend()::((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData).currentlyTrackingOrTamedTarget is null.");
            }
            return !TeamManager.IsTeamEnemy(skillSlot.characterBody.teamComponent.teamIndex, ((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData).currentlyTrackingOrTamedTarget.teamComponent.teamIndex);
        }

        public virtual bool HasAvailableTargets([NotNull] GenericSkill skillSlot)
        {
            if (((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData).currentlyTrackingOrTamedTarget != null)
            {
                return !IsTargetFriend(skillSlot) || (IsTargetFriend(skillSlot) && (friendMaxAvailableDistance <= 0 || Vector3.Distance(skillSlot.characterBody.corePosition, ((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData).currentlyTrackingOrTamedTarget.corePosition) <= friendMaxAvailableDistance));
            }
            return false;
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return HasAvailableTargets(skillSlot) && base.CanExecute(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && HasAvailableTargets(skillSlot);
        }

        public override EntityState InstantiateNextState(GenericSkill skillSlot)
        {
            if ((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData != null && ((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData).currentlyTrackingOrTamedTarget != null)
            {
                return IsTargetFriend(skillSlot) ? EntityStateCatalog.InstantiateState(this.friendStateType) : EntityStateCatalog.InstantiateState(this.tameStateType);
            }
            return EntityStateCatalog.InstantiateState(this.activationState);
        }

        public override void OnFixedUpdate([NotNull] GenericSkill skillSlot)
        {
            base.OnFixedUpdate(skillSlot);
            if ((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData != null)
            {
                ((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData).DoFixedUpdate(skillSlot);
            }
        }

        /// <summary>
        /// Tracks an available target or a friend, and updates the indicator.
        /// </summary>
        protected class InstanceData : SkillDef.BaseSkillInstanceData, IDisposable
        {
            /// <summary>
            /// Constantly updated with different targets or null targets whenever searching for foes.
            /// </summary>
            public CharacterBody currentlyTrackingOrTamedTarget;

            public float trackerUpdateFrequency;
            public float maxTrackingDistance;
            public float maxTrackingAngle;

            private readonly BullseyeSearch search = new BullseyeSearch();
            internal Indicator indicator;
            private float trackerUpdateStopwatch;

            public InstanceData()
            {
                search.filterByLoS = true;
                search.sortMode = BullseyeSearch.SortMode.Distance;
            }

            public void SetNewTrackingTarget(CharacterBody characterBody)
            {
                currentlyTrackingOrTamedTarget = characterBody;
            }

            public void SetNewTrackingTarget(HurtBox hurtbox)
            {
                if (hurtbox && hurtbox.healthComponent && hurtbox.healthComponent.body)
                {
                    currentlyTrackingOrTamedTarget = hurtbox.healthComponent.body;
                }
            }

            public void DoFixedUpdate(GenericSkill runningGS)
            {
                this.trackerUpdateStopwatch += Time.fixedDeltaTime;
                if (this.trackerUpdateStopwatch >= 1f / this.trackerUpdateFrequency)
                {
                    this.trackerUpdateStopwatch -= 1f / this.trackerUpdateFrequency;
                    CharacterBody cb = this.currentlyTrackingOrTamedTarget;
                    this.SearchForTarget(runningGS);
                    this.indicator.targetTransform = (this.currentlyTrackingOrTamedTarget ? this.currentlyTrackingOrTamedTarget.transform : null);
                }
            }

            /// <summary>
            /// Only searches for a target whenever theres no target or the currently tracking or tamed target is enemy.
            /// </summary>
            /// <param name="skillSlot"></param>
            public virtual void SearchForTarget(GenericSkill skillSlot)
            {
                if (!currentlyTrackingOrTamedTarget || TeamManager.IsTeamEnemy(skillSlot.characterBody.teamComponent.teamIndex, currentlyTrackingOrTamedTarget.teamComponent.teamIndex))
                {
                    search.teamMaskFilter = TeamMask.GetEnemyTeams(skillSlot.characterBody.teamComponent.teamIndex);

                    search.searchOrigin = skillSlot.characterBody.inputBank.aimOrigin;
                    search.searchDirection = skillSlot.characterBody.inputBank.aimDirection;
                    search.maxDistanceFilter = this.maxTrackingDistance;
                    search.maxAngleFilter = this.maxTrackingAngle;
                    search.RefreshCandidates();
                    search.FilterOutGameObject(skillSlot.gameObject);
                    SetNewTrackingTarget(((BeastmasterSkillDef.InstanceData)skillSlot.skillInstanceData).search.GetResults().FirstOrDefault<HurtBox>());
                }
            }

            public void Dispose()
            {
                currentlyTrackingOrTamedTarget = null;
                indicator.DestroyVisualizer();
                indicator = null;
            }
        }
    }
}