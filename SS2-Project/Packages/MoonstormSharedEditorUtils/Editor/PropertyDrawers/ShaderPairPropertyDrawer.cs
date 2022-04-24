using RoR2EditorKit.Core.PropertyDrawers;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using Moonstorm.EditorUtils.Settings;
using UnityEditor.UIElements;

namespace Moonstorm.EditorUtils.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ShaderDictionary.ShaderPair))]
    public class ShaderPairPropertyDrawer : VisualElementPropertyDrawer
    {
        protected override void DrawPropertyGUI()
        {
            VisualElement root = new VisualElement();
            root.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

            PropertyField prop = new PropertyField(serializedProperty.FindPropertyRelative("original"));
            Length length = new Length(50, LengthUnit.Percent);
            StyleLength sLength = new StyleLength(length);
            prop.style.width = sLength;
            root.Add(prop);

            prop = new PropertyField(serializedProperty.FindPropertyRelative("stubbed"));
            prop.style.width = sLength;
            root.Add(prop);
            RootVisualElement.Add(root);
        }
    }
}