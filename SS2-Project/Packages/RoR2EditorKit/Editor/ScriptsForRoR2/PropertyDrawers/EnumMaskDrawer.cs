using RoR2;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using RoR2EditorKit.Core.PropertyDrawers;

namespace RoR2EditorKit.RoR2Related.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(EnumMaskAttribute))]
    public class EnumMaskDrawer : IMGUIPropertyDrawer
    {
        protected override void DrawCustomDrawer()
        {
            Enum targetEnum = GetBaseProperty<Enum>(property);
            FieldInfo field = GetField(property);

            string propName = property.name;
            if (string.IsNullOrEmpty(propName))
                propName = Regex.Replace(property.name, "([^^])([A-Z])", "$1 $2");
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(rect, label, property);

            Enum enumNew = EditorGUI.EnumFlagsField(rect, ObjectNames.NicifyVariableName(propName), targetEnum);

            EditorGUI.EndProperty();
            if (EditorGUI.EndChangeCheck())
            {
                fieldInfo.SetValue(property.serializedObject.targetObject, enumNew);
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.UpdateIfRequiredOrScript();
            }
        }

        static T GetBaseProperty<T>(SerializedProperty prop)
        {
            // Separate the steps it takes to get to this property
            string[] separatedPaths = prop.propertyPath.Split('.');

            // Go down to the root of this serialized property
            System.Object reflectionTarget = prop.serializedObject.targetObject as object;
            // Walk down the path to get the target object
            foreach (var path in separatedPaths)
            {
                FieldInfo fieldInfo = reflectionTarget.GetType().GetField(path);
                reflectionTarget = fieldInfo.GetValue(reflectionTarget);
            }
            return (T)reflectionTarget;
        }

        static FieldInfo GetField(SerializedProperty prop)
        {
            string[] separatedPaths = prop.propertyPath.Split('.');

            object reflectionTarget = prop.serializedObject.targetObject;
            FieldInfo field = null;
            foreach (var path in separatedPaths)
            {
                field = reflectionTarget.GetType().GetField(path);
            }
            return field;
        }
    }
}