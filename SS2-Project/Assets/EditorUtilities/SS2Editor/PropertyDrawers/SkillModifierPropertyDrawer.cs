using RoR2EditorKit.PropertyDrawers;
using Moonstorm.Starstorm2.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.Starstorm2.Editor
{
    [CustomPropertyDrawer(typeof(NemesisSpawnCard.SkillOverride))]
    public class SkillModifierPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var skillSlot = property.FindPropertyRelative("skillSlot");
            var skillDef = property.FindPropertyRelative("skillDef");

            EditorGUI.BeginProperty(position, label, property);

            var skillDefRect = new Rect(position.x, position.y, position.width / 1.5f, position.height);
            EditorGUI.PropertyField(skillDefRect, skillDef);

            var skillSlotRect = new Rect(skillDefRect.xMax, skillDefRect.position.y, position.width - skillDefRect.width, skillDefRect.height);
            EditorGUI.PropertyField(skillSlotRect, skillSlot, new GUIContent(), true);

            EditorGUI.EndProperty();
        }
    }
}