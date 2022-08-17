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

namespace Moonstorm.Starstorm2.Items
{
    public sealed class BabyToys : ItemBase
    {
        private const string token = "SS2_ITEM_BABYTOYS_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("BabyToys");

        [ConfigurableField(ConfigDesc = "Levels removed per stack")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static int levelReductionPerStack = 3;

        public override void Initialize()
        {
            base.Initialize();
            
        }

        
        //better to do this with IL than after a normal hook, so other mods doing the same thing are guaranteed to catch it with a normal hook 
        /*private void UpdateExperienceOffsetInfo(ILCursor c)
        {
            c.MoveAfterLabels();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<CharacterBody>>((body) =>
            {
                if (body.master)
                {
                    if (BabyToysExperienceOffset.masterToExperienceOffset.TryGetValue(body.master, out BabyToysExperienceOffset experienceOffset))
                    {
                        ref BabyToysExperienceOffset.LevelInfo levelInfo = ref experienceOffset.levelInfo;
                        TeamIndex teamIndex = body.master.teamIndex;
                        TeamManager instance = TeamManager.instance;
                        levelInfo.experience = instance.GetTeamExperience(teamIndex);
                        levelInfo.level = TeamManager.FindLevelForExperience(levelInfo.experience);
                        levelInfo.currentLevelExperience = TeamManager.GetExperienceForLevel(levelInfo.level);
                        levelInfo.nextLevelExperience = TeamManager.GetExperienceForLevel(levelInfo.level + 1U);
                    }
                }
            });

        }*/
        
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
                if (levelChange > 0)
                {
                    remainingLevelReduction += (uint)levelChange;
                    TryReduceLevel();
                }
                RefreshLevelText();
            }
            public void TryReduceLevel()
            {
                ulong currentExperience = master.SS2GetAdjustedExperience();
                SS2Log.Info("Current experience" + currentExperience);
                uint currentLevel = TeamManager.FindLevelForExperience(currentExperience);
                SS2Log.Info("Current level" + currentLevel);
                uint newLevel = currentLevel - remainingLevelReduction;
                //account for overflow
                if(newLevel > currentLevel || newLevel < 1U)
                {
                    SS2Log.Info("resetting current level to 1U");
                    newLevel = 1U;
                }
                SS2Log.Info("new level: " + newLevel);
                uint levelsReduced = currentLevel - newLevel;
                SS2Log.Info("levels reduced: " + levelsReduced);
                if (levelsReduced <= 0U)
                {
                    SS2Log.Info("levels did not change! returning");
                    return;
                }
                remainingLevelReduction -= levelsReduced;

                ulong currentLevelExperience = TeamManager.GetExperienceForLevel(currentLevel);
                ulong nextLevelExperience = TeamManager.GetExperienceForLevel(currentLevel + 1U);

                ulong newCurrentLevelExperience = TeamManager.GetExperienceForLevel(newLevel);
                ulong newNextLevelExperience = TeamManager.GetExperienceForLevel(newLevel + 1U);

                //inverse lerp
                double currentLevelProgress = (double)(currentExperience - currentLevelExperience) / (double)(nextLevelExperience - currentLevelExperience);
                SS2Log.Info("current level progress: " + currentLevelProgress);

                //lerp
                long newExperience = (long)Math.Ceiling(newCurrentLevelExperience + (double)(newNextLevelExperience - newCurrentLevelExperience) * currentLevelProgress);
                SS2Log.Info("new experience: " + newExperience);
                long experienceChange = newExperience - (long)currentExperience;
                SS2Log.Info("exp change: " + experienceChange);

                master.SS2OffsetExperience(experienceChange);
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
                if(remainingLevelReduction > 0U && body.master == master)
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
        //Note from groove - I promise the voices had no say in the creation of this class, it seemed like the best way to handle it
        /*public class BabyToysExperienceOffset : MonoBehaviour
        {
            [Serializable]
            public struct LevelInfo
            {
                public uint level;
                public ulong experience;
                public ulong currentLevelExperience;
                public ulong nextLevelExperience;
            }
            public static Dictionary<CharacterMaster, BabyToysExperienceOffset> masterToExperienceOffset = new Dictionary<CharacterMaster, BabyToysExperienceOffset>();
            public CharacterMaster master;
            [SerializeField]
            public LevelInfo levelInfo;

            [SerializeField]
            public long experienceOffset;

            private bool hadPriorityInTeam;
            public byte ApplyStack()
            {
                //hasPriorityInTeam = true;
                if (master.hasBody)
                {
                    master.GetBody().RecalculateStats();
                }
                int amount = levelReductionPerStack;
                

                int currentLevel = (int)levelInfo.level;
                int levelReduction = Math.Min(amount, currentLevel - 1);
                ulong currentExperience = levelInfo.experience;
                float currentLevelProgress = Mathf.InverseLerp(levelInfo.currentLevelExperience, levelInfo.nextLevelExperience, currentExperience);
                uint newLevel = (uint)Math.Max(currentLevel - amount, 1);
                long newExperience = (long)Mathf.CeilToInt(Mathf.Lerp(TeamManager.GetExperienceForLevel(newLevel), TeamManager.GetExperienceForLevel(newLevel + 1U), currentLevelProgress));
                long experienceChange = newExperience - (long)currentExperience;

                experienceOffset += experienceChange;
            }
            private void Awake()
            {
                master = base.GetComponent<CharacterMaster>();
            }
            private void OnEnable()
            {
                masterToExperienceOffset[master] = this;
                //A lot of hooks, but doing it this way should maximize mod compat
                On.RoR2.UI.ExpBar.Update += ExpBar_Update;
                On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats1;
                On.RoR2.TeamManager.GetTeamCurrentLevelExperience += TeamManager_GetTeamCurrentLevelExperience;
                On.RoR2.TeamManager.GetTeamNextLevelExperience += TeamManager_GetTeamNextLevelExperience;
                On.RoR2.TeamManager.GetTeamExperience += TeamManager_GetTeamExperience;
                On.RoR2.TeamManager.GetTeamLevel += TeamManager_GetTeamLevel;
            }

            private void ExpBar_Update(On.RoR2.UI.ExpBar.orig_Update orig, ExpBar self)
            {
                hadPriorityInTeam = self.source == master;
                orig(self);
            }

            private void CharacterBody_RecalculateStats1(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
            {
                hadPriorityInTeam = self.master == master;
                orig(self);
            }

            private ulong TeamManager_GetTeamCurrentLevelExperience(On.RoR2.TeamManager.orig_GetTeamCurrentLevelExperience orig, TeamManager self, TeamIndex teamIndex)
            {
                if (hadPriorityInTeam)
                {
                    return TeamManager.GetExperienceForLevel(self.GetTeamLevel(teamIndex));
                }
                return orig(self, teamIndex);
            }

            private ulong TeamManager_GetTeamNextLevelExperience(On.RoR2.TeamManager.orig_GetTeamNextLevelExperience orig, TeamManager self, TeamIndex teamIndex)
            {
                if (hadPriorityInTeam)
                {
                    return TeamManager.GetExperienceForLevel(self.GetTeamLevel(teamIndex) + 1U);
                }
                return orig(self, teamIndex);
            }

            private ulong TeamManager_GetTeamExperience(On.RoR2.TeamManager.orig_GetTeamExperience orig, TeamManager self, TeamIndex teamIndex)
            {
                ulong original = orig(self, teamIndex);
                if (hadPriorityInTeam)
                {
                    return (ulong)Math.Max((long)original + experienceOffset, 0);
                }
                return original;
            }

            private uint TeamManager_GetTeamLevel(On.RoR2.TeamManager.orig_GetTeamLevel orig, TeamManager self, TeamIndex teamIndex)
            {
                if (hadPriorityInTeam)
                {
                    ulong currentExperience = self.GetTeamExperience(teamIndex);
                    uint level = TeamManager.FindLevelForExperience(currentExperience);
                    return level;
                }
                return orig(self, teamIndex);
            }
            private void OnDisable()
            {
                masterToExperienceOffset.Remove(master);
                On.RoR2.UI.ExpBar.Update -= ExpBar_Update;
                On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats1;
                On.RoR2.TeamManager.GetTeamCurrentLevelExperience -= TeamManager_GetTeamCurrentLevelExperience;
                On.RoR2.TeamManager.GetTeamNextLevelExperience -= TeamManager_GetTeamNextLevelExperience;
                On.RoR2.TeamManager.GetTeamExperience -= TeamManager_GetTeamExperience;
                On.RoR2.TeamManager.GetTeamLevel -= TeamManager_GetTeamLevel;
            }

        }*/
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