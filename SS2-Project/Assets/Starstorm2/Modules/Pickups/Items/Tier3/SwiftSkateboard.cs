using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;

using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace SS2.Items
{
    public sealed class SwiftSkateboard : SS2Item, IContentPackModifier
    {
        public const string token = "SS2_ITEM_SKATEBOARD_DESC";
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Movement speed bonus for each skateboard push. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float moveSpeedBonus = 0.2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Movement speed bonus for each skateboard push for each item stack past the first. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float moveSpeedBonusPerStack = 0.15f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Initial maximum stacks for the Swift Skateboard's movement speed buff.")]
        [FormatToken(token, 2)]
        public static int maxStacks = 4;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Additional maximum stacks for each item stack of Swift Skateboard past the first.")]
        [FormatToken(token, 3)]
        public static int maxStacksPerStack = 0;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Duration of Swift Skateboard's movement speed buff.")]
        [FormatToken(token, 4)]
        public static float buffDuration = 6f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Whether Swift Skateboard should allow all-directional sprinting.")]
        public static bool omniSprint = true;

        private static GameObject _effectPrefab;

        private BuffDef _buffKickflip; // SS2Assets.LoadAsset<BuffDef>("BuffKickflip", SS2Bundle.Items);

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "SwiftSkateboard" - Items
             * GameObject - "SkateboardActivate" - Items
             * BuffDef - "BuffKickflip" - Items
             */
            yield break;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.buffDefs.AddSingle(_buffKickflip);
        }

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.SwiftSkateboard;

            private bool hadOmniSprint;

            public void Start()
            {
                body.onSkillActivatedServer += Kickflip;

                if (omniSprint)
                {
                    //check if body prefab had sprintanydirection
                    GameObject bodyPrefab = BodyCatalog.GetBodyPrefab(this.body.bodyIndex);
                    if (bodyPrefab)
                    {
                        CharacterBody body = bodyPrefab.GetComponent<CharacterBody>();
                        hadOmniSprint = body.bodyFlags.HasFlag(CharacterBody.BodyFlags.SprintAnyDirection);
                    }
                    this.body.bodyFlags |= CharacterBody.BodyFlags.SprintAnyDirection;
                }

            }

            private void Kickflip(GenericSkill skill)
            {
                // no buff on primary use. increased duration to compensate
                if (this.body.skillLocator.primary == skill) return;

                if (!body.HasBuff(SS2Content.Buffs.BuffKickflip))
                {
                    Util.PlaySound("SwiftSkateboard", body.gameObject);
                }
                if (stack > 0)
                {
                    int buffCount = body.GetBuffCount(SS2Content.Buffs.BuffKickflip);
                    int maxBuffStack = maxStacks + (maxStacksPerStack * (stack - 1));
                    //body.AddTimedBuff(SS2Content.Buffs.BuffKickflip, buffDuration, maxStacks + ((stack - 1) * maxStacksPerStack));
                    if (buffCount < maxBuffStack)
                    {
                        body.AddTimedBuff(SS2Content.Buffs.BuffKickflip.buffIndex, buffDuration);
                    }
                    else if (buffCount == maxBuffStack)
                    {
                        body.RemoveOldestTimedBuff(SS2Content.Buffs.BuffKickflip.buffIndex);
                        body.AddTimedBuff(SS2Content.Buffs.BuffKickflip.buffIndex, buffDuration);
                    }

                    RefreshBuff();


                    // NO SOUND :((((((((((((((((((((((((((((((
                    EffectData effectData = new EffectData();
                    effectData.origin = base.body.corePosition;
                    CharacterDirection characterDirection = base.body.characterDirection;
                    effectData.rotation = characterDirection && characterDirection.moveVector != Vector3.zero ? Util.QuaternionSafeLookRotation(characterDirection.moveVector) : base.body.transform.rotation;
                    EffectManager.SpawnEffect(_effectPrefab, effectData, true);
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

            private void OnDestroy()
            {
                body.onSkillActivatedServer -= Kickflip;

                if (omniSprint && !this.hadOmniSprint)
                {
                    this.body.bodyFlags &= ~CharacterBody.BodyFlags.SprintAnyDirection;
                }
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