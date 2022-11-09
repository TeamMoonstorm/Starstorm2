using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Moonstorm.Starstorm2.ScriptableObjects;
using Moonstorm.AddressableAssets;
using RoR2;

namespace Moonstorm.Starstorm2.Editor
{
    [CustomEditor(typeof(NemesisInventory))]
    public class NemesisInventoryEditor : UnityEditor.Editor
    {
        private float difficultyCoefficient = 2f;
        private float timeElapsedInMinutes = 30;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Item Giving Simulation", EditorStyles.boldLabel);
            difficultyCoefficient = EditorGUILayout.FloatField("Difficulty Coefficient", difficultyCoefficient);
            timeElapsedInMinutes = EditorGUILayout.FloatField("Time Elapsed in Minutes", timeElapsedInMinutes);
            if(GUILayout.Button("Simulate"))
            {
                Simulate();
            }
            EditorGUILayout.EndVertical();
        }

        private void Simulate()
        {
            NemesisInventory inventory = (NemesisInventory)target;

            WeightedSelection<Bruh> bruhSelection = CreateBruhSelection(inventory.nemesisInventory);

            float itemsPerMin = 0.5f;
            float itemsDev = 0.2f;
            int runTimeMins = Mathf.CeilToInt((timeElapsedInMinutes * 60) / 60);
            int numItems = Mathf.CeilToInt(runTimeMins * itemsPerMin * difficultyCoefficient);

            numItems += Mathf.CeilToInt(numItems * UnityEngine.Random.Range(-itemsDev, itemsDev));

            Dictionary<string, int> simulatedInventory = new Dictionary<string, int>();
            for(int i = 0; i < numItems; i++)
            {
                var selected = bruhSelection.Evaluate(UnityEngine.Random.value);
                if(!simulatedInventory.ContainsKey(selected.itemName))
                {
                    simulatedInventory.Add(selected.itemName, UnityEngine.Random.Range(selected.minStacks, selected.maxStacks));
                }
            }

            List<string> log = new List<string>();
            foreach(var kvp in simulatedInventory)
            {
                log.Add($"{kvp.Value}x {kvp.Key}");
            }
            Debug.Log($"The following items have been obtained from simulation:\n{string.Join("\n", log)}");
        }

        private WeightedSelection<Bruh> CreateBruhSelection(List<NemesisInventory.NemesisAddressableItemReference> items)
        {
            WeightedSelection<Bruh> bruhSelection = new WeightedSelection<Bruh>();
            foreach (var itemRef in items)
            {
                var itemName = GetItemName(itemRef.itemDef);
                if (string.IsNullOrEmpty(itemName))
                    continue;

                WeightedSelection<Bruh>.ChoiceInfo choiceInfo = new WeightedSelection<Bruh>.ChoiceInfo
                {
                    weight = itemRef.weight,
                    value = new Bruh
                    {
                        itemName = itemName,
                        maxStacks = itemRef.maxStacks,
                        minStacks = itemRef.minStacks
                    }
                };
                bruhSelection.AddChoice(choiceInfo);
            }
            return bruhSelection;
        }
        private string GetItemName(AddressableItemDef itemDef)
        {
            if(string.IsNullOrEmpty(itemDef.address))
            {
                var protectedAssetField = (ItemDef)typeof(AddressableItemDef).GetField("asset").GetValue(itemDef);
                return protectedAssetField ? protectedAssetField.name : String.Empty;
            }

            var isAddres = itemDef.address.StartsWith("RoR2");
            if(!isAddres)
            {
                return itemDef.address;
            }

            var split = itemDef.address.Split('/');
            return split.LastOrDefault();
        }
        private struct Bruh
        {
            public string itemName;
            public int maxStacks;
            public int minStacks;
        }
    }
}