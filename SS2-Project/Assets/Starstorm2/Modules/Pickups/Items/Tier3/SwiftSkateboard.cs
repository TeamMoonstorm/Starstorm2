using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{

    public sealed class SwiftSkateboard : ItemBase
    {
        public const string token = "SS2_ITEM_SKATEBOARD_DESC";

        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("SwiftSkateboard", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Movement speed bonus for each skateboard push. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float moveSpeedBonus = 0.2f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Movement speed bonus for each skateboard push for each item stack past the first. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float moveSpeedBonusPerStack = 0.15f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Initial maximum stacks for the Swift Skateboard's movement speed buff.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static int maxStacks = 4;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Additional maximum stacks for each item stack of Swift Skateboard past the first.")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static int maxStacksPerStack = 0;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Duration of Swift Skateboard's movement speed buff.")]
        [TokenModifier(token, StatTypes.Default, 4)]
        public static float buffDuration = 3f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Cooldown between applications of Swift Skateboard's movement speed buff.")]
        [TokenModifier(token, StatTypes.Default, 5)]
        public static float buffCooldown = 0.75f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Whether Swift Skateboard should allow all-directional sprinting.")]
        public static bool omniSprint = true;

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.SwiftSkateboard;
            public CharacterMaster CharMaster { get => gameObject.GetComponent<CharacterMaster>(); }
            private float cooldownTimer;

            public void Start()
            {
                if (omniSprint)
                {
                    var cb = GetComponent<CharacterBody>();
                    if (!cb.bodyFlags.HasFlag(CharacterBody.BodyFlags.SprintAnyDirection))
                    {
                        cb.bodyFlags |= CharacterBody.BodyFlags.SprintAnyDirection;
                    }
                    else
                    {
                        //this means the character, without the effects of this item, had omnisprint
                        cb.gameObject.AddComponent<SkateboardToken>();
                    }
                }
                body.onSkillActivatedAuthority += Kickflip;
            }

            private void Kickflip(GenericSkill skill)
            {
                if (!body.HasBuff(SS2Content.Buffs.BuffKickflip))
                {
                    Util.PlaySound("SwiftSkateboard", body.gameObject);
                }
                if (stack > 0 && cooldownTimer == 0f)
                {
                    int buffCount = body.GetBuffCount(SS2Content.Buffs.BuffKickflip);
                    int maxBuffStack = maxStacks + (maxStacksPerStack * (stack - 1));
                    //body.AddTimedBuff(SS2Content.Buffs.BuffKickflip, buffDuration, maxStacks + ((stack - 1) * maxStacksPerStack));
                    if (buffCount < maxBuffStack)
                    {
                        body.AddTimedBuffAuthority(SS2Content.Buffs.BuffKickflip.buffIndex, buffDuration); //i swear if this works im killing hopoo
                    }
                    else if (buffCount == maxBuffStack)
                    {
                        body.RemoveOldestTimedBuff(SS2Content.Buffs.BuffKickflip.buffIndex);
                        body.AddTimedBuffAuthority(SS2Content.Buffs.BuffKickflip.buffIndex, buffDuration);
                    }


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
                if (omniSprint)
                {
                    var cb = GetComponent<CharacterBody>();
                    if (cb.bodyFlags.HasFlag(CharacterBody.BodyFlags.SprintAnyDirection) && !cb.gameObject.GetComponent<SkateboardToken>())
                    {
                        cb.bodyFlags &= ~CharacterBody.BodyFlags.SprintAnyDirection;
                    }
                    else
                    {
                        Destroy(cb.gameObject.GetComponent<SkateboardToken>());
                    }

                }
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
        public class SkateboardToken : MonoBehaviour
        {
            //public bool hadOmnisprint = true;
        }
    }
}