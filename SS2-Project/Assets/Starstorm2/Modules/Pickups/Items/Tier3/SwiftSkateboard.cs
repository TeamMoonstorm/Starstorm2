﻿using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{

    public sealed class SwiftSkateboard : ItemBase
    {
        public const string token = "SS2_ITEM_SKATEBOARD_DESC";

        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("SwiftSkateboard");

        [ConfigurableField(ConfigDesc = "Movement speed bonus for each skateboard push. (1 = 100%)")]
        [TokenModifier(token, StatTypes.Percentage, 0)]
        public static float moveSpeedBonus = 0.2f;

        [ConfigurableField(ConfigDesc = "Movement speed bonus for each skateboard push for each item stack past the first. (1 = 100%)")]
        public static float moveSpeedBonusPerStack = 0.15f;

        [ConfigurableField(ConfigDesc = "Initial maximum stacks for the Swift Skateboard's movement speed buff.")]
        public static int maxStacks = 4;

        [ConfigurableField(ConfigDesc = "Additional maximum stacks for each item stack of Swift Skateboard past the first.")]
        public static int maxStacksPerStack = 0;

        [ConfigurableField(ConfigDesc = "Duration of Swift Skateboard's movement speed buff.")]
        public static float buffDuration = 3f;

        [ConfigurableField(ConfigDesc = "Cooldown between applications of Swift Skateboard's movement speed buff.")]
        public static float buffCooldown = 0.75f;

        [ConfigurableField(ConfigDesc = "Whether skateboard should allow all-directoinal sprinting.")]
        public static bool omniSprint = true;

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.SwiftSkateboard;
            public CharacterMaster CharMaster { get => gameObject.GetComponent<CharacterMaster>(); }
            private float cooldownTimer;

            public void Start()
            {
                if (omniSprint) GetComponent<CharacterBody>().bodyFlags = CharacterBody.BodyFlags.SprintAnyDirection;
                body.onSkillActivatedAuthority += Kickflip;
            }

            private void Kickflip(GenericSkill skill)
            {
                if (body.HasBuff(SS2Content.Buffs.BuffKickflip))
                {
                    Util.PlaySound("SwiftSkateboard", body.gameObject);
                }
                if (cooldownTimer == 0f)
                {
                    body.AddTimedBuff(SS2Content.Buffs.BuffKickflip, buffDuration, maxStacks + ((stack - 1) * maxStacksPerStack));
                    cooldownTimer += buffCooldown;
                    RefreshBuff();
                }
            }

            private void RefreshBuff()
            {
                for (int i = 0; i < body.timedBuffs.Count; i++)
                {
                    if (body.timedBuffs[i].buffIndex == SS2Content.Buffs.BuffKickflip.buffIndex)
                    {
                        body.timedBuffs[i].timer = buffDuration;
                    }
                }
            }

            private void FixedUpdate()
            {
                if (cooldownTimer > 0f)
                {
                    cooldownTimer = Mathf.Clamp(cooldownTimer - Time.fixedDeltaTime, 0f, buffCooldown);
                }
            }

            private void OnDestroy()
            {
                if (omniSprint) GetComponent<CharacterBody>().bodyFlags -= CharacterBody.BodyFlags.SprintAnyDirection;
                body.onSkillActivatedAuthority -= Kickflip;
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (body.HasBuff(SS2Content.Buffs.BuffKickflip) && stack > 0)
                {
                    //args.baseMoveSpeedAdd += (body.baseMoveSpeed + body.levelMoveSpeed * (body.level - 1)) * (moveSpeedBonus * body.GetBuffCount(Starstorm2Content.Buffs.BuffKickflip));
                    args.moveSpeedMultAdd += (moveSpeedBonus + ((stack - 1) * moveSpeedBonusPerStack)) * body.GetBuffCount(SS2Content.Buffs.BuffKickflip);
                }
            }
        }
    }
}
