/*using RoR2;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core;
using RoR2EditorKit.Core.Windows;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Util = RoR2EditorKit.Util;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public class CreateIDRSHolderWindow : CreateRoR2ScriptableObjectWindow<IDRSHolder>
    {
        [Flags]
        public enum ModdedBodiesFlags : short
        {
            None = 0,
            LostInTransitEnemies = 1,
            RoR1ReturningSurvivors = 2,
            Starstorm2 = 4,
            Manipulator = 8,
            HereticUnchained = 16,
            RiskOfRuina = 32,
            Paladin = 64,
        }

        public IDRSHolder idrsHolder;
        public List<ItemDisplayRuleSet> idrs = new List<ItemDisplayRuleSet>();

        private ModdedBodiesFlags flags;

        [MenuItem(Constants.RoR2EditorKitContextRoot + "MoonstormSharedUtils/Editor/IDRSHolder", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateIDRSHolderWindow>(null, "Create IDRSHolder");
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            idrsHolder = (IDRSHolder)ScriptableObject;
            mainSerializedObject = new SerializedObject(idrsHolder);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            nameField = EditorGUILayout.TextField("Asset Name", nameField);
            flags = (ModdedBodiesFlags)EditorGUILayout.EnumFlagsField("Modded Bodies", flags);

            int newCount = Mathf.Max(0, EditorGUILayout.DelayedIntField("IDRS List Size", idrs.Count));
            while (newCount < idrs.Count)
                idrs.RemoveAt(idrs.Count - 1);
            while (newCount > idrs.Count)
                idrs.Add(null);

            EditorGUI.indentLevel++;
            for (int i = 0; i < idrs.Count; i++)
            {
                idrs[i] = (ItemDisplayRuleSet)EditorGUILayout.ObjectField($"Element {i}", idrs[i], typeof(ItemDisplayRuleSet), false);
            }
            EditorGUI.indentLevel--;

            if (SimpleButton("Create IDRSHolder"))
            {
                var result = CreateIDRSHolder();
                if (result)
                {
                    Debug.Log($"Succesfully Created IDRSHolder {idrsHolder.name}");
                    TryToClose();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private bool CreateIDRSHolder()
        {
            try
            {
                if (string.IsNullOrEmpty(nameField))
                    throw ErrorShorthands.ThrowNullAssetName(nameof(nameField));

                actualName = GetCorrectAssetName(nameField);

                idrsHolder.name = actualName;

                if (flags.HasFlag(ModdedBodiesFlags.LostInTransitEnemies)) PopulateWithLostInTransitEnemies();
                if (flags.HasFlag(ModdedBodiesFlags.RoR1ReturningSurvivors)) PopulateWithRoR1ReturningSurvivors();
                if (flags.HasFlag(ModdedBodiesFlags.Starstorm2)) PopulateWithStarstorm2();
                if (flags.HasFlag(ModdedBodiesFlags.Manipulator)) PopulateWithManipulator();
                if (flags.HasFlag(ModdedBodiesFlags.RiskOfRuina)) PopulateWithRiskOfRuina();
                if (flags.HasFlag(ModdedBodiesFlags.Paladin)) PopulateWithPaladin();

                if (idrs.Count > 0)
                {
                    PopulateWithIDRS();
                }

                Util.CreateAssetAtSelectionPath(idrsHolder);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while creating IDRSHolder: {e}");
                return false;
            }
        }

        private void PopulateWithLostInTransitEnemies()
        {
            var list = idrsHolder.IDRSStringAssetReferences;

            list.Add(CreateIDRSSAR("idrsMoffeinAncientWisp"));
            list.Add(CreateIDRSSAR("idrsMoffeinClayMan"));
            list.Add(CreateIDRSSAR("idrsNebbyArchWisp"));

            mainSerializedObject.Update();
        }

        private void PopulateWithRoR1ReturningSurvivors()
        {
            var list = idrsHolder.IDRSStringAssetReferences;

            list.Add(CreateIDRSSAR("idrsChef"));
            list.Add(CreateIDRSSAR("idrsHand"));
            list.Add(CreateIDRSSAR("idrsMiner"));
            list.Add(CreateIDRSSAR("idrsNemesisEnforcer"));
            list.Add(CreateIDRSSAR("idrsSniperClassic"));

            mainSerializedObject.Update();
        }

        private void PopulateWithStarstorm2()
        {
            var list = idrsHolder.IDRSStringAssetReferences;

            list.Add(CreateIDRSSAR("idrsExecutioner"));
            list.Add(CreateIDRSSAR("idrsNemmando"));
            list.Add(CreateIDRSSAR("idrsSecurityDrone"));

            mainSerializedObject.Update();
        }

        private void PopulateWithManipulator()
        {
            var list = idrsHolder.IDRSStringAssetReferences;

            list.Add(CreateIDRSSAR("idrsManipulator"));

            mainSerializedObject.Update();
        }

        private void PopulateWithRiskOfRuina()
        {
            var list = idrsHolder.IDRSStringAssetReferences;

            list.Add(CreateIDRSSAR("idrsArbiter"));
            list.Add(CreateIDRSSAR("idrsRedMist"));

            mainSerializedObject.Update();
        }

        private void PopulateWithPaladin()
        {
            var list = idrsHolder.IDRSStringAssetReferences;

            list.Add(CreateIDRSSAR("idrsPaladin"));

            mainSerializedObject.Update();
        }

        private void PopulateWithIDRS()
        {
            var list = idrsHolder.IDRSStringAssetReferences;
            foreach (ItemDisplayRuleSet idrs in idrs)
            {
                list.Add(new IDRSHolder.IDRSStringAssetReference { IDRS = idrs });
            }
            mainSerializedObject.Update();
        }

        private IDRSHolder.IDRSStringAssetReference CreateIDRSSAR(string idrsName)
        {
            var stringAssetRef = new IDRSHolder.IDRSStringAssetReference();
            stringAssetRef.IDRSName = idrsName;
            return stringAssetRef;
        }
    }
}*/