using RoR2EditorKit.Core.Inspectors;
using System;
using System.Collections.Generic;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Settings
{
    public class EditorInspectorSettings : ThunderKitSetting
    {
        [Serializable]
        public class InspectorSetting
        {
            public string inspectorName;

            [HideInInspector]
            public string typeReference;

            public bool isEnabled;
        }

        const string MarkdownStylePath = "Packages/com.passivepicasso.thunderkit/Documentation/uss/markdown.uss";
        const string DocumentationStylePath = "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss";

        [InitializeOnLoadMethod]
        static void SetupSettings()
        {
            GetOrCreateSettings<EditorInspectorSettings>();
        }

        private SerializedObject enabledAndDisabledInspectorSettingsSO;

        public bool enableNamingConventions = true;

        public List<InspectorSetting> inspectorSettings = new List<InspectorSetting>();

        public RoR2EditorKitSettings MainSettings { get => GetOrCreateSettings<RoR2EditorKitSettings>(); }

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            if (enabledAndDisabledInspectorSettingsSO == null)
                enabledAndDisabledInspectorSettingsSO = new SerializedObject(this);

            var namingConventions = CreateStandardField(nameof(enableNamingConventions));
            namingConventions.tooltip = $"If enabled, certain inspectors will notify you that you're not following the mod community's naming conventions.";
            rootElement.Add(namingConventions);

            /*var enabledInspectors = CreateStandardField(nameof(inspectorSettings));
            enabledInspectors.tooltip = $"Which Inspectors that use RoR2EditorKit systems are enabled.";*/

            var enabledInspectors = EditorInspectorSettingsInspector.StaticInspectorGUI(enabledAndDisabledInspectorSettingsSO);
            rootElement.Add(enabledInspectors);


            rootElement.Bind(enabledAndDisabledInspectorSettingsSO);
        }

        /// <summary>
        /// Tries to get or create the settings for an inspector
        /// </summary>
        /// <param name="type">The inspector's Type</param>
        /// <returns>The Inspector's InspectorSetting</returns>
        public InspectorSetting GetOrCreateInspectorSetting(Type type)
        {
            var setting = inspectorSettings.Find(x => x.typeReference == type.AssemblyQualifiedName);
            if (setting != null)
            {
                return setting;
            }
            else
            {
                setting = new InspectorSetting
                {
                    inspectorName = type.Name,
                    typeReference = type.AssemblyQualifiedName,
                    isEnabled = true
                };
                inspectorSettings.Add(setting);
                return setting;
            }
        }
    }
}
