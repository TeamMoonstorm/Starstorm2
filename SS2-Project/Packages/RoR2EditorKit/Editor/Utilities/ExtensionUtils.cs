using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace RoR2EditorKit.Utilities
{
    /// <summary>
    /// Class holding various utility methods for interacting with the editor and the asset database
    /// </summary>
    public static class ExtensionUtils
    {
        #region String Extensions
        public static bool IsNullOrEmptyOrWhitespace(this string text)
        {
            return (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text));
        }
        #endregion

        #region SerializedProperties/Objects  Extensions
        public static SerializedProperty GetBindedProperty(this ObjectField objField, SerializedObject objectBound)
        {
            if (objField.bindingPath.IsNullOrEmptyOrWhitespace())
                throw new NullReferenceException($"{objField} doesnot have a bindingPath set");

            return objectBound.FindProperty(objField.bindingPath);
        }

        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    yield return currentProperty;
                }
                while (currentProperty.NextVisible(false));
            }
        }
        #endregion

        #region Visual Element Extensions
        public static void TryRemoveFromParent(this VisualElement element)
        {
            if(element != null && element.parent != null)
            {
                element.parent.Remove(element);
            }
        }

        /// <summary>
        /// Quick method to set the ObjectField's object type
        /// </summary>
        /// <typeparam name="TObj">The type of object to set</typeparam>
        /// <param name="objField">The object field</param>
        public static void SetObjectType<T>(this ObjectField objField) where T : UnityEngine.Object
        {
            objField.objectType = typeof(T);
        }
        #endregion
    }
}
