using HG.GeneralSerializer;
using RoR2;
using RoR2EditorKit.Core.Inspectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(EntityStateConfiguration))]
    public class EntityStateConfigurationInspector : ScriptableObjectInspector<EntityStateConfiguration>
    {
        private delegate object FieldDrawHandler(FieldInfo fieldInfo, object value);
        private static readonly Dictionary<Type, FieldDrawHandler> typeDrawers = new Dictionary<Type, FieldDrawHandler>
        {
            [typeof(bool)] = (fieldInfo, value) => EditorGUILayout.Toggle(ObjectNames.NicifyVariableName(fieldInfo.Name), (bool)value),
            [typeof(long)] = (fieldInfo, value) => EditorGUILayout.LongField(ObjectNames.NicifyVariableName(fieldInfo.Name), (long)value),
            [typeof(int)] = (fieldInfo, value) => EditorGUILayout.IntField(ObjectNames.NicifyVariableName(fieldInfo.Name), (int)value),
            [typeof(float)] = (fieldInfo, value) => EditorGUILayout.FloatField(ObjectNames.NicifyVariableName(fieldInfo.Name), (float)value),
            [typeof(double)] = (fieldInfo, value) => EditorGUILayout.DoubleField(ObjectNames.NicifyVariableName(fieldInfo.Name), (double)value),
            [typeof(string)] = (fieldInfo, value) => EditorGUILayout.TextField(ObjectNames.NicifyVariableName(fieldInfo.Name), (string)value),
            [typeof(Vector2)] = (fieldInfo, value) => EditorGUILayout.Vector2Field(ObjectNames.NicifyVariableName(fieldInfo.Name), (Vector2)value),
            [typeof(Vector3)] = (fieldInfo, value) => EditorGUILayout.Vector3Field(ObjectNames.NicifyVariableName(fieldInfo.Name), (Vector3)value),
            [typeof(Color)] = (fieldInfo, value) => EditorGUILayout.ColorField(ObjectNames.NicifyVariableName(fieldInfo.Name), (Color)value),
            [typeof(Color32)] = (fieldInfo, value) => (Color32)EditorGUILayout.ColorField(ObjectNames.NicifyVariableName(fieldInfo.Name), (Color32)value),
            [typeof(AnimationCurve)] = (fieldInfo, value) => EditorGUILayout.CurveField(ObjectNames.NicifyVariableName(fieldInfo.Name), (AnimationCurve)value ?? new AnimationCurve()),
        };

        private static readonly Dictionary<Type, Func<object>> specialDefaultValueCreators = new Dictionary<Type, Func<object>>
        {
            [typeof(AnimationCurve)] = () => new AnimationCurve(),
        };

        private Type entityStateType;
        private readonly List<FieldInfo> serializableStaticFields = new List<FieldInfo>();
        private readonly List<FieldInfo> serializableInstanceFields = new List<FieldInfo>();

        public void Legacy()
        {
            var collectionProperty = serializedObject.FindProperty(nameof(EntityStateConfiguration.serializedFieldsCollection));
            var systemTypeProp = serializedObject.FindProperty(nameof(EntityStateConfiguration.targetType));
            var assemblyQuallifiedName = systemTypeProp.FindPropertyRelative("assemblyQualifiedName").stringValue;

            EditorGUILayout.PropertyField(systemTypeProp);

            if (entityStateType?.AssemblyQualifiedName != assemblyQuallifiedName)
            {
                entityStateType = Type.GetType(assemblyQuallifiedName);
                PopulateSerializableFields();
            }

            if (entityStateType == null)
            {
                return;
            }

            var serializedFields = collectionProperty.FindPropertyRelative(nameof(SerializedFieldCollection.serializedFields));

            DrawFields(serializableStaticFields, "Static fields", "There is no static fields");
            DrawFields(serializableInstanceFields, "Instance fields", "There is no instance fields");

            var unrecognizedFields = new List<KeyValuePair<SerializedProperty, int>>();
            for (var i = 0; i < serializedFields.arraySize; i++)
            {
                var field = serializedFields.GetArrayElementAtIndex(i);
                var name = field.FindPropertyRelative(nameof(SerializedField.fieldName)).stringValue;
                if (!(serializableStaticFields.Any(el => el.Name == name) || serializableInstanceFields.Any(el => el.Name == name)))
                {
                    unrecognizedFields.Add(new KeyValuePair<SerializedProperty, int>(field, i));
                }
            }

            if (unrecognizedFields.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Unrecognized fields", EditorStyles.boldLabel);
                if (GUILayout.Button("Clear unrecognized fields"))
                {
                    foreach (var fieldRow in unrecognizedFields.OrderByDescending(el => el.Value))
                    {
                        serializedFields.DeleteArrayElementAtIndex(fieldRow.Value);
                    }
                    unrecognizedFields.Clear();
                }

                EditorGUI.indentLevel++;
                foreach (var fieldRow in unrecognizedFields)
                {
                    DrawUnrecognizedField(fieldRow.Key);
                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            void DrawFields(List<FieldInfo> fields, string groupLabel, string emptyLabel)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(groupLabel, EditorStyles.boldLabel);
                if (fields.Count == 0)
                {
                    EditorGUILayout.LabelField(emptyLabel);
                }
                EditorGUI.indentLevel++;
                foreach (var fieldInfo in fields)
                {
                    DrawField(fieldInfo, GetOrCreateField(serializedFields, fieldInfo));
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawUnrecognizedField(SerializedProperty field)
        {
            var name = field.FindPropertyRelative(nameof(SerializedField.fieldName)).stringValue;
            var valueProperty = field.FindPropertyRelative(nameof(SerializedField.fieldValue));
            EditorGUILayout.PropertyField(valueProperty, new GUIContent(ObjectNames.NicifyVariableName(name)), true);
        }

        private void DrawField(FieldInfo fieldInfo, SerializedProperty field)
        {
            var serializedValueProperty = field.FindPropertyRelative(nameof(SerializedField.fieldValue));
            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldInfo.FieldType))
            {
                var objectValue = serializedValueProperty.FindPropertyRelative(nameof(SerializedValue.objectValue));
                EditorGUILayout.ObjectField(objectValue, fieldInfo.FieldType, new GUIContent(ObjectNames.NicifyVariableName(fieldInfo.Name)));
            }
            else
            {
                var stringValue = serializedValueProperty.FindPropertyRelative(nameof(SerializedValue.stringValue));
                var serializedValue = new SerializedValue
                {
                    stringValue = string.IsNullOrWhiteSpace(stringValue.stringValue) ? null : stringValue.stringValue
                };

                if (typeDrawers.TryGetValue(fieldInfo.FieldType, out var drawer))
                {
                    EditorGUI.BeginChangeCheck();
                    var newValue = drawer(fieldInfo, serializedValue.GetValue(fieldInfo));

                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedValue.SetValue(fieldInfo, newValue);
                        stringValue.stringValue = serializedValue.stringValue;
                    }
                }
                else
                {
                    DrawUnrecognizedField(field);
                }
            }
        }

        private SerializedProperty GetOrCreateField(SerializedProperty collectionProperty, FieldInfo fieldInfo)
        {
            for (var i = 0; i < collectionProperty.arraySize; i++)
            {
                var field = collectionProperty.GetArrayElementAtIndex(i);
                if (field.FindPropertyRelative(nameof(SerializedField.fieldName)).stringValue == fieldInfo.Name)
                {
                    return field;
                }
            }
            collectionProperty.arraySize++;

            var serializedField = collectionProperty.GetArrayElementAtIndex(collectionProperty.arraySize - 1);
            var fieldNameProperty = serializedField.FindPropertyRelative(nameof(SerializedField.fieldName));
            fieldNameProperty.stringValue = fieldInfo.Name;

            var fieldValueProperty = serializedField.FindPropertyRelative(nameof(SerializedField.fieldValue));
            var serializedValue = new SerializedValue();
            if (specialDefaultValueCreators.TryGetValue(fieldInfo.FieldType, out var creator))
            {
                serializedValue.SetValue(fieldInfo, creator());
            }
            else
            {
                serializedValue.SetValue(fieldInfo, fieldInfo.FieldType.IsValueType ? Activator.CreateInstance(fieldInfo.FieldType) : (object)null);
            }

            fieldValueProperty.FindPropertyRelative(nameof(SerializedValue.stringValue)).stringValue = serializedValue.stringValue;
            fieldValueProperty.FindPropertyRelative(nameof(SerializedValue.objectValue)).objectReferenceValue = null;

            return serializedField;
        }

        private void PopulateSerializableFields()
        {
            serializableStaticFields.Clear();
            serializableInstanceFields.Clear();

            if (entityStateType == null)
            {
                return;
            }

            var allFieldsInType = entityStateType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var filteredFields = allFieldsInType.Where(fieldInfo =>
            {
                bool canSerialize = SerializedValue.CanSerializeField(fieldInfo);
                bool shouldSerialize = !fieldInfo.IsStatic || (fieldInfo.DeclaringType == entityStateType);
                bool doesNotHaveAttribute = fieldInfo.GetCustomAttribute<HideInInspector>() == null;
                return canSerialize && shouldSerialize && doesNotHaveAttribute;
            });

            serializableStaticFields.AddRange(filteredFields.Where(fieldInfo => fieldInfo.IsStatic));
            serializableInstanceFields.AddRange(filteredFields.Where(fieldInfo => !fieldInfo.IsStatic));
        }

        protected override void DrawInspectorGUI()
        {
            DrawInspectorElement.Add(new IMGUIContainer(Legacy));
        }
    }
}