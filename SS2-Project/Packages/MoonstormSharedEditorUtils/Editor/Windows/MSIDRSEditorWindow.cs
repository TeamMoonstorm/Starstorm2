/*using RoR2EditorKit.Core.Windows;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public class MSIDRSEditorWindow : ExtendedEditorWindow
    {
        public Vector2 scrollPos = new Vector2();
        private string searchFilter;

        private SerializedProperty mainProperty;

        private SerializedProperty selectedKeyAssetProp;
        private string selectedKeyAssetPropPath;

        public string selectedRulePath;
        public SerializedProperty selectedRuleProperty;

        public bool switchingBool;
        private CreateMSIDRSWindow.MSIDRSFlags flags;
        private KeyAssetDisplayPairHolder KADPH;

        private MSIDRS msidrs;

        protected override void OnWindowOpened()
        {
            msidrs = mainSerializedObject.targetObject as MSIDRS;
        }
        private void OnGUI()
        {
            mainProperty = mainSerializedObject.FindProperty("MSUKeyAssetRuleGroup");

            DrawField("VanillaIDRSKey");
            searchFilter = EditorGUILayout.TextField("Search Filter", searchFilter);

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

            if (SwitchButton("Show Utilities", ref switchingBool))
            {
                flags = CreateMSIDRSWindow.MSIDRSFlags.None;
                KADPH = null;
            }
            if (switchingBool)
            {
                ShowUtilities();
            }

            var tuple = DrawCustomSidebar(mainProperty, scrollPos);
            scrollPos = tuple.Item1;

            if (tuple.Item2)
            {
                selectedRulePath = null;
                selectedRuleProperty = null;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            if (selectedKeyAssetProp != null)
            {
                DrawSelectedKeyAssetPropPanel();
            }
            else
            {
                EditorGUILayout.LabelField("Select a Key Asset Rule Group from the List.");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private void DrawSelectedKeyAssetPropPanel()
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150));

            DrawField(selectedKeyAssetProp.FindPropertyRelative("keyAssetName"), true);

            var rules = selectedKeyAssetProp.FindPropertyRelative("rules");

            DrawButtonSidebar(rules, ref selectedRulePath, ref selectedRuleProperty);

            GUILayout.Space(30);

            if (SimpleButton("Delete This KeyAsset Element"))
            {
                msidrs.MSUKeyAssetRuleGroup.RemoveAt(msidrs.MSUKeyAssetRuleGroup.FindIndex(x => x.keyAssetName == selectedKeyAssetProp.FindPropertyRelative("keyAssetName").stringValue));
                selectedKeyAssetProp = null;
                selectedKeyAssetPropPath = string.Empty;

                mainSerializedObject.Update();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.MaxWidth(300));

            if (selectedRuleProperty != null)
            {
                DrawProperties(selectedRuleProperty, true);
            }
            else
            {
                EditorGUILayout.LabelField("Select a Rule from the list.");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

        }

        private (Vector2, bool) DrawCustomSidebar(SerializedProperty property, Vector2 scrollPosition)
        {
            bool pressed = false;
            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(300));

            if (property.arraySize != 0)
            {
                List<SerializedProperty> propsInProperty = new List<SerializedProperty>();

                foreach (SerializedProperty prop in property)
                {
                    propsInProperty.Add(prop);
                }

                if (!string.IsNullOrEmpty(searchFilter))
                {
                    propsInProperty = propsInProperty.Where(prop => prop.displayName.ToLowerInvariant().Contains(searchFilter.ToLowerInvariant())).ToList();
                }
                propsInProperty
                    .ForEach(prop =>
                    {
                        string name = prop.displayName;
                        var rules = prop.FindPropertyRelative("rules");
                        foreach (SerializedProperty p in rules)
                        {
                            var values = p.FindPropertyRelative("IDPHValues");
                            if (string.IsNullOrEmpty(values.stringValue))
                            {
                                name += " | NULL IDPHValues";
                                break;
                            }
                        }
                        if (GUILayout.Button(name))
                        {
                            selectedKeyAssetPropPath = prop.propertyPath;
                            GUI.FocusControl(null);
                            pressed = true;
                        }
                    });
                if (!string.IsNullOrEmpty(selectedKeyAssetPropPath))
                {
                    selectedKeyAssetProp = mainSerializedObject.FindProperty(selectedKeyAssetPropPath);
                }

            }
            else
            {
                EditorGUILayout.LabelField($"Increase {property.name}'s Size.");
            }
            EditorGUILayout.EndScrollView();
            return (scrollPosition, pressed);
        }

        private void ShowUtilities()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Add Key Assets In Bulk");

            flags = (CreateMSIDRSWindow.MSIDRSFlags)EditorGUILayout.EnumFlagsField("Vanilla Key Assets", flags);
            KADPH = (KeyAssetDisplayPairHolder)EditorGUILayout.ObjectField("Key Asset Display Pair Holder", KADPH, typeof(KeyAssetDisplayPairHolder), false);

            if (SimpleButton("Add Selected Key Assets"))
            {
                AddSelectedKeyAssets();
                switchingBool = false;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void AddSelectedKeyAssets()
        {
            if (flags.HasFlag(CreateMSIDRSWindow.MSIDRSFlags.VanillaWhites)) PopulateWithMissingWhites();
            if (flags.HasFlag(CreateMSIDRSWindow.MSIDRSFlags.VanillaGreens)) PopulateWithMissingGreens();
            if (flags.HasFlag(CreateMSIDRSWindow.MSIDRSFlags.VanillaReds)) PopulateWithMissingReds();
            if (flags.HasFlag(CreateMSIDRSWindow.MSIDRSFlags.VanillaYellows)) PopulateWithMissingYellows();
            if (flags.HasFlag(CreateMSIDRSWindow.MSIDRSFlags.VanillaLunars)) PopulateWithMissingLunars();
            if (flags.HasFlag(CreateMSIDRSWindow.MSIDRSFlags.VanillaEquipments)) PopulateWithMissingEquipments();
            if (flags.HasFlag(CreateMSIDRSWindow.MSIDRSFlags.VanillaLunarEquipments)) PopulateWithMissingVanillaLunarEquipments();
            if (flags.HasFlag(CreateMSIDRSWindow.MSIDRSFlags.VanillaElites)) PopulateWithMissingVanillaElites();

            if (KADPH)
            {
                PopulateWithMissingKADPH();
            }

            mainSerializedObject.Update();
        }

        private void PopulateWithMissingWhites()
        {
            foreach (var (keyAssset, display, ruleCount) in CreateMSIDRSWindow.FlagsToItemLists[CreateMSIDRSWindow.MSIDRSFlags.VanillaWhites])
            {
                if (!msidrs.MSUKeyAssetRuleGroup.Any(x => x.keyAssetName == keyAssset))
                {
                    msidrs.MSUKeyAssetRuleGroup.Add(CreateMSIDRSWindow.CreateKARG(keyAssset, display, ruleCount));
                }
            }
            if (!msidrs.MSUKeyAssetRuleGroup.Any(x => x.keyAssetName == "Tooth"))
            {
                var toothGroup = new MSIDRS.KeyAssetRuleGroup();
                toothGroup.keyAssetName = "Tooth";
                toothGroup.AddDisplayRule(new MSIDRS.ItemDisplayRule { displayPrefabName = "DisplayToothNecklaceDecal" });
                toothGroup.AddDisplayRule(new MSIDRS.ItemDisplayRule { displayPrefabName = "DisplayToothMeshLarge" });
                toothGroup.AddDisplayRule(new MSIDRS.ItemDisplayRule { displayPrefabName = "DisplayToothMeshSmall1" });
                toothGroup.AddDisplayRule(new MSIDRS.ItemDisplayRule { displayPrefabName = "DisplayToothMeshSmall1" });
                toothGroup.AddDisplayRule(new MSIDRS.ItemDisplayRule { displayPrefabName = "DisplayToothMeshSmall2" });
                toothGroup.AddDisplayRule(new MSIDRS.ItemDisplayRule { displayPrefabName = "DisplayToothMeshSmall2" });

                msidrs.MSUKeyAssetRuleGroup.Add(toothGroup);
            }
        }

        private void PopulateWithMissingGreens()
        {
            foreach (var (keyAssset, display, ruleCount) in CreateMSIDRSWindow.FlagsToItemLists[CreateMSIDRSWindow.MSIDRSFlags.VanillaGreens])
            {
                if (!msidrs.MSUKeyAssetRuleGroup.Any(x => x.keyAssetName == keyAssset))
                {
                    msidrs.MSUKeyAssetRuleGroup.Add(CreateMSIDRSWindow.CreateKARG(keyAssset, display, ruleCount));
                }
            }
        }

        private void PopulateWithMissingReds()
        {
            foreach (var (keyAssset, display, ruleCount) in CreateMSIDRSWindow.FlagsToItemLists[CreateMSIDRSWindow.MSIDRSFlags.VanillaReds])
            {
                if (!msidrs.MSUKeyAssetRuleGroup.Any(x => x.keyAssetName == keyAssset))
                {
                    msidrs.MSUKeyAssetRuleGroup.Add(CreateMSIDRSWindow.CreateKARG(keyAssset, display, ruleCount));
                }
            }
        }

        private void PopulateWithMissingYellows()
        {
            foreach (var (keyAssset, display, ruleCount) in CreateMSIDRSWindow.FlagsToItemLists[CreateMSIDRSWindow.MSIDRSFlags.VanillaYellows])
            {
                if (!msidrs.MSUKeyAssetRuleGroup.Any(x => x.keyAssetName == keyAssset))
                {
                    msidrs.MSUKeyAssetRuleGroup.Add(CreateMSIDRSWindow.CreateKARG(keyAssset, display, ruleCount));
                }
            }
        }

        private void PopulateWithMissingLunars()
        {
            foreach (var (keyAssset, display, ruleCount) in CreateMSIDRSWindow.FlagsToItemLists[CreateMSIDRSWindow.MSIDRSFlags.VanillaLunars])
            {
                if (!msidrs.MSUKeyAssetRuleGroup.Any(x => x.keyAssetName == keyAssset))
                {
                    msidrs.MSUKeyAssetRuleGroup.Add(CreateMSIDRSWindow.CreateKARG(keyAssset, display, ruleCount));
                }
            }
        }

        private void PopulateWithMissingEquipments()
        {
            foreach (var (keyAssset, display, ruleCount) in CreateMSIDRSWindow.FlagsToItemLists[CreateMSIDRSWindow.MSIDRSFlags.VanillaEquipments])
            {
                if (!msidrs.MSUKeyAssetRuleGroup.Any(x => x.keyAssetName == keyAssset))
                {
                    msidrs.MSUKeyAssetRuleGroup.Add(CreateMSIDRSWindow.CreateKARG(keyAssset, display, ruleCount));
                }
            }
        }

        private void PopulateWithMissingVanillaLunarEquipments()
        {
            foreach (var (keyAssset, display, ruleCount) in CreateMSIDRSWindow.FlagsToItemLists[CreateMSIDRSWindow.MSIDRSFlags.VanillaLunarEquipments])
            {
                if (!msidrs.MSUKeyAssetRuleGroup.Any(x => x.keyAssetName == keyAssset))
                {
                    msidrs.MSUKeyAssetRuleGroup.Add(CreateMSIDRSWindow.CreateKARG(keyAssset, display, ruleCount));
                }
            }
        }

        private void PopulateWithMissingVanillaElites()
        {
            foreach (var (keyAssset, display, ruleCount) in CreateMSIDRSWindow.FlagsToItemLists[CreateMSIDRSWindow.MSIDRSFlags.VanillaElites])
            {
                if (!msidrs.MSUKeyAssetRuleGroup.Any(x => x.keyAssetName == keyAssset))
                {
                    msidrs.MSUKeyAssetRuleGroup.Add(CreateMSIDRSWindow.CreateKARG(keyAssset, display, ruleCount));
                }
            }
        }

        private void PopulateWithMissingKADPH()
        {
            foreach (var keyAssetDisplayPair in KADPH.KeyAssetDisplayPairs)
            {
                if (msidrs.MSUKeyAssetRuleGroup.Any(x => x.keyAssetName == keyAssetDisplayPair.keyAsset.name))
                {
                    var keyAssetName = keyAssetDisplayPair.keyAsset.name;
                    var constructedName = $"{keyAssetName}DisplayPrefab_0";
                    msidrs.MSUKeyAssetRuleGroup.Add(CreateMSIDRSWindow.CreateKARG(keyAssetName, constructedName, 1));
                }
            }
        }
    }
}*/