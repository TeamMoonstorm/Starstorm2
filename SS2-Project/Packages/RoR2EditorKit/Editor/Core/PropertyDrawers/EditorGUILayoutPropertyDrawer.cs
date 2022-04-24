using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.PropertyDrawers
{
    public abstract class EditorGUILayoutPropertyDrawer : PropertyDrawer
    {
        SerializedProperty serializedProperty;

        /// <summary>
        /// Override this method to make your own GUI for this property
        /// <para>You'll probably want to use DrawPropertyDrawer instead</para>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <param name="label"></param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            serializedProperty = property;
            EditorGUI.BeginProperty(position, label, property);
            DrawPropertyDrawer(property);
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Draws the custom property drawer
        /// </summary>
        /// <param name="property">The property drawer used for this property</param>
        protected abstract void DrawPropertyDrawer(SerializedProperty property);
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return -2f;
        }

        /// <summary>
        /// Draws a property field using the given property name
        /// <para>The property will be found from the serialized object that's being inspected</para>
        /// </summary>
        /// <param name="propName">The property's name</param>
        protected void DrawField(string propName) => EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(propName), true);
        /// <summary>
        /// Draws a property field using the given property name
        /// <para>The property will be found from the given SerializedProperty</para>
        /// </summary>
        /// <param name="property">The property to search in</param>
        /// <param name="propName">The property to find and draw</param>
        protected void DrawField(SerializedProperty property, string propName) => EditorGUILayout.PropertyField(property.FindPropertyRelative(propName), true);
        /// <summary>
        /// Draws a property field using the given property
        /// </summary>
        /// <param name="property">The property to draw</param>
        protected void DrawField(SerializedProperty property) => EditorGUILayout.PropertyField(property, true);
        /// <summary>
        /// Creates a Header for the inspector
        /// </summary>
        /// <param name="label">The text for the label used in this header</param>
        protected void Header(string label) => EditorGUILayout.LabelField(new GUIContent(label), EditorStyles.boldLabel);

        /// <summary>
        /// Creates a Header with a tooltip for the inspector
        /// </summary>
        /// <param name="label">The text for the label used in this header</param>
        /// <param name="tooltip">A tooltip that's displayed after the mouse hovers over the label</param>
        protected void Header(string label, string tooltip) => EditorGUILayout.LabelField(new GUIContent(label, tooltip), EditorStyles.boldLabel);
    }
}