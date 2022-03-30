/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Common;
using UnityEditor;
using Moonstorm.Starstorm2.ScriptableObjects;

namespace Moonstorm.SS2Editor.Inspectors
{
    [CustomEditor(typeof(NemesisSpawnCard))]
    public class NemesisSpawnCardInspector : ExtendedInspector
    {
        public override void DrawCustomInspector()
        {
            Header("Character Spawn Card", "Section pertaining to the Spawn Card");

            EditorGUILayout.BeginVertical("box");
            DrawField("prefab");
            DrawField("sendOverNetwork");
            DrawField("hullSize");
            DrawField("nodeGraphType");
            DrawField("requiredFlags");
            DrawField("forbiddenFlags");
            DrawField("occupyPosition");
            DrawField("eliteRules");
            EditorGUILayout.EndVertical();

            Header("Nemesis Data", "Data passed over to the EventDirector to handle the loadout and the effects of the nemesis");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nemesisInventory"), new GUIContent("Nemesis Inventory", "A Nemesis Inventory ScriptableObject to store what items the nemesis can obtain"));
            Header("Initial Entity State Override", "If enabled, the initial body entity state will be overwritten by the one specified");
            var useState = serializedObject.FindProperty("useOverrideState");
            DrawField(useState);
            if(useState.boolValue)
            {
                EditorGUI.indentLevel++;
                DrawField("overrideSpawnState");
                EditorGUI.indentLevel--;
            }

            Header("Stat and Skill Modifiers", "Modifiers that are applied to the CharacterBody stats, and skills that are overwritten when the nemesis spawns.");
            EditorGUI.indentLevel++;
            DrawField("statModifiers");
            DrawField("skillOverrides");
            EditorGUI.indentLevel--;

            Header("Visual Effects", "A VisualEffect that's instantiated on the Nemesis, Gets destroyed when the nemesis dies.");
            var visualEffect = serializedObject.FindProperty("visualEffect");
            DrawField(visualEffect);
            if(visualEffect.objectReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                DrawField("childName");
                EditorGUI.indentLevel--;
            }

            Header("Defeat Reward", "The Item that the nemesis drops when killed.");
            DrawField("itemDef");
            EditorGUILayout.EndVertical();
        }
    }
}
*/