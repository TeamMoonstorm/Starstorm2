using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using RoR2;
using Moonstorm.Starstorm2.ScriptableObjects;
using RoR2EditorKit.PropertyDrawers;

namespace Moonstorm.Starstorm2.Editor
{
    [CustomPropertyDrawer(typeof(NemesisSpawnCard.StatModifier))]
    public class StatModifierPropertyDrawer : PropertyDrawer
    {
        public static float StandardPropertyHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        private static Type CharacterBodyType => typeof(CharacterBody);
        private List<string> validFields;
        private string[] validFieldsArray;
        public StatModifierPropertyDrawer()
        {
            validFields = CharacterBodyType.GetFields()
                .Where(fi => fi.IsPublic && !fi.IsStatic && fi.Name.Contains("base") && fi.FieldType != typeof(string))
                .Select(fi => fi.Name)
                .ToList();
            validFieldsArray = validFields.ToArray();
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fieldName = property.FindPropertyRelative("fieldName");
            var modifier = property.FindPropertyRelative("modifier");
            var modifierType = property.FindPropertyRelative("statModifierType");

            EditorGUI.BeginProperty(position, label, property);

            var foldoutRect = new Rect(position.x, position.y, position.width, 18);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
            if(property.isExpanded)
            {
                EditorGUI.indentLevel++;
                var num = validFields.IndexOf(fieldName.stringValue);
                var chosenIndex = num == -1 ? 0 : num;
                var fieldRect = new Rect(foldoutRect.x, foldoutRect.yMax, foldoutRect.width, foldoutRect.height);
                var newIndex = EditorGUI.Popup(fieldRect, "Target Base Stat", chosenIndex, validFieldsArray);
                fieldName.stringValue = validFields[newIndex];

                var modifierRect = new Rect(fieldRect.x, fieldRect.yMax, fieldRect.width / 1.5f, fieldRect.height);
                EditorGUI.PropertyField(modifierRect, modifier);

                var modifierTypeRect = new Rect(modifierRect.xMax, modifierRect.y, fieldRect.width - modifierRect.width, modifierRect.height);
                EditorGUI.PropertyField(modifierTypeRect, modifierType, new GUIContent());
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var basePropertyHeight = base.GetPropertyHeight(property, label);
            if(property.isExpanded)
            {
                basePropertyHeight *= 3;
            }
            return basePropertyHeight; 
        }
    }
    /*[CustomPropertyDrawer(typeof(NemesisSpawnCard.StatModifier))]
    public class StatModifierPropertyDrawer : EditorGUILayoutPropertyDrawer
    {
        private Type CharBodyType
        {
            get
            {
                return typeof(CharacterBody);
            }
        }

        protected override void DrawPropertyDrawer(SerializedProperty property)
        {
            var fieldName = property.FindPropertyRelative("fieldName");
            EditorGUILayout.PropertyField(fieldName);
            if (!DoesFieldExist(fieldName.stringValue))
            {
                EditorGUILayout.HelpBox($"There is no field with the name of {fieldName.stringValue}", MessageType.Error);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("statModifierType"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("modifier"));
                EditorGUI.indentLevel--;
                EditorGUILayout.EndHorizontal();
            }
        }

        public bool DoesFieldExist(string fieldName)
        {
            var fieldInfo = CharBodyType.GetField(fieldName);
            if(fieldInfo == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /*public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var fieldName = property.FindPropertyRelative("fieldName");

            var fieldNamePos = position;
            fieldNamePos.height /= 2.5f;
            EditorGUI.PropertyField(fieldNamePos, fieldName);
            if(!DoesFieldExist(fieldName.stringValue))
            {
                var helpRect = new Rect(position.x, position.y + position.height / 2.0f, position.width, position.height / 2f);

                string text = string.Empty;
                if(string.IsNullOrEmpty(fieldName.stringValue))
                {
                    text = "Please input a CharacterBody field name";
                }
                else
                {
                    text = $"There is no field in CharacterBody with the name of {fieldName.stringValue}";
                }

                EditorGUI.HelpBox(helpRect, text, MessageType.Error);
            }
            else
            {
                EditorGUI.indentLevel++;

                var modifierTypeRect = new Rect(position.x, position.y + position.height / 2, position.width / 2, position.height / 2.25f);
                EditorGUI.PropertyField(modifierTypeRect, property.FindPropertyRelative("statModifierType"), GUIContent.none);

                var modifierRect = new Rect(position.x + position.width / 2, position.y + position.height / 2, position.width / 2, position.height / 2.25f);
                EditorGUI.PropertyField(modifierRect, property.FindPropertyRelative("modifier"), GUIContent.none);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2.5f;
        }

    }*/
}
