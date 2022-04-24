/*using RoR2EditorKit.Core.Inspectors;
using UnityEditor;
using UnityEngine;
namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(MSMonsterFamily))]
    public class MSMonsterFamilyInspector : ScriptableObjectInspector
    {
        bool showingBasics = true;
        bool showingMinibosses = true;
        bool showingChampions = true;
        public override void DrawCustomInspector()
        {
            Header("Family Selections", "Available monsters of this family");

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginVertical("box");
            showingBasics = EditorGUILayout.ToggleLeft("Family's Basic Monsters", showingBasics);
            if(showingBasics)
            {
                DrawField("basicMonsterWeight");
                EditorGUI.indentLevel++;
                DrawField("familyBasicMonsters");
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            showingMinibosses = EditorGUILayout.ToggleLeft("Family's Minibosses", showingMinibosses);
            if(showingMinibosses)
            {
                DrawField("miniBossWeight");
                EditorGUI.indentLevel++;
                DrawField("familyMiniBosses");
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            showingChampions = EditorGUILayout.ToggleLeft("Family's Champions", showingChampions);
            if(showingChampions)
            {
                DrawField("championWeight");
                EditorGUI.indentLevel++;
                DrawField("familyChampions");
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            Header("DirectorAPI Data", "Data passed over to R2API's DirectorAPI");

            EditorGUILayout.BeginVertical("box");
            DrawField("minStageCompletion");
            DrawField("maxStageCompletion");
            DrawField("familySelectionWeight");
            DrawField("selectionToken");
            DrawField("stages");
            EditorGUI.indentLevel++;
            DrawField("customStages");
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    }
}
*/