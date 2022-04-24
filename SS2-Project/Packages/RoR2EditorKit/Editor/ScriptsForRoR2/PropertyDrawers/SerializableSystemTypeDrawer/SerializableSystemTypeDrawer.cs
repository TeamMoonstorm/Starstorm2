using HG;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SerializableSystemType), true)]
    public class SerializableSystemTypeDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var typeReference = property.FindPropertyRelative("assemblyQualifiedName");
            HandleDragAndDrop(typeReference, position);

            position = EditorGUI.PrefixLabel(position,
                GUIUtility.GetControlID(FocusType.Passive), label);

            var style = new GUIStyle(GUI.skin.button);
            style.alignment = TextAnchor.MiddleLeft;
            style.fontStyle = FontStyle.Normal;
            style.fontSize = 9;

            var (typeName, fullName) = GetTypeName(typeReference);
            if (string.IsNullOrEmpty(typeName))
            {
                typeName = "None";
                typeReference.stringValue = string.Empty;
                style.normal.textColor = new Color(0.7f, 0, 0);
            }

            if (GUI.Button(position, new GUIContent(typeName, fullName), style))
            {
                new SerializableSystemTypeTreePicker.PickerCreator
                {
                    parentProperty = property,
                    systemTypeReference = typeReference,
                    pickerPosition = GetLastRectAbsolute(position),
                    serializedObject = property.serializedObject,
                };
            }

            EditorGUI.EndProperty();
        }

        protected virtual (string typeName, string fullName) GetTypeName(SerializedProperty typeReference)
        {
            string reference = typeReference.stringValue;
            Type type;
            if (string.IsNullOrEmpty(reference) || (type = Type.GetType(reference, false)) == null)
                return (string.Empty, string.Empty);

            return (type.Name, type.FullName);
        }

        private void HandleDragAndDrop(SerializedProperty typeReference, Rect dropArea)
        {
            var currentEvent = Event.current;
            if (!dropArea.Contains(currentEvent.mousePosition))
                return;

            if (currentEvent.type != EventType.DragUpdated && currentEvent.type != EventType.DragPerform)
                return;

            var reference = DragAndDrop.objectReferences[0];
            if (reference != null && !(reference is TextAsset))
                reference = null;

            DragAndDrop.visualMode = reference != null ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;

            if (currentEvent.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                if (reference != null)
                    typeReference.stringValue = ((TextAsset)reference).text;
                DragAndDrop.PrepareStartDrag();
                GUIUtility.hotControl = 0;
            }
            currentEvent.Use();
        }


        public static Rect GetLastRectAbsolute(Rect relativePos)
        {
            Rect result = relativePos;
            result.x += EditorWindow.focusedWindow.position.x;
            result.y += EditorWindow.focusedWindow.position.y;
            try
            {
                Type type = EditorWindow.focusedWindow.GetType();
                FieldInfo field = type.GetField("s_CurrentInspectorWindow", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                FieldInfo field2 = type.GetField("m_ScrollPosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Vector2 vector = (Vector2)field2.GetValue(field.GetValue(null));
                result.x -= vector.x;
                result.y -= vector.y;
            }
            catch
            {
            }
            return result;
        }

    }
}