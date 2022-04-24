/*using RoR2EditorKit.Common;
using RoR2EditorKit.Core;
using RoR2EditorKit.Core.Windows;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Util = RoR2EditorKit.Util;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public class CreateMSIDRSWindow : CreateRoR2ScriptableObjectWindow<MSIDRS>
    {
        [Flags]
        public enum MSIDRSFlags : short
        {
            None = 0,
            VanillaWhites = 1,
            VanillaGreens = 2,
            VanillaReds = 4,
            VanillaYellows = 8,
            VanillaLunars = 16,
            VanillaEquipments = 32,
            VanillaLunarEquipments = 64,
            VanillaElites = 128,
            Everything = ~None
        }

        public MSIDRS msidrs;

        public static Dictionary<MSIDRSFlags, List<(string, string, int)>> FlagsToItemLists = new Dictionary<MSIDRSFlags, List<(string, string, int)>>
        {
            {
                MSIDRSFlags.VanillaWhites, new List<(string,string, int)>
                {
                    ("BossDamageBonus", "DisplayAPRound", 1),
                    ("SecondarySkillMagazine", "DisplayDoubleMag", 1),
                    ("FlatHealth", "DisplaySteakCurved", 1),
                    ("Firework", "DisplayFirework", 1),
                    ("Mushroom", "DisplayMushroom", 1),
                    ("HealWhileSafe", "DisplaySnail", 1),
                    ("Crowbar", "DisplayCrowbar", 1),
                    ("SprintBonus", "DisplaySoda", 1),
                    ("NearbyDamageBonus", "DisplayDiamond", 1),
                    ("IgniteOnKill", "DisplayGasoline", 1),
                    ("CritGlasses", "DisplayGlasses", 1),
                    ("Medkit", "DisplayMedkit", 1),
                    ("Hoof", "DisplayHoof", 1),
                    ("PersonalShield", "DisplayShieldGenerator", 1),
                    ("ArmorPlate", "DisplayRepulsionArmorPlate", 1),
                    ("TreasureCache", "DisplayKey", 1),
                    ("StickyBomb", "DisplayStickyBomb", 1),
                    ("StunChanceOnHit", "DisplayStunGrenade", 1),
                    ("BarrierOnKill", "DisplayBrooch", 1),
                    ("Bear", "DisplayBear", 1),
                    ("BleedOnHit", "DisplayTriTip", 1),
                    ("WardOnLevel", "DisplayWarbanner", 1)
                }
            },
            {
                MSIDRSFlags.VanillaGreens, new List<(string, string, int)>
                {
                    ("Missile", "DisplayMissileLauncher", 1),
                    ("WarCryOnMultikill", "DisplayPauldron", 1),
                    ("SlowOnHit", "DisplayBauble", 1),
                    ("Deathmark", "DisplayDeathmark", 1),
                    ("EquipmentMagazine", "DisplayBattery", 1),
                    ("BonusGoldPackOnKill", "DisplayTome", 1),
                    ("HealOnCrit", "DisplayScythe", 1),
                    ("Feather", "DisplayFeather", 1),
                    ("Infusion", "DisplayInfusion", 1),
                    ("FireRing", "DisplayFireRing", 1),
                    ("Seed", "DisplaySeed", 1),
                    ("TPHealingNova", "DisplayGlowFlower", 1),
                    ("ExecuteLowHealthElite", "DisplayGuillotine", 1),
                    ("Phasing", "DisplayStealthKit", 1),
                    ("AttackSpeedOnCrit", "DisplayWolfPelt", 1),
                    ("Thorns", "DisplayRazorwireLeft", 1),
                    ("SprintOutOfCombat", "DisplayWhip", 1),
                    ("SprintArmor", "DisplayBuckler", 1),
                    ("IceRing", "DisplayIceRing", 1),
                    ("Squid", "DisplaySquidTurret", 1),
                    ("ChainLightning", "DisplayUkulele", 1),
                    ("EnergizedOnEquipmentUse", "DisplayWarhorn", 1),
                    ("JumpBoost", "DisplayWaxBird", 1),
                    ("ExplodeOnDeath", "DisplayWillOWisp", 1),
                }
            },
            {
                MSIDRSFlags.VanillaReds, new List<(string, string, int)>
                {
                    ("Clover", "DisplayClover", 1),
                    ("BarrierOnOverheal", "DisplayAegis", 1),
                    ("AlienHead", "DisplayAlienHead", 1),
                    ("KillEliteFrenzy", "DisplayBrainStalk", 1),
                    ("Behemoth", "DisplayBehemoth", 1),
                    ("Dagger", "DisplayDagger", 1),
                    ("ExtraLife", "DisplayHippo", 1),
                    ("Icicle", "DisplayFrostRelic", 1),
                    ("FallBoots", "DisplayGravBoots", 2),
                    ("GhostOnKill", "DisplayMask", 1),
                    ("UtilitySkillMagazine", "DisplayAfterburnerShoulderRing", 1),
                    ("Plant", "DisplayInterstellarDeskPlant", 1),
                    ("NovaOnHeal", "DisplayDevilHorns", 2),
                    ("IncreaseHealing", "DisplayAntler", 2),
                    ("LaserTurbine", "DisplayLaserTurbine", 1),
                    ("BounceNearby", "DisplayHook", 1),
                    ("ArmorReductionOnHit", "DisplayWarhammer", 1),
                    ("Talisman", "DisplayTalisman", 1),
                    ("ShockNearby", "DisplayTeslaCoil", 1),
                    ("Headhunter", "DisplaySkullcrown", 1),
                }
            },
            {
                MSIDRSFlags.VanillaYellows, new List<(string, string, int)>
                {
                    ("LightningStrikeOnHit", "DisplayChargedPerforator", 1),
                    ("NovaOnLowHealth", "DisplayJellyGuts", 1),
                    ("TitanGoldDuringTP", "DisplayGoldHeart", 1),
                    ("ShinyPearl", "DisplayShinyPearl", 1),
                    ("SprintWisp", "DisplayBrokenMask", 1),
                    ("SiphonOnLowHealth", "DisplaySiphonOnLowHealth", 1),
                    ("FireballsOnHit", "DisplayFireballsOnHit", 1),
                    ("Pearl", "DisplayPearl", 1),
                    ("ParentEgg", "DisplayParentEgg", 1),
                    ("BeetleGland", "DisplayBeetleGland", 1),
                    ("BleedOnHitAndExplode", "DisplayBleedOnHitAndExplode", 1),
                    ("Knurl", "DisplayKnurl", 1),
                }
            },
            {
                MSIDRSFlags.VanillaLunars, new List<(string, string, int)>
                {
                    ("LunarTrinket", "DisplayBeads", 1),
                    ("GoldOnHit", "DisplayBoneCrown", 1),
                    ("RepeatHeal", "DisplayCorpseFlower", 1),
                    ("MonstersOnShrineUse", "DisplayMonstersOnShrineUse", 1),
                    ("LunarSpecialReplacement", "DisplayBirdHeart", 1),
                    ("FocusConvergence", "DisplayFocusedConvergence", 1),
                    ("AutoCastEquipment", "DisplayFossil", 1),
                    ("LunarSecondaryReplacement", "DisplayBirdClaw", 1),
                    ("RandomDamageZone", "DisplayRandomDamageZone", 1),
                    ("LunarDagger", "DisplayLunarDagger", 1),
                    ("LunarUtilityReplacement", "DisplayBirdFoot", 1),
                    ("ShieldOnly", "DisplayShieldBug", 2),
                    ("LunarPrimaryReplacement", "DisplayBirdEye", 1),
                }
            },
            {
                MSIDRSFlags.VanillaEquipments, new List<(string, string, int)>
                {
                    ("Cleanse", "DisplayWaterPack", 1),
                    ("CommandMissile", "DisplayMissileRack", 1),
                    ("Gateway", "DisplayVase", 1),
                    ("Fruit", "DisplayFruit", 1),
                    ("DeathProjectile", "DisplayDeathProjectile", 1),
                    ("QuestVolatileBattery", "DisplayBatteryArray", 1),
                    ("TeamWarCry", "DisplayTeamWarCry", 1),
                    ("GainArmor", "DisplayElephantFigure", 1),
                    ("Jetpack", "DisplayBugWings", 1),
                    ("CritOnUse", "DisplayNeuralImplant", 1),
                    ("BFG", "DisplayBFG", 1),
                    ("BlackHole", "DisplayGravCube", 1),
                    ("Scanner", "DisplayScanner", 1),
                    ("Recycle", "DisplayRecycler", 1),
                    ("Lightning", "DisplayLightningArmRight", 1),
                    ("Saw", "DisplaySawmerangFollower", 1),
                    ("LifeStealOnHit", "DisplayLifeStealOnHit", 1),
                    ("DroneBackup", "DisplayRadio", 1),
                    ("GoldGat", "DisplayGoldGat", 1),
                    ("FireballDash", "DisplayEgg", 1),
                }
            },
            {
                MSIDRSFlags.VanillaLunarEquipments, new List<(string, string, int)>
                {
                    ("CrippleWard", "DisplayEffigy", 1),
                    ("Meteor", "DisplayMeteor", 1),
                    ("BurnNearby", "DisplayPotion", 1),
                    ("Tonic", "DisplayTonic", 1),
                }
            },
            {
                MSIDRSFlags.VanillaElites, new List<(string, string, int)>
                {
                    ("AffixWhite", "DisplayEliteIceCrown", 1),
                    ("AffixRed", "DisplayEliteHorn", 2),
                    ("AffixBlue", "DisplayEliteRhinoHorn", 2),
                    ("AffixPoison", "DisplayEliteUrchinCrown", 1),
                    ("AffixLunar", "DisplayEliteLunar,eye", 1),
                    ("AffixHaunted", "DisplayEliteStealthCrown", 1),
                }
            }

        };

        private bool createAssetNameFromKey = true;
        private MSIDRSFlags flags;

        private KeyAssetDisplayPairHolder keyAssetsAndDisplayPairs;

        [MenuItem(Constants.RoR2EditorKitContextRoot + "MoonstormSharedUtils/MSIDRS", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateMSIDRSWindow>(null, "Create MSIDRS");
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            msidrs = (MSIDRS)ScriptableObject;
            createAssetNameFromKey = true;
            mainSerializedObject = new SerializedObject(msidrs);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            DrawField("VanillaIDRSKey", mainSerializedObject);
            createAssetNameFromKey = EditorGUILayout.Toggle("Create Asset Name from IDRS Key", createAssetNameFromKey);
            if (!createAssetNameFromKey)
                nameField = EditorGUILayout.TextField("Asset Name", nameField);

            flags = (MSIDRSFlags)EditorGUILayout.EnumFlagsField("IDRS Flags", flags);
            keyAssetsAndDisplayPairs = (KeyAssetDisplayPairHolder)EditorGUILayout.ObjectField("Key Assets and Display Pairs", keyAssetsAndDisplayPairs, typeof(KeyAssetDisplayPairHolder), false);

            if (SimpleButton("Create MSIDRS"))
            {
                var result = CreateMSIDRS();
                if (result)
                {
                    Debug.Log($"Succesfully Created MSIDRS {msidrs.name}");
                    TryToClose();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private bool CreateMSIDRS()
        {
            actualName = createAssetNameFromKey ? $"MSIDRS{msidrs.VanillaIDRSKey}" : GetCorrectAssetName(nameField);
            try
            {
                if (string.IsNullOrEmpty(actualName))
                    throw ErrorShorthands.ThrowNullAssetName(nameof(nameField));

                msidrs.name = actualName;

                if (flags.HasFlag(MSIDRSFlags.VanillaWhites)) PopulateWithWhites();
                if (flags.HasFlag(MSIDRSFlags.VanillaGreens)) PopulateWithGreens();
                if (flags.HasFlag(MSIDRSFlags.VanillaReds)) PopulateWithReds();
                if (flags.HasFlag(MSIDRSFlags.VanillaYellows)) PopulateWithYellows();
                if (flags.HasFlag(MSIDRSFlags.VanillaLunars)) PopulateWithLunars();
                if (flags.HasFlag(MSIDRSFlags.VanillaEquipments)) PopulateWithEquipments();
                if (flags.HasFlag(MSIDRSFlags.VanillaLunarEquipments)) PopulateWithLunarEquipments();
                if (flags.HasFlag(MSIDRSFlags.VanillaElites)) PopulateWithEliteEquipments();

                if (keyAssetsAndDisplayPairs)
                {
                    PopulateWithKADP();
                }

                Util.CreateAssetAtSelectionPath(msidrs);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while creating MSIDRS: {e}");
                return false;
            }
        }

        private void PopulateWithWhites()
        {
            var list = msidrs.MSUKeyAssetRuleGroup;

            foreach (var tuple in FlagsToItemLists[MSIDRSFlags.VanillaWhites])
            {
                list.Add(CreateKARG(tuple.Item1, tuple.Item2, tuple.Item3));
            }

            var toothGroup = new MSIDRS.KeyAssetRuleGroup();
            toothGroup.keyAssetName = "Tooth";
            toothGroup.AddDisplayRule(new MSIDRS.ItemDisplayRule { displayPrefabName = "DisplayToothNecklaceDecal" });
            toothGroup.AddDisplayRule(new MSIDRS.ItemDisplayRule { displayPrefabName = "DisplayToothMeshLarge" });
            toothGroup.AddDisplayRule(new MSIDRS.ItemDisplayRule { displayPrefabName = "DisplayToothMeshSmall1" });
            toothGroup.AddDisplayRule(new MSIDRS.ItemDisplayRule { displayPrefabName = "DisplayToothMeshSmall1" });
            toothGroup.AddDisplayRule(new MSIDRS.ItemDisplayRule { displayPrefabName = "DisplayToothMeshSmall2" });
            toothGroup.AddDisplayRule(new MSIDRS.ItemDisplayRule { displayPrefabName = "DisplayToothMeshSmall2" });

            list.Add(toothGroup);

            mainSerializedObject.Update();
        }

        private void PopulateWithGreens()
        {
            var list = msidrs.MSUKeyAssetRuleGroup;

            foreach (var tuple in FlagsToItemLists[MSIDRSFlags.VanillaGreens])
            {
                list.Add(CreateKARG(tuple.Item1, tuple.Item2, tuple.Item3));
            }

            mainSerializedObject.Update();
        }

        private void PopulateWithReds()
        {
            var list = msidrs.MSUKeyAssetRuleGroup;

            foreach (var tuple in FlagsToItemLists[MSIDRSFlags.VanillaReds])
            {
                list.Add(CreateKARG(tuple.Item1, tuple.Item2, tuple.Item3));
            }

            mainSerializedObject.Update();
        }

        private void PopulateWithYellows()
        {
            var list = msidrs.MSUKeyAssetRuleGroup;

            foreach (var tuple in FlagsToItemLists[MSIDRSFlags.VanillaYellows])
            {
                list.Add(CreateKARG(tuple.Item1, tuple.Item2, tuple.Item3));
            }

            mainSerializedObject.Update();
        }

        private void PopulateWithLunars()
        {
            var list = msidrs.MSUKeyAssetRuleGroup;

            foreach (var tuple in FlagsToItemLists[MSIDRSFlags.VanillaLunars])
            {
                list.Add(CreateKARG(tuple.Item1, tuple.Item2, tuple.Item3));
            }

            mainSerializedObject.Update();
        }

        private void PopulateWithEquipments()
        {
            var list = msidrs.MSUKeyAssetRuleGroup;

            foreach (var tuple in FlagsToItemLists[MSIDRSFlags.VanillaEquipments])
            {
                list.Add(CreateKARG(tuple.Item1, tuple.Item2, tuple.Item3));
            }

            mainSerializedObject.Update();
        }

        private void PopulateWithLunarEquipments()
        {
            var list = msidrs.MSUKeyAssetRuleGroup;

            foreach (var tuple in FlagsToItemLists[MSIDRSFlags.VanillaLunarEquipments])
            {
                list.Add(CreateKARG(tuple.Item1, tuple.Item2, tuple.Item3));
            }

            mainSerializedObject.Update();
        }

        private void PopulateWithEliteEquipments()
        {
            var list = msidrs.MSUKeyAssetRuleGroup;

            foreach (var tuple in FlagsToItemLists[MSIDRSFlags.VanillaElites])
            {
                list.Add(CreateKARG(tuple.Item1, tuple.Item2, tuple.Item3));
            }

            mainSerializedObject.Update();
        }

        private void PopulateWithKADP()
        {
            var list = msidrs.MSUKeyAssetRuleGroup;

            foreach (var keyAssetDisplayPair in keyAssetsAndDisplayPairs.KeyAssetDisplayPairs)
            {
                if (keyAssetDisplayPair.keyAsset && keyAssetDisplayPair.displayPrefabs.Count > 0)
                {
                    var keyAssetName = keyAssetDisplayPair.keyAsset.name;
                    var constructedName = $"{keyAssetName}DisplayPrefab_0";
                    list.Add(CreateKARG(keyAssetName, constructedName, 1));
                }
            }

            mainSerializedObject.Update();
        }
        public static MSIDRS.KeyAssetRuleGroup CreateKARG(string keyAssetName, string displayName, int ruleAmount)
        {
            var ruleGroup = new MSIDRS.KeyAssetRuleGroup();
            ruleGroup.keyAssetName = keyAssetName;
            for (int i = 0; i < ruleAmount; i++)
            {
                ruleGroup.AddDisplayRule(new MSIDRS.ItemDisplayRule
                {
                    displayPrefabName = displayName
                });
            }
            return ruleGroup;
        }
    }
}
*/