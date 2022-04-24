using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using RoR2EditorKit.Settings;
using RoR2EditorKit.Common;
using System;
using Object = UnityEngine.Object;
using RoR2EditorKit.Utilities;

namespace RoR2EditorKit.Core.Inspectors
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;

    /// <summary>
    /// Base inspector for all the RoR2EditorKit Inspectors.
    /// <para>If you want to make a Scriptable Object Inspector, you'll probably want to use the ScriptableObjectInspector</para>
    /// <para>If you want to make an Inspector for a Component, you'll probably want to use the ComponentInspector</para>
    /// </summary>
    /// <typeparam name="T">The type of Object being inspected</typeparam>
    public abstract class ExtendedInspector<T> : Editor where T : Object
    {
        #region Properties
        /// <summary>
        /// Access to the Settings file
        /// </summary>
        public static RoR2EditorKitSettings Settings { get => RoR2EditorKitSettings.GetOrCreateSettings<RoR2EditorKitSettings>(); }

        /// <summary>
        /// The setting for this inspector
        /// </summary>
        public EditorInspectorSettings.InspectorSetting InspectorSetting
        {
            get
            {
                if(_inspectorSetting == null)
                {
                    _inspectorSetting = Settings.InspectorSettings.GetOrCreateInspectorSetting(GetType()); ;
                }
                return _inspectorSetting;
            }
            set
            {
                if(_inspectorSetting != value)
                {
                    var index = Settings.InspectorSettings.inspectorSettings.IndexOf(_inspectorSetting);
                    Settings.InspectorSettings.inspectorSettings[index] = value;
                    _inspectorSetting = value;
                }
            }
        }
        private EditorInspectorSettings.InspectorSetting _inspectorSetting;

        /// <summary>
        /// Check if the inspector is enabled
        /// <para>If you're setting the value, and the value is different from the old value, the inspector will redraw completely to accomodate the new look using either the base inspector or custom inspector</para>
        /// </summary>
        public bool InspectorEnabled
        {
            get
            {
                return InspectorSetting.isEnabled;
            }
            set
            {
                if(value != InspectorSetting.isEnabled)
                {
                    InspectorSetting.isEnabled = value;
                    OnInspectorEnabledChange();
                }
            }
        }

        /// <summary>
        /// The root visual element of the inspector, This is what gets returned by CreateInspectorGUI()
        /// <para>When the inspector is enabled, the "DrawInspectorElement" is added to this</para>
        /// <para>When the inspector is disabled, the "IMGUIContainerElement" with the default inspector is added to this.</para>
        /// </summary>
        protected VisualElement RootVisualElement
        {
            get
            {
                if (_rootVisualElement == null)
                {
                    _rootVisualElement = new VisualElement();
                    _rootVisualElement.name = "ExtendedInspector_RootElement";
                }

                return _rootVisualElement;
            }
        }
        private VisualElement _rootVisualElement;

        /// <summary>
        /// The root visual element where your custom inspector will be drawn.
        /// <para>This visual element will have the VisualTreeAsset applied.</para>
        /// </summary>
        protected VisualElement DrawInspectorElement
        {
            get
            {
                if (_drawInspectorElement == null)
                {
                    _drawInspectorElement = new VisualElement();
                    _drawInspectorElement.name = "ExtendedInspector_CustomEditor";
                }
                return _drawInspectorElement;
            }
        }
        private VisualElement _drawInspectorElement;

        /// <summary>
        /// The root visual element where the default, IMGUI inspector is drawn
        /// <para>This visual element will not have the VisualTreeAsset applied</para>
        /// <para>The IMGUIContainer that gets returned by the default inspector is added to this, it's name is "defaultInspector" if you need to Query it.</para>
        /// </summary>
        protected VisualElement IMGUIContainerElement
        {
            get
            {
                if (_imguiContianerElement == null)
                {
                    _imguiContianerElement = new VisualElement();
                    _imguiContianerElement.name = "ExtendedInspector_DefaultInspector";
                }
                return _imguiContianerElement;
            }
        }
        private VisualElement _imguiContianerElement;

        /// <summary>
        /// Direct access to the object that's being inspected as its type.
        /// </summary>
        protected T TargetType { get => target as T; }
        #endregion Properties

        #region Fields
        /// <summary>
        /// The visual tree asset, every inspector should have an UXML file with the inspector layout
        /// <para>The visual tree asset is used to setup the inspector layout for the "DrawInspectorElement"</para>
        /// </summary>
        protected VisualTreeAsset visualTreeAsset;

        /// <summary>
        /// The prefix this asset should use, leave this null unless the asset youre creating requires a prefix.
        /// </summary>
        protected string prefix = null;

        /// <summary>
        /// If the "prefix" string uses the TokenPrefix on the settings file, set this to true.
        /// </summary>
        protected bool prefixUsesTokenPrefix = false;

        private IMGUIContainer prefixContainer = null;
        #endregion Fields

        #region Methods
        /// <summary>
        /// Called when the inspector is enabled, always keep the original implementation unless you know what youre doing
        /// </summary>
        protected virtual void OnEnable() { }

        private void OnInspectorEnabledChange()
        {
            void ClearElements()
            {
                RootVisualElement.Clear();
                RootVisualElement.styleSheets.Clear();
                IMGUIContainerElement.Clear();
                IMGUIContainerElement.styleSheets.Clear();
                DrawInspectorElement.Clear();
                DrawInspectorElement.styleSheets.Clear();
            }

            ClearElements();
            OnRootElementsCleared?.Invoke();

            GetTemplateInstance(GetType().Name, DrawInspectorElement, ValidateUXMLPath);
            DrawInspectorElement.Bind(serializedObject);
            OnVisualTreeCopy?.Invoke();

            EnsureNamingConventions();

            if(!InspectorEnabled)
            {
                var defaultImguiContainer = new IMGUIContainer(OnInspectorGUI);
                defaultImguiContainer.name = "defaultInspector";
                IMGUIContainerElement.Add(defaultImguiContainer);
                RootVisualElement.Add(IMGUIContainerElement);
                OnIMGUIContainerElementAdded?.Invoke();
            }
            else
            {
                DrawInspectorGUI();
                RootVisualElement.Add(DrawInspectorElement);
                OnDrawInspectorElementAdded?.Invoke();
            }
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual bool ValidateUXMLPath(string path)
        {
            return path.StartsWith(Constants.AssetFolderPath) || path.StartsWith(Constants.PackageFolderPath);
        }

        /// <summary>
        /// DO NOT OVERRIDE THIS METHOD. Use "DrawInspectorGUI" if you want to implement your inspector!
        /// </summary>
        /// <returns>DO NOT OVERRIDE THIS METHOD. Use "DrawInspectorGUI" if you want to implement your inspector!</returns>
        public override VisualElement CreateInspectorGUI()
        {
            OnInspectorEnabledChange();
            serializedObject.ApplyModifiedProperties();
            return RootVisualElement;
        }
        #endregion Methods

        #region Delegates
        /// <summary>
        /// Invoked when the RootVisualElement, DrawInspectorElement and IMGUIContainerElement are cleared;
        /// </summary>
        protected Action OnRootElementsCleared;

        /// <summary>
        /// Invoked when the VisualTree assigned to this inspector has been copied to the "DrawInspectorElement"
        /// </summary>
        protected Action OnVisualTreeCopy;

        /// <summary>
        /// Invoked right after "IMGUIContainerElement" is added to the "RootVisualElement"
        /// </summary>
        protected Action OnIMGUIContainerElementAdded;

        /// <summary>
        /// Invoked right after the "DrawInspectorElement" is added to the "RootVisualElement"
        /// </summary>
        protected Action OnDrawInspectorElementAdded;
        #endregion

        /// <summary>
        /// Implement The code functionality of your inspector here.
        /// </summary>
        protected abstract void DrawInspectorGUI();

        #region Util Methods
        /// <summary>
        /// Shorthand for finding a visual element. the element you're requesting will be queried on the DrawInspectorElement.
        /// </summary>
        /// <typeparam name="TElement">The type of visual element.</typeparam>
        /// <param name="name">Optional parameter to find the element</param>
        /// <param name="ussClass">Optional parameter to find the element</param>
        /// <returns>The VisualElement specified</returns>
        protected TElement Find<TElement>(string name = null, string ussClass = null) where TElement : VisualElement
        {
            return DrawInspectorElement.Q<TElement>(name, ussClass);
        }

        /// <summary>
        /// Shorthand for finding a visual element. the element you're requesting will be queried on the "elementToSearch"
        /// </summary>
        /// <typeparam name="TElement">The Type of VisualElement</typeparam>
        /// <param name="elementToSearch">The VisualElement where the Quering process will be done.</param>
        /// <param name="name">Optional parameter to find the element</param>
        /// <param name="ussClass">Optional parameter to find the element</param>
        /// <returns>The VisualElement specified</returns>
        protected TElement Find<TElement>(VisualElement elementToSearch, string name = null, string ussClass = null)where TElement : VisualElement
        {
            return elementToSearch.Q<TElement>(name, ussClass);
        }
        /// <summary>
        /// Queries a visual element of type T from the DrawInspectorElement, and binds it to a property on the serialized object.
        /// <para>Property is found by using the Element's name as the binding path</para>
        /// </summary>
        /// <typeparam name="TElement">The Type of VisualElement, must inherit IBindable</typeparam>
        /// <param name="name">Optional parameter to find the Element, used in the Quering</param>
        /// <param name="ussClass">Optional parameter of the name of a USSClass the element youre finding uses</param>
        /// <returns>The VisualElement specified, with a binding to the property</returns>
        protected TElement FindAndBind<TElement>(string name = null, string ussClass = null) where TElement : VisualElement, IBindable
        {
            var bindableElement = DrawInspectorElement.Q<TElement>(name, ussClass);
            if (bindableElement == null)
                throw new NullReferenceException($"Could not find element of type {typeof(TElement)} inside the DrawInspectorElement.");

            bindableElement.bindingPath = bindableElement.name;
            bindableElement.BindProperty(serializedObject);

            return bindableElement;
        }

        /// <summary>
        /// Queries a visual element of type T from the DrawInspectorElement, and binds it to a property on the serialized object.
        /// </summary>
        /// <typeparam name="TElement">The Type of VisualElement, must inherit IBindable</typeparam>
        /// <param name="prop">The property which is used in the Binding process</param>
        /// <param name="name">Optional parameter to find the Element, used in the Quering</param>
        /// <param name="ussClass">Optional parameter of the name of a USSClass the element youre finding uses</param>
        /// <returns>The VisualElement specified, with a binding to the property</returns>
        protected TElement FindAndBind<TElement>(SerializedProperty prop, string name = null, string ussClass = null) where TElement : VisualElement, IBindable
        {
            var bindableElement = DrawInspectorElement.Q<TElement>(name, ussClass);
            if (bindableElement == null)
                throw new NullReferenceException($"Could not find element of type {typeof(TElement)} inside the DrawInspectorElement.");

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
            bindableElement.BindProperty(serializedObject);

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
        /// <param name="attachToRootIfElementToAttachIsNull">If left true, and the elementToAttach is not null, the IMGUIContainer is added to the RootVisualElement.</param>
        /// <param name="elementToAttach">Optional, if specified, the Container will be added to this element, otherwise if the "attachToRootIfElementToAttachIsNull" is true, it'll attach it to the RootVisualElement, otherwise if both those conditions fail, it returns the IMGUIContainer unattached.</param>
        /// <returns>An IMGUIContainer that's either not attached to anything, attached to the RootElement, or attached to the elementToAttach argument.</returns>
        protected IMGUIContainer CreateHelpBox(string message, MessageType messageType)
        {
            IMGUIContainer container = new IMGUIContainer();
            container.name = $"ExtendedInspector_HelpBox";
            container.onGUIHandler = () =>
            {
                EditorGUILayout.HelpBox(message, messageType);
            };

            return container;
        }

        /// <summary>
        /// Ensure the naming convention for a specific object stays.
        /// <para>This method is ran right after OnRootElementCleared by default.</para>
        /// <para>Requires that the prefix for this inspector is not null.</para>
        /// </summary>
        /// <param name="evt">The ChangeEvent, used if the Method is used on a visual element's RegisterValueChange</param>
        /// <returns>If the convention is not followed, an IMGUIContainer with a help box, otherwise it returns null.</returns>
        protected virtual IMGUIContainer EnsureNamingConventions(ChangeEvent<string> evt = null)
        {
            if(!Settings.InspectorSettings.enableNamingConventions)
            {
                return null;
            }

            if(prefixContainer != null)
            {
                prefixContainer.TryRemoveFromParent();
            }

            if(evt != null)
            {
                TargetType.name = evt.newValue;
            }

            if(prefixUsesTokenPrefix && Settings.TokenPrefix.IsNullOrEmptyOrWhitespace())
            {
                throw ErrorShorthands.ThrowNullTokenPrefix();
            }


            if(prefix != null)
            {
                if(TargetType && !TargetType.name.ToLowerInvariant().StartsWith(prefix.ToLowerInvariant()))
                {
                    string typeName = typeof(T).Name;
                    prefixContainer = CreateHelpBox($"This {typeName}'s name should start with {prefix} for naming conventions.", MessageType.Info);
                    return prefixContainer;
                }
            }
            return null;
        }

        #endregion
    }
}
