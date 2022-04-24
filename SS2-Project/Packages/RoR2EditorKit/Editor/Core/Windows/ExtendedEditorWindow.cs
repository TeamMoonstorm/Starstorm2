using RoR2EditorKit.Common;
using RoR2EditorKit.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.EditorWindows
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;
    public abstract class ExtendedEditorWindow : EditorWindow
    {
        public static RoR2EditorKitSettings Settings { get => RoR2EditorKitSettings.GetOrCreateSettings<RoR2EditorKitSettings>(); }
        protected SerializedObject SerializedObject { get; private set; }
        protected virtual void OnEnable()
        {
            base.rootVisualElement.Clear();
            GetTemplateInstance(GetType().Name, rootVisualElement, ValidateUXMLPath);
            SerializedObject = new SerializedObject(this);
            rootVisualElement.Bind(SerializedObject);
        }

        protected virtual bool ValidateUXMLPath(string path)
        {
            return path.StartsWith("Assets/RoR2EditorKit") || path.StartsWith("/Packages/riskofthunder-ror2editorkit");
        }

        protected void CreateGUI()
        {
            DrawGUI();
        }

        protected abstract void DrawGUI();

        #region Util Methods
        protected TElement Find<TElement>(string name = null, string ussClass = null) where TElement : VisualElement
        {
            return rootVisualElement.Q<TElement>(name, ussClass);
        }
        protected TElement Find<TElement>(VisualElement elementToSearch, string name = null, string ussClass = null) where TElement : VisualElement
        {
            return elementToSearch.Q<TElement>(name, ussClass);
        }
        /// <summary>
        /// Queries a visual element of type T from the RootVisualElement, and binds it to a property on the serialized object.
        /// <para>Property is found by using the Element's name as the binding path</para>
        /// </summary>
        /// <typeparam name="TElement">The Type of VisualElement, must inherit IBindable</typeparam>
        /// <param name="name">Optional parameter to find the Element, used in the Quering</param>
        /// <param name="ussClass">Optional parameter of the name of a USSClass the element youre finding uses</param>
        /// <returns>The VisualElement specified, with a binding to the property</returns>
        protected TElement FindAndBind<TElement>(string name = null, string ussClass = null) where TElement : VisualElement, IBindable
        {
            var bindableElement = rootVisualElement.Q<TElement>(name, ussClass);
            if (bindableElement == null)
                throw new NullReferenceException($"Could not find element of type {typeof(TElement)} inside the RootVisualElement.");

            bindableElement.bindingPath = bindableElement.name;
            bindableElement.BindProperty(SerializedObject);

            return bindableElement;
        }

        /// <summary>
        /// Queries a visual element of type T from the RootVisualElement, and binds it to a property on the serialized object.
        /// </summary>
        /// <typeparam name="TElement">The Type of VisualElement, must inherit IBindable</typeparam>
        /// <param name="prop">The property which is used in the Binding process</param>
        /// <param name="name">Optional parameter to find the Element, used in the Quering</param>
        /// <param name="ussClass">Optional parameter of the name of a USSClass the element youre finding uses</param>
        /// <returns>The VisualElement specified, with a binding to the property</returns>
        protected TElement FindAndBind<TElement>(SerializedProperty prop, string name = null, string ussClass = null) where TElement : VisualElement, IBindable
        {
            var bindableElement = rootVisualElement.Q<TElement>(name, ussClass);
            if (bindableElement == null)
                throw new NullReferenceException($"Could not find element of type {typeof(TElement)} inside the RootVisualElement.");

            bindableElement.BindProperty(prop);

            return bindableElement;
        }

        /// <summary>
        /// Queries a visual element of type T from the elementToSearch argument, and binds it to a property on the serialized object.
        /// <para>Property is found by using the Element's name as the binding path</para>
        /// </summary>
        /// <typeparam name="TElement">The Type of VisualElement, must inherit IBindable</typeparam>
        /// <param name="elementToSearch">The VisualElement where the Quering process will be done.</param>
        /// <param name="name">Optional parameter to find the Element, used in the Quering</param>
        /// <param name="ussClass">The name of a USSClass the element youre finding uses</param>
        /// <returns>The VisualElement specified, with a binding to the property</returns>
        protected TElement FindAndBind<TElement>(VisualElement elementToSearch, string name = null, string ussClass = null) where TElement : VisualElement, IBindable
        {
            var bindableElement = elementToSearch.Q<TElement>(name, ussClass);
            if (bindableElement == null)
                throw new NullReferenceException($"Could not find element of type {typeof(TElement)} inside element {elementToSearch.name}.");

            bindableElement.bindingPath = bindableElement.name;
            bindableElement.BindProperty(SerializedObject);

            return bindableElement;
        }

        /// <summary>
        /// Queries a visual element of type T from the elementToSearch argument, and binds it to a property on the serialized object.
        /// <para>Property is found by using the Element's name as the binding path</para>
        /// </summary>
        /// <typeparam name="TElement">The Type of VisualElement, must inherit IBindable</typeparam>
        /// <param name="elementToSearch">The VisualElement where the Quering process will be done.</param>
        /// <param name="name">Optional parameter to find the Element, used in the Quering</param>
        /// <param name="ussClass">The name of a USSClass the element youre finding uses</param>
        /// <returns>The VisualElement specified, with a binding to the property</returns>
        protected TElement FindAndBind<TElement>(VisualElement elementToSearch, SerializedProperty prop, string name = null, string ussClass = null) where TElement : VisualElement, IBindable
        {
            var bindableElement = elementToSearch.Q<TElement>(name, ussClass);
            if (bindableElement == null)
                throw new NullReferenceException($"Could not find element of type {typeof(TElement)} inside element {elementToSearch.name}.");

            bindableElement.BindProperty(prop);

            return bindableElement;
        }

        /// <summary>
        /// Creates a HelpBox and attatches it to a visualElement using IMGUIContainer
        /// </summary>
        /// <param name="message">The message that'll appear on the help box</param>
        /// <param name="messageType">The type of message</param>
        /// <param name="elementToAttach">Optional, if specified, the Container will be added to this element, otherwise, it's added to the RootVisualElement</param>
        protected IMGUIContainer CreateHelpBox(string message, MessageType messageType, VisualElement elementToAttach = null)
        {
            IMGUIContainer container = new IMGUIContainer();
            container.onGUIHandler = () =>
            {
                EditorGUILayout.HelpBox(message, messageType);
            };

            if (elementToAttach != null)
            {
                elementToAttach.Add(container);
                return container;
            }
            rootVisualElement.Add(container);
            return container;
        }
        #endregion
    }
}