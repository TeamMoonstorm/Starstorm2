using RoR2EditorKit.Utilities;
using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests;
using ThunderKit.Markdown;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Settings
{
    public class RoR2EditorKitSettings : ThunderKitSetting
    {
        const string MarkdownStylePath = "Packages/com.passivepicasso.thunderkit/Documentation/uss/markdown.uss";
        const string DocumentationStylePath = "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss";

        [InitializeOnLoadMethod]
        static void SetupSettings()
        {
            GetOrCreateSettings<RoR2EditorKitSettings>();
        }

        private SerializedObject ror2EditorKitSettingsSO;

        public string TokenPrefix;

        public Manifest MainManifest;

        public bool madeRoR2EKAssetsNonEditable = false;

        public EditorInspectorSettings InspectorSettings { get => GetOrCreateSettings<EditorInspectorSettings>(); }

        public MaterialEditorSettings MaterialEditorSettings { get => GetOrCreateSettings<MaterialEditorSettings>(); }

        public override void Initialize() => TokenPrefix = "";

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            MarkdownElement markdown = null;
            if (string.IsNullOrEmpty(TokenPrefix))
            {
                markdown = new MarkdownElement
                {
                    Data = $@"**__Warning:__** No Token Prefix assigned. Assign a token prefix before continuing.",

                    MarkdownDataType = MarkdownDataType.Text
                };
                markdown.RefreshContent();
                rootElement.Add(markdown);
            }

            var tokenPrefix = CreateStandardField(nameof(TokenPrefix));
            tokenPrefix.tooltip = $"RoR2EK has tools to automatically set the tokens for certain objects." +
                $"\nThis token is also used when fixing the naming conventions set in place by certain editors." +
                $"\nToken should not have Underscores (_), can be lower or uppercase.";
            rootElement.Add(tokenPrefix);

            var mainManifest = CreateStandardField(nameof(MainManifest));
            mainManifest.tooltip = $"The main manifest of this unity project, used for certain windows and utilities";
            rootElement.Add(mainManifest);

            if (ror2EditorKitSettingsSO == null)
                ror2EditorKitSettingsSO = new SerializedObject(this);

            rootElement.Bind(ror2EditorKitSettingsSO);
        }

        public string GetPrefixUppercase()
        {
            if(TokenPrefix.IsNullOrEmptyOrWhitespace())
            {
                throw ErrorShorthands.ThrowNullTokenPrefix();
            }
            return TokenPrefix.ToUpperInvariant();
        }
        public string GetPrefixLowercase()
        {
            if (TokenPrefix.IsNullOrEmptyOrWhitespace())
            {
                throw ErrorShorthands.ThrowNullTokenPrefix();
            }
            return TokenPrefix.ToLowerInvariant();
        }
        public string GetPrefix1stUpperRestLower()
        {
            List<char> prefix = new List<char>();
            for (int i = 0; i < TokenPrefix.Length; i++)
            {
                char letter = TokenPrefix[i];
                if(i == 0)
                {
                    prefix.Add(char.ToUpperInvariant(letter));
                }
                else
                {
                    prefix.Add(char.ToLowerInvariant(letter));
                }
            }
            return new string(prefix.ToArray());
        }
    }
}
