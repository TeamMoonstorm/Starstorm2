using RoR2EditorKit.Core.Inspectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine.UIElements;
using UnityEditor;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(ChildLocator))]
    public class ChildLocatorInspector : ComponentInspector<ChildLocator>
    {
        protected override void DrawInspectorGUI()
        {
            DrawInspectorElement.Add(new IMGUIContainer(Legacy));
        }
        private void Legacy()
        {
            var array = serializedObject.FindProperty("transformPairs");
            array.arraySize = EditorGUILayout.DelayedIntField("Size", array.arraySize);
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;
            DrawNameTransformPairs(array);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }
        private void DrawNameTransformPairs(SerializedProperty property)
        {
            for (int i = 0; i < property.arraySize; i++)
            {
                var prop = property.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(prop.FindPropertyRelative("name"));
                EditorGUILayout.PropertyField(prop.FindPropertyRelative("transform"));
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
