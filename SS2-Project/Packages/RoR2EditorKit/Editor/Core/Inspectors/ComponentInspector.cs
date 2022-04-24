using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.Inspectors
{
    /// <summary>
    /// Inherit from this class to make your own Component Inspectors.
    /// </summary>
    public abstract class ComponentInspector<T> : ExtendedInspector<T> where T : MonoBehaviour
    {

        private IMGUIContainer container;
        protected override void OnEnable()
        {
            base.OnEnable();
            container = new IMGUIContainer(DisplayToggle);
            OnRootElementsCleared += AddIMGUIContainer;
        }

        protected void AddIMGUIContainer()
        {
            RootVisualElement.Add(container);
        }

        private void DisplayToggle()
        {
            EditorGUILayout.BeginVertical("box");
            InspectorEnabled = EditorGUILayout.ToggleLeft($"Enable {ObjectNames.NicifyVariableName(target.GetType().Name)} Inspector", InspectorEnabled);
            EditorGUILayout.EndVertical();
        }
    }
}