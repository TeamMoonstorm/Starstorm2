using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.CharacterBody;

namespace SS2.Items
{
    public sealed class WatchMetronome : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_WATCHMETRONOME_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acWatchMetronome", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Maximum movement speed bonus that can be achieved. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float maxMovementSpeed = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Maximum duration of the buff.")]
        [FormatToken(token, 1)]
        public static float maxDuration = 3f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Maximum duration of the buff, per stack.")]
        [FormatToken(token, 2)]
        public static float maxDurationPerStack = 2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Duration of the buff gained per second, while not sprinting.")]
        public static float gainPerSecond = 1;

        private static int maxBuffStacks = 5;
        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.EntityStates.BaseCharacterMain.UpdateAnimationParameters += BaseCharacterMain_UpdateAnimationParameters;
        }

        //
        private void BaseCharacterMain_UpdateAnimationParameters(On.EntityStates.BaseCharacterMain.orig_UpdateAnimationParameters orig, EntityStates.BaseCharacterMain self)
        {
            orig(self);

            if (self.characterAnimParamAvailability.isSprinting)
            {
                // While sprinting with the watch buff, body can sprint in any direction and their skills can be used while sprinting
                // skills that only animate the upper body will look weird with the sprint animation
                // skills that only animate the upper body also usually set the aim timer
                if (self.characterBody.HasBuff(SS2Content.Buffs.BuffWatchMetronome) && self.characterBody.aimTimer > 0)
                {
                    self.modelAnimator.SetBool(AnimationParameters.isSprinting, false);
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(SS2Content.Buffs.BuffWatchMetronome);
            if (buffCount > 0 && sender.isSprinting)
            {
                args.moveSpeedMultAdd +=  maxMovementSpeed * buffCount / maxBuffStacks;
            }
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.WatchMetronome;
            private float charge;

            private bool hadFlag = false;
            private bool hadAlwaysAim = false;
            private bool wasSprintingLastFrame = false;
            private void Start()
            {
                var prefabBody = BodyCatalog.GetBodyPrefabBodyComponent(body.bodyIndex);
                if (prefabBody)
                {
                    hadFlag = (prefabBody?.bodyFlags & CharacterBody.BodyFlags.SprintAnyDirection) == CharacterBody.BodyFlags.SprintAnyDirection;
                    hadAlwaysAim = prefabBody.alwaysAim;
                }
            }
            private void OnEnable()
            {
                body.onSkillActivatedAuthority += OnSkillActivatedAuthority;
            }

            private void OnSkillActivatedAuthority(GenericSkill skillSlot)
            {
                // fake "agile" effect.
                // if body was sprinting and activates a skill, make it continue sprinting
                // but, if the skill is normally cancelled from sprinting, dont continue sprinting
                if (wasSprintingLastFrame && body.GetBuffCount(SS2Content.Buffs.BuffWatchMetronome) > 0 && !skillSlot.skillDef.canceledFromSprinting)
                {
                    skillSlot.characterBody.isSprinting = true;
                }
            }

            public void FixedUpdate()
            {
                if (!body.isSprinting)
                {
                    charge += gainPerSecond * Time.fixedDeltaTime;
                }
                else
                {
                    float drainPerSecond = maxBuffStacks / (maxDuration + maxDurationPerStack * (stack-1));
                    charge -= drainPerSecond * Time.fixedDeltaTime;
                }
                charge = Mathf.Clamp(charge, 0, maxBuffStacks);

                bool hasBuff = body.GetBuffCount(SS2Content.Buffs.BuffWatchMetronome) > 0;
                SetSprintAnyDirection(hasBuff);
                // sprinting normally makes the body face the move direction (sprinting sets body.shouldAim to false)
                // with watch buff, player can use skills while sprinting, and sprint in any direction
                // we want the body to aim even while sprinting, but only if its supposed to (only if body.aimTimer > 0) (usually during skills)
                body.alwaysAim = hasBuff &&hadAlwaysAim || body.aimTimer > 0f;

                if (NetworkServer.active)
                {
                    body.SetBuffCount(SS2Content.Buffs.BuffWatchMetronome.buffIndex, Mathf.CeilToInt(charge));
                }

                wasSprintingLastFrame = body.isSprinting;
            }

            private void SetSprintAnyDirection(bool enabled)
            {
                if (hadFlag)
                {
                    return;
                }
                if (enabled)
                {
                    body.bodyFlags |= CharacterBody.BodyFlags.SprintAnyDirection;
                }
                else
                {
                    body.bodyFlags &= ~CharacterBody.BodyFlags.SprintAnyDirection;
                }
            }
            private void OnDisable()
            {
                SetSprintAnyDirection(false);
                body.alwaysAim = hadAlwaysAim;
                body.onSkillActivatedAuthority -= OnSkillActivatedAuthority;

                if (NetworkServer.active)
                {
                    body.SetBuffCount(SS2Content.Buffs.BuffWatchMetronome.buffIndex, 0);
                }
            }
        }
    }
}