using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using RoR2;
using Moonstorm.Starstorm2.ScriptableObjects;
using RoR2EditorKit.Core.PropertyDrawers;

namespace Moonstorm.SS2Editor.PropertyDrawers
{
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
