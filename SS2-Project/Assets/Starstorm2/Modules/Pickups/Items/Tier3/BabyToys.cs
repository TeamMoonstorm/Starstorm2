using R2API;
using RoR2;
using RoR2.Items;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2.UI;
using System.Text;
using System;
using UnityEngine;
using System.Collections.Generic;
using Moonstorm.Components;
using static RoR2.Items.BaseItemBodyBehavior;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class BabyToys : ItemBase
    {
        private const string pickupToken = "SS2_ITEM_BABYTOYS_PICKUP";

        private const string descToken = "SS2_ITEM_BABYTOYS_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("BabyToys", SS2Bundle.Items);

        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Levels removed per stack.")]
        [TokenModifier(pickupToken, StatTypes.Default, 0)]
        [TokenModifier(descToken, StatTypes.Default, 0)]
        public static int levelReductionPerStack = 3;

        public override void Initialize()
        {
            base.Initialize();
        }



        //public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        //{
        //    [ItemDefAssociation]
        //    private static ItemDef GetItemDef() => SS2Content.Items.BabyToys;
        //    public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
        //    {
        //        BabyToyToken token = body.GetComponent<BabyToyToken>();
        //        if (token)
        //        {
        //
        //        }
        //        args.levelFlatAdd += (stack * levelReductionPerStack - (int)token.remainingLevelReduction);
        //        //throw new NotImplementedException();
        //    }
        //}

        public sealed class MasterBehavior : BaseItemMasterBehavior
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            private static ItemDef GetItemDef() => SS2Content.Items.BabyToys;

            public const float levelTextScaleAdjustment = 15f;
            private uint remainingLevelReduction;
            public int bonusLevelCount
            {
                get
                {
                    return stack * levelReductionPerStack - (int)remainingLevelReduction;
                }
            }

            private int _stack;
            private List<LevelText> adjustedLevelText = new List<LevelText>();

            private void Start()
            {
                CheckStackChange();
            }

            private void OnDestroy()
            {
                stack = 0;
                CheckStackChange();
            }
            public void CheckStackChange()
            {
                if (_stack != stack)
                {
                    int change = stack - _stack;
                    _stack = stack;
                    OnStackChanged(change);
                }
            }
            public void OnStackChanged(int change)
            {
                int levelChange = change * levelReductionPerStack;
                BabyToyToken token = master.GetBody().GetComponent<BabyToyToken>();
                if (!token)
                {
                    token = master.GetBody().gameObject.AddComponent<BabyToyToken>();
                }
                if (levelChange > 0)
                {
                    remainingLevelReduction += (uint)levelChange;
                    token.remainingLevelReduction = remainingLevelReduction;
                    TryReduceLevel();
                }
                else if (levelChange < 0)
                {
                    remainingLevelReduction -= (uint)levelChange;
                    token.remainingLevelReduction = remainingLevelReduction;
                    TryIncreaseLevel();
                }
            }

            private void TryIncreaseLevel()
            {
                ulong currentExperience = master.SS2GetAdjustedExperience();
                uint currentLevel = TeamManager.FindLevelForExperience(currentExperience);
                uint newLevel = currentLevel + remainingLevelReduction;

                //if (newLevel > currentLevel || newLevel < 1U)
                //{
                //    newLevel = 1U;
                //}
                //uint levelsReduced = currentLevel - newLevel;
                //if (levelsReduced <= 0U)
                //{
                //    return;
                //}
                //remainingLevelReduction -= levelsReduced;

                ulong currentLevelExperience = TeamManager.GetExperienceForLevel(currentLevel);
                ulong nextLevelExperience = TeamManager.GetExperienceForLevel(currentLevel + 1U);

                ulong newCurrentLevelExperience = TeamManager.GetExperienceForLevel(newLevel);
                ulong newNextLevelExperience = TeamManager.GetExperienceForLevel(newLevel + 1U);

                //inverse lerp
                double currentLevelProgress = (double)(currentExperience - currentLevelExperience) / (double)(nextLevelExperience - currentLevelExperience);

                //lerp
                long newExperience = (long)Math.Ceiling(newCurrentLevelExperience + (double)(newNextLevelExperience - newCurrentLevelExperience) * currentLevelProgress);
                long experienceChange = newExperience - (long)currentExperience;

                bool hasBody = master.hasBody;
                //SS2Log.Debug("about to adjust exp");

                if (hasBody)
                {
                    var token = master.bodyInstanceObject.AddComponent<BabyToyToken>();
                }
                master.SS2OffsetExperience(experienceChange);

                if (hasBody)
                {
                    master.GetBody().RecalculateStats();
                    var token = master.bodyInstanceObject.GetComponent<BabyToyToken>();
                    Destroy(token);
                }
                RefreshLevelText();
            }

            public void TryReduceLevel()
            {
                ulong currentExperience = master.SS2GetAdjustedExperience();
                uint currentLevel = TeamManager.FindLevelForExperience(currentExperience);
                uint newLevel = currentLevel - remainingLevelReduction;
                if (newLevel > currentLevel || newLevel < 1U)
                {
                    newLevel = 1U;
                }
                uint levelsReduced = currentLevel - newLevel;
                if (levelsReduced <= 0U)
                {
                    return;
                }
                remainingLevelReduction -= levelsReduced;
                BabyToyToken token = master.GetBody().GetComponent<BabyToyToken>();
                if (token)
                {
                    token.remainingLevelReduction = remainingLevelReduction;
                }
                ulong currentLevelExperience = TeamManager.GetExperienceForLevel(currentLevel);
                ulong nextLevelExperience = TeamManager.GetExperienceForLevel(currentLevel + 1U);

                ulong newCurrentLevelExperience = TeamManager.GetExperienceForLevel(newLevel);
                ulong newNextLevelExperience = TeamManager.GetExperienceForLevel(newLevel + 1U);

                //inverse lerp
                double currentLevelProgress = (double)(currentExperience - currentLevelExperience) / (double)(nextLevelExperience - currentLevelExperience);

                //lerp
                long newExperience = (long)Math.Ceiling(newCurrentLevelExperience + (double)(newNextLevelExperience - newCurrentLevelExperience) * currentLevelProgress);
                long experienceChange = newExperience - (long)currentExperience;

                master.SS2OffsetExperience(experienceChange);

                if (master.hasBody)
                {
                    master.GetBody().RecalculateStats();
                }
                RefreshLevelText();
            }
            public void RefreshLevelText()
            {
                foreach (HUD hud in HUD.readOnlyInstanceList)
                {
                    if (hud.targetMaster == master && hud.levelText)
                    {
                        hud.levelText.displayData = uint.MaxValue;
                        hud.levelText.Update();
                    }
                }
            }
            private void OnEnable()
            {
                GlobalEventManager.onCharacterLevelUp += GlobalEventManager_onCharacterLevelUp;
                master.inventory.onInventoryChanged += Inventory_onInventoryChanged;
                IL.RoR2.UI.LevelText.SetDisplayData += LevelText_SetDisplayData;
                IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            }

            private void GlobalEventManager_onCharacterLevelUp(CharacterBody body)
            {
                if (remainingLevelReduction > 0U && body.master == master)
                {
                    TryReduceLevel();
                }
            }

            private void Inventory_onInventoryChanged()
            {
                CheckStackChange();

            }

            private void LevelText_SetDisplayData(ILContext il)
            {
                ILCursor c = new ILCursor(il);

                bool ILFound = c.TryGotoNext(MoveType.After,
                    x => x.MatchCall(nameof(StringBuilderExtensions), nameof(StringBuilderExtensions.AppendUint)),
                    x => x.MatchPop()
                    );

                if (ILFound)
                {
                    c.MoveAfterLabels();
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Action<LevelText>>((levelText) =>
                    {
                        bool shouldBeAffected = levelText.source && levelText.source.master == master && bonusLevelCount > 0;
                        bool hasBeenAdjusted = adjustedLevelText.Contains(levelText);
                        if (shouldBeAffected)
                        {
                            if (!hasBeenAdjusted && levelText.targetText)
                            {
                                RectTransform rectTransform = (RectTransform)levelText.targetText.transform;
                                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x + levelTextScaleAdjustment, rectTransform.sizeDelta.y);
                                adjustedLevelText.Add(levelText);
                            }
                            LevelText.sharedStringBuilder.Append(" <style=cIsUtility>+");
                            LevelText.sharedStringBuilder.AppendInt(bonusLevelCount);
                            LevelText.sharedStringBuilder.Append("</style>");
                        }
                        else
                        {
                            if (hasBeenAdjusted && levelText.targetText)
                            {
                                RectTransform rectTransform = (RectTransform)levelText.targetText.transform;
                                //Transform transf = new Transform(0)
                                rectTransform.transform.position += (Vector3.right * 1); 
                                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x - levelTextScaleAdjustment, rectTransform.sizeDelta.y);
                                adjustedLevelText.Remove(levelText);
                            }
                        }
                    });
                }
                else { SS2Log.Error(this + ": Level Text IL hook failed!"); }
            }

            private void CharacterBody_RecalculateStats(ILContext il)
            {
                ILCursor c = new ILCursor(il);
            
                int localLevelMultiplierLocIndex = -1;
            
                bool ILFound = c.TryGotoNext(MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchCall<CharacterBody>("get_level"),
                    x => x.MatchLdcR4(1),
                    x => x.MatchSub()
                    //thanks r2api...this is also applied afted the recalc stats api level multiplier probably but w/e
                    ) && c.TryGotoNext(MoveType.After,
                    x => x.MatchStloc(out localLevelMultiplierLocIndex)
                    );
            
                if (ILFound)
                {
                    c.MoveAfterLabels();
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldloc, localLevelMultiplierLocIndex);
                    c.EmitDelegate<Func<CharacterBody, float, float>>((body, localLevelMultiplier) =>
                    {
                        if (body.master == master)
                        {
                            return localLevelMultiplier + bonusLevelCount;
                        }
                        return localLevelMultiplier;
                    });
                    c.Emit(OpCodes.Stloc, localLevelMultiplierLocIndex);
                }
                else { SS2Log.Error(this + ": Bonus Level Stats IL hook failed!"); }
            
            }

            private void OnDisable()
            {
                IL.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
                IL.RoR2.UI.LevelText.SetDisplayData -= LevelText_SetDisplayData;
                master.inventory.onInventoryChanged -= Inventory_onInventoryChanged;
                GlobalEventManager.onCharacterLevelUp -= GlobalEventManager_onCharacterLevelUp;
            }

        }
        public class BabyToyToken : MonoBehaviour
        {
            public int pastStock;
            public int usesLeft;
            //helps keep track of the target and player responsible
            public CharacterBody PlayerOwner;
            public uint remainingLevelReduction;
        }
    }
}
/*[ConfigurableField(ConfigName = "Stat Multiplier", ConfigDesc = "Multiplier applied to the stats per stack.")]
[TokenModifier(token, StatTypes.Default, 0)]
[TokenModifier(token, StatTypes.DivideBy2, 1)]
public static float StatMultiplier = 3;
[ConfigurableField(ConfigName = "XP Multiplier", ConfigDesc = "Multiplier applied to XP Gain per stack.")]
[TokenModifier(token, StatTypes.Default, 2)]
[TokenModifier(token, StatTypes.DivideBy2, 3)]
public static float XPMultiplier = 2;
public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier, IOnKilledOtherServerReceiver
{
    [ItemDefAssociation]
    private static ItemDef GetItemDef() => SS2Content.Items.BabyToys;
    public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
    {
        args.armorAdd += GetStatAugmentation(body.levelArmor);
        args.baseAttackSpeedAdd += GetStatAugmentation(body.levelAttackSpeed);
        args.baseDamageAdd += GetStatAugmentation(body.levelDamage);
        args.baseHealthAdd += GetStatAugmentation(body.levelMaxHealth);
        args.baseMoveSpeedAdd += GetStatAugmentation(body.levelMoveSpeed);
        args.baseRegenAdd += GetStatAugmentation(body.levelRegen);
        args.baseShieldAdd += GetStatAugmentation(body.levelMaxShield);
        args.critAdd += GetStatAugmentation(body.levelCrit);
    }
    private float GetStatAugmentation(float stat)
    {
        return stat * (StatMultiplier + ((StatMultiplier / 2) * (stack - 1)));
    }
    public void OnKilledOtherServer(DamageReport damageReport)
    {
        if (damageReport.victimBody)
        {
            var deathRewards = damageReport.victimBody.GetComponent<DeathRewards>();
            if (deathRewards)
            {
                body.master.GiveExperience((ulong)(deathRewards.expReward * (XPMultiplier + ((XPMultiplier / 2) * (stack - 1)))));
            }
        }
    }
}*/