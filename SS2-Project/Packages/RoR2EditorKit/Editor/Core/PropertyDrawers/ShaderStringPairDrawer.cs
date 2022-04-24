using RoR2EditorKit.Settings;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(MaterialEditorSettings.ShaderStringPair))]
    public class ShaderStringPairPropertyDrawer : IMGUIPropertyDrawer
    {
        protected override void DrawCustomDrawer()
        {
            Begin();
            var objRefProperty = property.FindPropertyRelative("shader");
            objRefProperty.objectReferenceValue = EditorGUI.ObjectField(rect, NicifyName(property.FindPropertyRelative("shaderName").stringValue), objRefProperty.objectReferenceValue, typeof(Shader), false);
            End();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) / 3;
        }
    }
}
