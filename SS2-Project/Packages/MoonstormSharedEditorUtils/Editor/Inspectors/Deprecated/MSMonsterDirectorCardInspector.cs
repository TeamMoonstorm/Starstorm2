/*using RoR2EditorKit.Core.Inspectors;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(MSMonsterDirectorCard))]
    public class MSMonsterDirectorCardInspector : ScriptableObjectInspector
    {
        bool showingInventory = true;
        public override void DrawCustomInspector()
        {
            Header("Monster Spawn Card", "Section pertaining to the Monster's Character Spawn Card");

            EditorGUILayout.BeginVertical("box");
            DrawField("prefab");
            DrawField("sendOverNetwork");
            DrawField("hullSize");
            DrawField("nodeGraphType");
            DrawField("requiredFlags");
            DrawField("forbiddenFlags");
            DrawField("directorCreditCost");
            DrawField("occupyPosition");
            DrawField("eliteRules");
            DrawField("noElites");
            DrawField("forbiddenAsBoss");
            DrawField("_loadout");
            showingInventory = EditorGUILayout.Toggle("Show Inventory", showingInventory);
            if (showingInventory)
            {
                //EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical("box");
                DrawField("equipmentToGrant");
                DrawField("itemsToGrant");
                EditorGUILayout.EndVertical();
                //EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            Header("DirectorAPI Data", "Data passed over to R2API's DirectorAPI");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginVertical("box");
            Header("Director Card Data", "Data of the Director Card associated to this Monster Spawn Card");
            var dCard = serializedObject.FindProperty("directorCard");
            DrawField(dCard, "selectionWeight");
            DrawField(dCard, "spawnDistance");
            DrawField(dCard, "preventOverhead");
            DrawField(dCard, "minimumStageCompletions");
            DrawField(dCard, "requiredUnlockableDef");
            DrawField(dCard, "forbiddenUnlockableDef");
            EditorGUILayout.EndVertical();


            DrawField("monsterCategory");
            DrawField("stages");
            EditorGUI.indentLevel++;
            DrawField("customStages");
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    }
}
*/