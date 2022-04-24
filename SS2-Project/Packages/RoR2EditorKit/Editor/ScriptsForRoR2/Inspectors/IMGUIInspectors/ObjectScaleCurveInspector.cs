using RoR2;
using RoR2EditorKit.Core.Inspectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(ObjectScaleCurve))]
    public class ObjectScaleCurveInspector : ComponentInspector<ObjectScaleCurve>
    {
        protected override void DrawInspectorGUI()
        {
            DrawInspectorElement.Add(new IMGUIContainer(Legacy));
        }

        private void Legacy()
        {
            SerializedProperty useOverallCurveProp = serializedObject.FindProperty("useOverallCurveOnly");
            EditorGUILayout.PropertyField(useOverallCurveProp);
            if (useOverallCurveProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("overallCurve"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("curveX"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("curveY"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("curveZ"));
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("timeMax"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}