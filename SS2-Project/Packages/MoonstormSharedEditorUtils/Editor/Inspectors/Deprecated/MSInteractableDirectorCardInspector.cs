/*using RoR2EditorKit.Core.Inspectors;
using UnityEditor;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(MSInteractableDirectorCard))]
    public class MSInteractableDirectorCardInspector : ScriptableObjectInspector
    {
        public override void DrawCustomInspector()
        {
            Header("Interactable Spawn Card", "Section pertaining to the Interactable Spawn Card");

            EditorGUILayout.BeginVertical("box");
            DrawField("prefab");
            DrawField("sendOverNetwork");
            DrawField("hullSize");
            DrawField("requiredFlags");
            DrawField("forbiddenFlags");
            DrawField("directorCreditCost");
            DrawField("occupyPosition");
            DrawField("orientToFloor");
            DrawField("slightlyRandomizeOrientation");
            DrawField("skipSpawnWhenSacrificeArtifactEnabled");
            EditorGUILayout.EndVertical();

            Header("DirectorAPI Data", "Data passed over to R2API's DirectorAPI");

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginVertical("box");
            Header("Director Card Data", "Data of the Director Card associated to this Interactable Spawn Card");
            var dCard = serializedObject.FindProperty("directorCard");
            DrawField(dCard, "selectionWeight");
            DrawField(dCard, "minimumStageCompletions");
            DrawField(dCard, "requiredUnlockableDef");
            DrawField(dCard, "forbiddenUnlockableDef");
            EditorGUILayout.EndVertical();

            DrawField("interactableCategory");
            DrawField("stages");
            EditorGUI.indentLevel++;
            DrawField("customStages");
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    }
}
*/