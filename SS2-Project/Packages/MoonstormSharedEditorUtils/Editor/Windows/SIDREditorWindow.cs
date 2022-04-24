/*using RoR2EditorKit.Core.Windows;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public class SIDREditorWindow : ExtendedEditorWindow
    {
        private MSSingleItemDisplayRule sidr;
        public Vector2 scrollPos = new Vector2();
        private string searchFilter;

        private SerializedProperty mainProp;

        private SerializedProperty selectedIDRSProperty;
        private string selectedIDRSPropPath;

        private string selectedRulesPath;
        private SerializedProperty selectedRulesProperty;

        private bool switchingBool;
        private CreateSIDRSWindow.SIDRFlag flags;
        private IDRSHolder idrsHolder;

        protected override void OnWindowOpened()
        {
            sidr = mainSerializedObject.targetObject as MSSingleItemDisplayRule;
        }
        private void OnGUI()
        {
            mainProp = mainSerializedObject.FindProperty("singleItemDisplayRules");

            DrawField(mainSerializedObject.FindProperty("keyAssetName"), true);
            DrawField(mainSerializedObject.FindProperty("displayPrefabName"), true);
            searchFilter = EditorGUILayout.TextField("Search Filter", searchFilter);

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

            if (SwitchButton("Show Utilities", ref switchingBool))
            {
                flags = CreateSIDRSWindow.SIDRFlag.None;
                idrsHolder = null;
            }

            if (switchingBool)
            {
                ShowUtilities();
            }



            var tuple = DrawCustomSidebar(mainProp, scrollPos);
            scrollPos = tuple.Item1;

            if (tuple.Item2)
            {
                selectedRulesPath = null;
                selectedRulesProperty = null;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            if (selectedIDRSProperty != null)
            {
                DrawSelectedSingleKeyAssetPropPanel();
            }
            else
            {
                EditorGUILayout.LabelField("Select an item from the List.");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private void DrawSelectedSingleKeyAssetPropPanel()
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150));

            DrawField(selectedIDRSProperty.FindPropertyRelative("vanillaIDRSKey"), true);

            var rules = selectedIDRSProperty.FindPropertyRelative("itemDisplayRules");

            DrawButtonSidebar(rules, ref selectedRulesPath, ref selectedRulesProperty);

            GUILayout.Space(30);

            if (SimpleButton("Delete This IDRS Element"))
            {
                sidr.singleItemDisplayRules.RemoveAt(sidr.singleItemDisplayRules.FindIndex(x => x.vanillaIDRSKey == selectedIDRSProperty.FindPropertyRelative("vanillaIDRSKey").stringValue));
                selectedIDRSProperty = null;
                selectedIDRSPropPath = null;

                mainSerializedObject.Update();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.MaxWidth(300));

            if (selectedRulesProperty != null)
            {
                DrawProperties(selectedRulesProperty, true);
            }
            else
            {
                EditorGUILayout.LabelField("Select a Rule from the List.");
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
                        var rules = prop.FindPropertyRelative("itemDisplayRules");
                        foreach (SerializedProperty p in rules)
                        {
                            var values = p.FindPropertyRelative("IDPHValues");
                            if (string.IsNullOrEmpty(values.stringValue))
                            {
                                name += " | NULL IDPHValues";
                                break;
                            }
                        }
                        if (SimpleButton(name))
                        {
                            selectedIDRSPropPath = prop.propertyPath;
                            GUI.FocusControl(null);
                            pressed = true;
                        }
                    });
                if (!string.IsNullOrEmpty(selectedIDRSPropPath))
                {
                    selectedIDRSProperty = mainSerializedObject.FindProperty(selectedIDRSPropPath);
                }
            }
            else
            {
                EditorGUILayout.LabelField($"Increase {property.name}'s Size");
            }
            EditorGUILayout.EndScrollView();

            return (scrollPos, pressed);
        }

        private void ShowUtilities()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Add IDRS in Bulk");

            flags = (CreateSIDRSWindow.SIDRFlag)EditorGUILayout.EnumFlagsField("Vanilla IDRS", flags);
            idrsHolder = (IDRSHolder)EditorGUILayout.ObjectField("IDRS Holder", idrsHolder, typeof(IDRSHolder), false);

            if (SimpleButton("Add Selected IDRS"))
            {
                AddSelectedIDRS();
                switchingBool = false;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void AddSelectedIDRS()
        {
            if (flags.HasFlag(CreateSIDRSWindow.SIDRFlag.Survivors)) PopulateWithMissingSurvivors();
            if (flags.HasFlag(CreateSIDRSWindow.SIDRFlag.Enemies)) PopulateWithMissingEnemies();
            if (flags.HasFlag(CreateSIDRSWindow.SIDRFlag.Scavenger)) PopulateWithMissingScav();

            if (idrsHolder)
            {
                PopulateWithMissingIDRSHolder();
            }

            mainSerializedObject.Update();
        }

        private void PopulateWithMissingSurvivors()
        {
            foreach (string name in CreateSIDRSWindow.FlagsToIDRS[CreateSIDRSWindow.SIDRFlag.Survivors])
            {
                if (!sidr.singleItemDisplayRules.Any(x => x.vanillaIDRSKey == name))
                {
                    sidr.singleItemDisplayRules.Add(CreateSIDRSWindow.CreateSKARG(name, 1));
                }
            }
        }

        private void PopulateWithMissingEnemies()
        {
            foreach (string name in CreateSIDRSWindow.FlagsToIDRS[CreateSIDRSWindow.SIDRFlag.Enemies])
            {
                if (!sidr.singleItemDisplayRules.Any(x => x.vanillaIDRSKey == name))
                {
                    sidr.singleItemDisplayRules.Add(CreateSIDRSWindow.CreateSKARG(name, 1));
                }
            }
        }

        private void PopulateWithMissingScav()
        {
            foreach (string name in CreateSIDRSWindow.FlagsToIDRS[CreateSIDRSWindow.SIDRFlag.Scavenger])
            {
                if (!sidr.singleItemDisplayRules.Any(x => x.vanillaIDRSKey == name))
                {
                    sidr.singleItemDisplayRules.Add(CreateSIDRSWindow.CreateSKARG(name, 1));
                }
            }
        }

        private void PopulateWithMissingIDRSHolder()
        {
            foreach (IDRSHolder.IDRSStringAssetReference stringAssetRef in idrsHolder.IDRSStringAssetReferences)
            {
                if (stringAssetRef.IDRS && !sidr.singleItemDisplayRules.Any(x => x.vanillaIDRSKey == stringAssetRef.IDRS.name))
                {
                    sidr.singleItemDisplayRules.Add(CreateSIDRSWindow.CreateSKARG(stringAssetRef.IDRS.name, 1));
                }
                else if (!string.IsNullOrEmpty(stringAssetRef.IDRSName) && !sidr.singleItemDisplayRules.Any(x => x.vanillaIDRSKey == stringAssetRef.IDRSName))
                {
                    sidr.singleItemDisplayRules.Add(CreateSIDRSWindow.CreateSKARG(stringAssetRef.IDRSName, 1));
                }
            }
        }
    }
}*/