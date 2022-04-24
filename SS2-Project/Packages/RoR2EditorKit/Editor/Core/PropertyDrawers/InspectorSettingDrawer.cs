using RoR2EditorKit.Settings;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(EditorInspectorSettings.InspectorSetting))]
    public class InspectorSettingPropertyDrawer : IMGUIPropertyDrawer
    {
        protected override void DrawCustomDrawer()
        {
            Begin();
            var isEnabled = property.FindPropertyRelative("isEnabled");
            var displayName = property.FindPropertyRelative("inspectorName");

            GUIContent content = new GUIContent(NicifyName(displayName.stringValue), "Wether or not this inspector is enabled.");

            EditorGUI.PropertyField(rect, isEnabled, content, false);
            End();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) / 3;
        }
    }
}
