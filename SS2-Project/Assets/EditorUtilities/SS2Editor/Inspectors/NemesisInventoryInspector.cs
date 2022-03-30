/*using Moonstorm.Starstorm2.ScriptableObjects;
using RoR2EditorKit.Core.Inspectors;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Moonstorm.SS2Editor.Inspectors
{
    [CustomEditor(typeof(NemesisInventory))]
    public class NemesisInventoryInspector : ExtendedInspector
    {
        int arraySize;
        string itemDefName;
        string _itemDefName;
        List<SerializedProperty> propsInProperty = new List<SerializedProperty>();

        public override void DrawCustomInspector()
        {
            Header("Nemesis Inventory", "A list of items that can be given to the nemesis once it spawns, used in the Nemesis Spawn Card");

            itemDefName = EditorGUILayout.TextField("Searchbar", itemDefName);

            EditorGUILayout.Space();

            var property = serializedObject.FindProperty("nemesisItems");
            property.arraySize = EditorGUILayout.DelayedIntField("Array Size", property.arraySize);

            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;

            if(itemDefName != _itemDefName)
            {
                UpdateList(property);
            }
            propsInProperty.ForEach(prop =>
            {
                DrawNemesisItemStringProperty(prop);
            });


            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void UpdateList(SerializedProperty property)
        {
            propsInProperty.Clear();

            foreach (SerializedProperty prop in property)
            {
                propsInProperty.Add(prop);
            }

            if (!string.IsNullOrEmpty(itemDefName))
            {
                propsInProperty = propsInProperty.Where(prop => prop.displayName.ToLowerInvariant().Contains(itemDefName.ToLowerInvariant())).ToList();
            }
            _itemDefName = itemDefName;
        }
        private void DrawNemesisItemStringProperty(SerializedProperty property)
        {
            DrawField(property, "chanceForApplying");
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("box");
            DrawField(property, "itemDefName");
            EditorGUILayout.BeginHorizontal();
            DrawField(property, "minStacks");
            DrawField(property, "maxStacks");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
    }
}*/