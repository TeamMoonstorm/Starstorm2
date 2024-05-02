using R2API;
using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;

using MSU;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace SS2.Items
{
    public sealed class X4 : SS2Item, IContentPackModifier
    {
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public static float secCooldown = 0.25f;
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Cooldown reduction per X-4 Stimulant. (1 = 100%)")]
        [FormatToken("SS2_ITEM_X4_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float cdReduction = .1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Regen boost when using secondary skill. (1 = 1hp/s)")]
        [FormatToken("SS2_ITEM_X4_DESC", 1)]
        public static float baseRegenBoost = 2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Regen boost per stack when using secondary skill. (1 = 1hp/s)")]
        [FormatToken("SS2_ITEM_X4_DESC", 2)]
        public static float stackRegenBoost = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Regen duration when using secondary skill. (1 = 1s)")]
        [FormatToken("SS2_ITEM_X4_DESC", 3)]
        public static float regenDuration = 3f;
        public static float extraRegeneration = 0.2f;

        public static ProcChainMask ignoredProcs;

        public static ItemDisplay itemDisplay;

        private BuffDef _x4Buff; //SS2Assets.LoadAsset<BuffDef>("BuffX4", SS2Bundle.Items);

        //N: Why u no skill index!
        public static Dictionary<string, bool> specialSkillNames;

        public override void Initialize()
        {
            InitalizeSpecialSkillsList(); // hi nebby :)
            //N: I hate you 3000
        }

        public void InitalizeSpecialSkillsList()
        {
            //N: I'm crying and shitting and farting rn
            specialSkillNames = new Dictionary<string, bool> //Name of skill, isIgnored
            {
                { "SS2_EXECUTIONER2_IONCHARGE_NAME", true },
                { "SS2_EXECUTIONER2_IONBURST_NAME", false },
                { "SS2_NEMCOMMANDO_SECONDARY_SHOOT_NAME", false },
                { "RAILGUNNER_SECONDARY_NAME", true },
                { "RAILGUNNER_SNIPE_HEAVY_NAME", false },
                { "RAILGUNNER_SECONDARY_ALT_NAME", true},
                { "RAILGUNNER_SNIPE_LIGHT_NAME", false }
            };
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "X4" - Items
             * BuffDef - "BuffX4" - Items
             */
            yield break;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.buffDefs.AddSingle(_x4Buff);
        }

        public sealed class Behavior : BaseItemBodyBehavior, IStatItemBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.X4;

            public void Start()
            {
                body.onSkillActivatedAuthority += X4HealOnSkillActivation;
            }

            private void X4HealOnSkillActivation(GenericSkill skill)
            {
                var charbody = skill.characterBody;
                if (charbody)
                {
                    if (charbody.inventory)
                    {
                        int count = charbody.inventory.GetItemCount(SS2Content.Items.X4.itemIndex);
                        if (count > 0)
                        {
                            bool success = specialSkillNames.TryGetValue(skill.skillDef.skillNameToken, out bool isIgnored);
                            if (success)
                            {
                                if (isIgnored)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                isIgnored = true;
                            }
                            if (!isIgnored || (skill == skill.characterBody.skillLocator.secondaryBonusStockSkill && skill.baseRechargeInterval != 0)) // if it doesnt have a zero sec cooldown, max buffs at the number of bonus stocks
                            {
                                int buffcap = skill.bonusStockFromBody + 1;

                                if (!isIgnored)
                                {
                                    var secondary = skill.characterBody.skillLocator.secondaryBonusStockSkill;
                                    if (skill.skillDef.skillNameToken == "SS2_EXECUTIONER2_IONBURST_NAME")
                                    {
                                        if (secondary.skillNameToken == "SS2_EXECUTIONER2_IONCHARGE_NAME")
                                        {
                                            if (secondary.stock <= 0)
                                            {
                                                return;
                                            }
                                        }
                                    }

                                    if (skill.skillDef.skillNameToken != secondary.skillNameToken) //if skill is technically not secondary slot, 
                                    {
                                        buffcap = secondary.bonusStockFromBody + 1; //this assumes that the """SECONDARY""" being used should benefit from backup mags. for at least all base, dlc1, and ss2 characters, this is true
                                    }

                                }

                                int buffCount = charbody.GetBuffCount(SS2Content.Buffs.BuffX4);

                                if (buffCount < buffcap)
                                {
                                    charbody.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);

                                }
                                else if (buffCount == buffcap && buffCount >= 1)
                                {
                                    SS2Util.RefreshOldestBuffStack(charbody, SS2Content.Buffs.BuffX4, regenDuration);
                                }
                            }
                            else if (skill == skill.characterBody.skillLocator.secondaryBonusStockSkill) // if the skill is spammable, cap the buff count at 1
                            {
                                charbody.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                                int buffCount = charbody.GetBuffCount(SS2Content.Buffs.BuffX4);
                                if (buffCount != 0)
                                {
                                    charbody.RemoveOldestTimedBuff(SS2Content.Buffs.BuffX4.buffIndex);
                                }
                            }
                        }
                    }
                }
            }

            public void RecalculateStatsEnd()
            {
                if (body.skillLocator)
                {
                    if (body.skillLocator.secondaryBonusStockSkill)
                    {
                        float timeMult = Mathf.Pow(1 - cdReduction, stack);
                        body.skillLocator.secondaryBonusStockSkill.cooldownScale *= timeMult;
                    }
                }
            }

            public void RecalculateStatsStart() { }

            private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody self, RecalculateStatsAPI.StatHookEventArgs args) { }
        }

        public sealed class X4Buff : BaseBuffBehaviour
        {
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffX4;

            //private float atkMult = Items.X4.atkSpeedBonus;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                //args.attackSpeedMultAdd += atkMult;
                if (CharacterBody.HasBuff(SS2Content.Buffs.BuffX4))
                {
                    int count = CharacterBody.GetItemCount(SS2Content.Items.X4);
                    float regenAmnt = Items.X4.baseRegenBoost + (Items.X4.stackRegenBoost * (count - 1));
                    args.baseRegenAdd += (regenAmnt + ((regenAmnt / 5) * CharacterBody.level)) * BuffCount; //original code not taken from bitter root becasue i was lazy
                                                                                                    //SS2Log.Debug(regenAmnt + " per buff stack");

                    //BuffCount is the replacement for buffStacks right? If it isn't kill me. -Jace
                }
            }
        }
    }
}