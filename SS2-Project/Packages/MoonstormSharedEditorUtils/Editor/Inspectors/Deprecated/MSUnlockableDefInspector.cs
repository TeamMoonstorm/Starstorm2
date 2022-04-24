/*using RoR2EditorKit.Core.Inspectors;
using UnityEditor;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(MSUnlockableDef))]
    public class MSUnlockableDefInspector : ScriptableObjectInspector
    {
        public override void DrawCustomInspector()
        {
            Header("UnlockableDef", "Section pertaining to the UnlockableDef itself");

            EditorGUILayout.BeginVertical("box");
            DrawField("nameToken");
            DrawField("hidden");
            EditorGUILayout.EndVertical();

            Header("Achievement Def", "Section pertaining to the AchievementDef tied to this UnlockableDef");

            EditorGUILayout.BeginVertical("box");

            DrawField("achievementCondition");
            var serverTrackedProp = serializedObject.FindProperty("serverTracked");
            EditorGUILayout.PropertyField(serverTrackedProp);
            if (serverTrackedProp.boolValue)
            {
                EditorGUI.indentLevel++;
                DrawField("baseServerAchievement");
                EditorGUI.indentLevel--;
            }
            DrawField("achievementNameToken");
            DrawField("achievementDescToken");
            DrawField("achievedIcon");
            DrawField("unachievedIcon");

            EditorGUILayout.BeginVertical("box");

            Header("Prerequisite Unlockable", "Wether this unlockable requires another unlockable to be unlocked first. can be left null.");
            DrawField("prerequisiteAchievement");

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }
    }
}
*/