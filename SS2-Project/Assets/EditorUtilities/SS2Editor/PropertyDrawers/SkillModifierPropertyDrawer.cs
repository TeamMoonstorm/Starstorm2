using RoR2EditorKit.Core.PropertyDrawers;
using Moonstorm.Starstorm2.ScriptableObjects;
using UnityEditor;

namespace Moonstorm.SS2Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(NemesisSpawnCard.SkillOverride))]
    public class SkillModifierPropertyDrawer : EditorGUILayoutPropertyDrawer
    {
        protected override void DrawPropertyDrawer(SerializedProperty property)
        {
            EditorGUILayout.BeginHorizontal();
            DrawField("skillSlot");
            DrawField("skillDef");
            EditorGUILayout.EndHorizontal();
        }
    }
}