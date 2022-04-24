using RoR2EditorKit.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.Inspectors
{
    //This is also fucking stupid
    [CustomEditor(typeof(MaterialEditorSettings))]
    public class MaterialEditorSettingsInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return StaticInspectorGUI(serializedObject);
        }

        public static VisualElement StaticInspectorGUI(SerializedObject serializedObject)
        {
            VisualElement ve = new VisualElement();
            ve.Add(new PropertyField(serializedObject.FindProperty(nameof(MaterialEditorSettings.EnableMaterialEditor))));

            SerializedProperty settings = serializedObject.FindProperty(nameof(MaterialEditorSettings.shaderStringPairs));

            foreach (SerializedProperty prop in settings)
            {
                ve.Add(new PropertyField(prop));
            }
            return ve;
        }
    }
}
