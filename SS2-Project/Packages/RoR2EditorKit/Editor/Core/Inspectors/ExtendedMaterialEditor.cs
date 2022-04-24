using RoR2EditorKit.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.Inspectors
{
    [CustomEditor(typeof(Material))]
    public class ExtendedMaterialInspector : MaterialEditor
    {
        public static Dictionary<string, Action> shaderNameToAction = new Dictionary<string, Action>();

        public static RoR2EditorKitSettings Settings { get => RoR2EditorKitSettings.GetOrCreateSettings<RoR2EditorKitSettings>(); }

        public static bool MaterialEditorEnabled { get => Settings.MaterialEditorSettings.EnableMaterialEditor; }

        public static MaterialEditor Instance { get; private set; }

        private Action chosenActionForMaterial = null;

        private Material material;

        public override void Awake()
        {
            base.Awake();
            if(MaterialEditorEnabled)
            {
                material = target as Material;
                chosenActionForMaterial = GetActionForMaterial();
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            material = target as Material;
            chosenActionForMaterial = GetActionForMaterial();
        }

        protected override void OnShaderChanged()
        {
            base.OnShaderChanged();
            material = target as Material;
            chosenActionForMaterial = GetActionForMaterial();
        }

        private Action GetActionForMaterial()
        {
            foreach(var shaderStringPair in Settings.MaterialEditorSettings.shaderStringPairs)
            {
                if(shaderNameToAction.ContainsKey(shaderStringPair.shaderName) && material.shader == shaderStringPair.shader)
                {
                    return shaderNameToAction[shaderStringPair.shaderName];
                }
            }
            return null;
        }

        public override void OnInspectorGUI()
        {
            Instance = this as MaterialEditor;
            if (chosenActionForMaterial != null)
            {
                chosenActionForMaterial.Invoke();
                serializedObject.ApplyModifiedProperties();
            }
            else
                base.OnInspectorGUI();
        }

        /// <summary>
        /// Adds a Shader editor to the Extended Material Inspector.
        /// <para>Adding the shader will make it appear in the MaterialEditorSettings section of the ThunderkitSettings window</para>
        /// </summary>
        /// <param name="shaderName">The name of the shader, try to use the file name instead of the actual name.</param>
        /// <param name="inspectorForShader">A method for drawing the material inspector</param>
        /// <param name="callingType">The type that's calling the method</param>
        public static void AddShader(string shaderName, Action inspectorForShader, Type callingType)
        {
            Settings.MaterialEditorSettings.CreateShaderStringPairIfNull(shaderName, callingType);

            shaderNameToAction.Add(shaderName, inspectorForShader);
        }

        /// <summary>
        /// Draws a Material Property in the inspector, using the shader's Property UI
        /// </summary>
        /// <param name="name">The name of the property to draw</param>
        /// <returns>The Drawn Property, If no material editor instance exists, it returns null</returns>
        public static MaterialProperty DrawProperty(string name)
        {
            if (Instance)
            {
                MaterialProperty prop = GetMaterialProperty(Instance.targets, name);
                Instance.ShaderProperty(prop, prop.displayName);
                return prop;
            }
            return null;
        }

        /// <summary>
        /// Grabs a MaterialProperty from the inspected material
        /// </summary>
        /// <param name="name">The name of the property to grab</param>
        /// <returns>The requested property, If no material editor instance exists, it returns null</returns>
        public static MaterialProperty GetProperty(string name)
        {
            if (Instance)
                return GetMaterialProperty(Instance.targets, name);
            return null;
        }

        /// <summary>
        /// Checks if a Shader keyword is enabled. By looking if the given property's float value is 0 (false) or 1 (true)
        /// </summary>
        /// <param name="prop">The property to check</param>
        /// <returns>True if the float value is 1, false if its 0, or if the value is not 0 or 1</returns>
        public static bool ShaderKeyword(MaterialProperty prop)
        {
            if (prop.floatValue == 1)
                return true;
            else if (prop.floatValue == 0)
                return false;

            return false;
        }

        /// <summary>
        /// Creates a Header for the inspector
        /// </summary>
        /// <param name="label">The text for the label used in this header</param>
        public static void Header(string label) => EditorGUILayout.LabelField(new GUIContent(label), EditorStyles.boldLabel);

        /// <summary>
        /// Creates a Header with a tooltip for the inspector
        /// </summary>
        /// <param name="label">The text for the label used in this header</param>
        /// <param name="tooltip">A tooltip that's displayed after the mouse hovers over the label</param>
        public static void Header(string label, string tooltip) => EditorGUILayout.LabelField(new GUIContent(label, tooltip), EditorStyles.boldLabel);
    }
}
