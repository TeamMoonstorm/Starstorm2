using Moonstorm;
using Moonstorm.Starstorm2.Components;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Chirr
{
    public class ChirrMain : GenericCharacterMain
    {
        public static GameObject enemyPrefab;
        public static GameObject befriendPrefab;
        public static float friendRequiredHealth;
        public static float enemySearchAngle;
        public static float befriendSearchAngle;
        public static float hoverVelocity;
        public static float hoverAcceleration;
        public static float flyBuffCoefficient;
        [TokenModifier("SS2_KEYWORD_CHIRRBUFF", StatTypes.Percentage, 0)]
        public static float allyBuffCoefficient;
        public static float enemySlowCoefficient;
        public static BuffDef chirrFlyBuff;
        public static SkillDef specialOriginal;
        public static SkillDef specialCommand;

        private ChirrNetworkInfo chirrInfo;
        private BullseyeSearch bullseyeSearch;
        private Indicator befriendIndicator;
        private Indicator enemyIndicator;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();

            chirrInfo = characterBody.GetComponent<ChirrNetworkInfo>();
            animator = GetModelAnimator();

            SetupBullseyeSearch();
            SetupIndicator();
            Hook();
        }

        public void SetupBullseyeSearch()
        {
            if (bullseyeSearch != null) return;

            bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.teamMaskFilter = TeamMask.all;
            bullseyeSearch.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
            bullseyeSearch.filterByLoS = true;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
            bullseyeSearch.maxDistanceFilter = 10000;
            bullseyeSearch.maxAngleFilter = 10f;
        }

        public void SetupIndicator()
        {
            enemyIndicator = new Indicator(gameObject, null);
            if (enemyPrefab) enemyIndicator.visualizerPrefab = enemyPrefab;

            befriendIndicator = new Indicator(gameObject, null);
            if (befriendPrefab) befriendIndicator.visualizerPrefab = befriendPrefab;
        }

        public void Hook()
        {
            On.RoR2.Stage.Start += (orig, self) =>
            {
                orig(self);
                if (characterBody && chirrInfo)
                {
                    chirrInfo.friend = null;
                    chirrInfo.hasFriend = false;
                }
            };
            On.RoR2.PingerController.SetCurrentPing += FriendPing;
            characterBody.master.inventory.onInventoryChanged += OnInventoryChanged;
        }

        private void FriendPing(On.RoR2.PingerController.orig_SetCurrentPing orig, PingerController self, PingerController.PingInfo ping)
        {
            if (ping.targetGameObject)
            {
                CharacterBody pingBody = ping.targetGameObject.GetComponent<CharacterBody>();
                if (pingBody && chirrInfo.friend)
                {
                    BaseAI friendAI = chirrInfo.friend.master.GetComponent<BaseAI>();
                    friendAI.currentEnemy.gameObject = pingBody.gameObject;
                    chirrInfo.pingTarget = pingBody;
                    friendAI.UpdateTargets();
                }
            }
            orig(self, ping);
        }

        private void OnInventoryChanged()
        {
            if (chirrInfo.friend && NetworkServer.active)
            {
                Inventory friendInventory = chirrInfo.friend.master.inventory;
                friendInventory.CopyItemsFrom(chirrInfo.baseInventory);
                friendInventory.AddItemsFrom(characterBody.inventory);
                friendInventory.ResetItem(RoR2Content.Items.WardOnLevel.itemIndex);
                friendInventory.ResetItem(RoR2Content.Items.BeetleGland.itemIndex);
                friendInventory.ResetItem(RoR2Content.Items.CrippleWardOnLevel.itemIndex);
                friendInventory.ResetItem(RoR2Content.Items.TPHealingNova.itemIndex);
                friendInventory.ResetItem(RoR2Content.Items.FocusConvergence.itemIndex);
                friendInventory.ResetItem(RoR2Content.Items.TitanGoldDuringTP.itemIndex);
                friendInventory.ResetItem(RoR2Content.Items.ExtraLife.itemIndex);
            }
        }

        public override void HandleMovements()
        {
            base.HandleMovements();
            if (hasCharacterMotor && isAuthority)
            {
                if (!characterMotor.isGrounded)
                {
                    if (!characterBody.HasBuff(chirrFlyBuff))
                        characterBody.AddBuff(chirrFlyBuff);
                }

                if (characterMotor.isGrounded)
                {
                    if (characterBody.HasBuff(chirrFlyBuff))
                        characterBody.RemoveBuff(chirrFlyBuff);
                }
            }
        }

        public override void ProcessJump()
        {
            base.ProcessJump();
            if (hasCharacterMotor && hasInputBank && isAuthority)
            {
                bool hoverInput = inputBank.jump.down && characterMotor.velocity.y < 0f && !characterMotor.isGrounded;
                animator.SetBool("isGliding", hoverInput);

                if (hoverInput && isAuthority)
                {
                    float num = characterMotor.velocity.y;
                    num = Mathf.MoveTowards(num, hoverVelocity, hoverAcceleration * Time.fixedDeltaTime);
                    characterMotor.velocity = new Vector3(characterMotor.velocity.x, num, characterMotor.velocity.z);
                }

            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!chirrInfo.hasFriend)
            {
                if (chirrInfo.friend)
                {
                    // This is where we put the second skill for befriend
                    //skillLocator.special.SetBaseSkill(specialCommand);
                    skillLocator.special.SetSkillOverride(this, specialCommand, GenericSkill.SkillOverridePriority.Contextual);
                    chirrInfo.hasFriend = true;
                }
            }
            else if (!chirrInfo.friend)
            {
                // Reset skill back to default
                //skillLocator.special.SetBaseSkill(specialOriginal);
                skillLocator.special.UnsetSkillOverride(this, specialCommand, GenericSkill.SkillOverridePriority.Contextual);
                skillLocator.special.RemoveAllStocks();
                chirrInfo.hasFriend = false;
                chirrInfo.pingTarget = null;
                chirrInfo.enemyTarget = null;
                chirrInfo.friendOrigIndex = TeamIndex.None;
            }

            if (chirrInfo.friend)
            {
                BaseAI friendAI = chirrInfo.friend.master.GetComponent<BaseAI>();
                if (chirrInfo.futureFriend)
                {
                    chirrInfo.futureFriend = null;
                    UpdateIndicator(befriendIndicator, null);
                }
                if (chirrInfo.pingTarget)
                {
                    if (friendAI)
                    {
                        UpdateIndicator(enemyIndicator, friendAI.currentEnemy.characterBody.mainHurtBox.transform);
                    }
                }
                else if (chirrInfo.enemyTarget)
                {
                    if (friendAI)
                    {
                        friendAI.currentEnemy.gameObject = chirrInfo.enemyTarget.gameObject;
                        friendAI.UpdateTargets();
                        UpdateIndicator(enemyIndicator, friendAI.currentEnemy.mainHurtBox.transform);
                    }
                }
                else UpdateIndicator(enemyIndicator, null);
                if (inputBank.skill1.justPressed)
                {
                    chirrInfo.enemyTarget = GetEnemyTarget().healthComponent.body;
                }
            }
            else
            {
                UpdateIndicator(enemyIndicator, null);
                chirrInfo.futureFriend = GetBefriendTarget();
            }
        }

        public HurtBox GetEnemyTarget()
        {
            if (bullseyeSearch == null) SetupBullseyeSearch();
            HurtBox target = null;

            Ray aimRay = GetAimRay();
            bullseyeSearch.searchOrigin = aimRay.origin;
            bullseyeSearch.searchDirection = aimRay.direction;
            bullseyeSearch.maxAngleFilter = enemySearchAngle;
            bullseyeSearch.RefreshCandidates();

            target = bullseyeSearch.GetResults().FirstOrDefault();

            return target;
        }

        public HurtBox GetBefriendTarget()
        {
            if (bullseyeSearch == null) SetupBullseyeSearch();

            HurtBox futureFriend = null;
            //Debug.LogWarning("Chirr: GetBefriendTarget");

            if (!chirrInfo.friend)
            {
                Ray aimRay = GetAimRay();
                bullseyeSearch.searchOrigin = aimRay.origin;
                bullseyeSearch.searchDirection = aimRay.direction;
                bullseyeSearch.maxAngleFilter = befriendSearchAngle;
                bullseyeSearch.RefreshCandidates();

                List<HurtBox> targets = bullseyeSearch.GetResults().ToList();
                foreach (HurtBox target in targets)
                {
                    if (!target.healthComponent.body.master.isBoss &&
                        (target.healthComponent.combinedHealthFraction < friendRequiredHealth + (characterBody.executeEliteHealthFraction / 2f))
                        && !target.healthComponent.body.master.GetComponent<PlayerCharacterMasterController>())
                    {
                        futureFriend = target;
                        break;
                    }
                }
                if (futureFriend) UpdateIndicator(befriendIndicator, futureFriend.transform);
                else UpdateIndicator(befriendIndicator, null);
            }
            else UpdateIndicator(befriendIndicator, null);
            return futureFriend;
        }

        public void UpdateIndicator(Indicator indicator, Transform targetTransform)
        {
            indicator.targetTransform = targetTransform;
            if (targetTransform == null) indicator.active = false;
            else indicator.active = true;
        }

        public override void OnExit()
        {
            On.RoR2.PingerController.SetCurrentPing -= FriendPing;
            characterBody.master.inventory.onInventoryChanged -= OnInventoryChanged;

            UpdateIndicator(enemyIndicator, null);

            base.OnExit();

            bullseyeSearch = null;
        }
    }
}
