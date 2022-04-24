/*using RoR2EditorKit;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core;
using RoR2EditorKit.Core.Windows;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public class CreateSIDRSWindow : CreateRoR2ScriptableObjectWindow<MSSingleItemDisplayRule>
    {
        [Flags]
        public enum SIDRFlag : short
        {
            None = 0,
            Survivors = 1,
            Scavenger = 2,
            Enemies = 4,
            Everything = ~None
        }

        public MSSingleItemDisplayRule sidr;

        public static Dictionary<SIDRFlag, List<string>> FlagsToIDRS = new Dictionary<SIDRFlag, List<string>>
        {
            {
                SIDRFlag.Survivors, new List<string>
                {
                    "idrsBandit2",
                    "idrsCommando",
                    "idrsCaptain",
                    "idrsCroco",
                    "idrsEngi",
                    "idrsHuntress",
                    "idrsLoader",
                    "idrsMage",
                    "idrsMerc",
                    "idrsToolbotBody",
                    "idrsTreebot"
                }
            },
            {
                SIDRFlag.Enemies, new List<string>
                {
                    "idrsBell",
                    "idrsBeetle",
                    "idrsBeetleGuard",
                    "idrsBeetleQueen",
                    "idrsBison",
                    "idrsClayboss",
                    "idrsClayBruiser",
                    "idrsGolem",
                    "idrsGrandparent",
                    "idrsGravekeeperBody",
                    "idrsGreaterWisp",
                    "idrsHermitCrab",
                    "idrsImp",
                    "idrsImpBoss",
                    "idrsJellyfish",
                    "idrsLemurian",
                    "idrsLemurianBruiser",
                    "idrsMiniMushroom",
                    "idrsNullifier",
                    "idrsParent",
                    "idrsRoboballBoss",
                    "idrsRoboballMini",
                    "idrsTitan",
                    "idrsVagrant",
                    "idrsVulture",
                    "idrsWisp"
                }
            },
            {
                SIDRFlag.Scavenger, new List<string>
                {
                    "idrsScav"
                }
            }
        };

        private bool createAssetNameFromKeyAsset = true;
        private SIDRFlag flags;
        private int ruleAmount;
        private IDRSHolder idrsHolder;

        [MenuItem(Constants.RoR2EditorKitContextRoot + "MoonstormSharedUtils/SIDR", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateSIDRSWindow>(null, "Create SIDR");
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            sidr = (MSSingleItemDisplayRule)ScriptableObject;
            createAssetNameFromKeyAsset = true;
            mainSerializedObject = new SerializedObject(sidr);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            DrawField("keyAssetName", mainSerializedObject);
            DrawField("displayPrefabName", mainSerializedObject);

            createAssetNameFromKeyAsset = EditorGUILayout.Toggle("Create Asset Name From Key Asset", createAssetNameFromKeyAsset);
            if (!createAssetNameFromKeyAsset)
                nameField = EditorGUILayout.TextField("Asset Name", nameField);

            flags = (SIDRFlag)EditorGUILayout.EnumFlagsField("SIDR Flags", flags);
            idrsHolder = (IDRSHolder)EditorGUILayout.ObjectField("IDRS Holder", idrsHolder, typeof(IDRSHolder), false);
            ruleAmount = EditorGUILayout.IntField("Amount of Rules", ruleAmount);

            if (SimpleButton("Create SIDR"))
            {
                var result = CreateSIDR();
                if (result)
                {
                    Debug.Log($"Succesfully Creates SIDR {sidr.name}");
                    TryToClose();
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private bool CreateSIDR()
        {
            actualName = createAssetNameFromKeyAsset ? $"SIDR{sidr.keyAssetName}" : GetCorrectAssetName(nameField);
            try
            {
                if (string.IsNullOrEmpty(actualName))
                    throw ErrorShorthands.ThrowNullAssetName(nameof(nameField));

                sidr.name = actualName;

                if (flags.HasFlag(SIDRFlag.Survivors)) PopulateWithSurvivors();
                if (flags.HasFlag(SIDRFlag.Enemies)) PopulateWithEnemies();
                if (flags.HasFlag(SIDRFlag.Scavenger)) PopulateWithScavenger();

                if (idrsHolder)
                {
                    PopulateWithIDRSHolder();
                }

                Util.CreateAssetAtSelectionPath(sidr);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while creating SIDR: {e}");
                return false;
            }
        }

        private void PopulateWithSurvivors()
        {
            var list = sidr.singleItemDisplayRules;

            foreach (string name in FlagsToIDRS[SIDRFlag.Survivors])
            {
                list.Add(CreateSKARG(name, ruleAmount));
            }

            mainSerializedObject.Update();
        }

        private void PopulateWithEnemies()
        {
            var list = sidr.singleItemDisplayRules;

            foreach (string name in FlagsToIDRS[SIDRFlag.Enemies])
            {
                list.Add(CreateSKARG(name, ruleAmount));
            }

            mainSerializedObject.Update();
        }

        private void PopulateWithScavenger()
        {
            var list = sidr.singleItemDisplayRules;

            foreach (string name in FlagsToIDRS[SIDRFlag.Scavenger])
            {
                list.Add(CreateSKARG(name, ruleAmount));
            }

            mainSerializedObject.Update();
        }

        private void PopulateWithIDRSHolder()
        {
            var list = sidr.singleItemDisplayRules;

            foreach (IDRSHolder.IDRSStringAssetReference stringAssetRef in idrsHolder.IDRSStringAssetReferences)
            {
                if (stringAssetRef.IDRS)
                {
                    list.Add(CreateSKARG(stringAssetRef.IDRS.name, ruleAmount));
                }
                else
                {
                    list.Add(CreateSKARG(stringAssetRef.IDRSName, ruleAmount));
                }
            }

            mainSerializedObject.Update();
        }

        public static MSSingleItemDisplayRule.SingleKeyAssetRuleGroup CreateSKARG(string idrsName, int ruleAmount)
        {
            var ruleGroup = new MSSingleItemDisplayRule.SingleKeyAssetRuleGroup();
            ruleGroup.vanillaIDRSKey = idrsName;
            if (ruleGroup.itemDisplayRules == null)
                ruleGroup.itemDisplayRules = new List<MSSingleItemDisplayRule.SingleItemDisplayRule>();

            for (int i = 0; i < ruleAmount; i++)
            {
                ruleGroup.AddDisplayRule(new MSSingleItemDisplayRule.SingleItemDisplayRule());
            }

            return ruleGroup;
        }
    }
}*/