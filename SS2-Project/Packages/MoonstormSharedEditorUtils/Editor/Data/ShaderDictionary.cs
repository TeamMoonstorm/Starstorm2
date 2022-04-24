using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.Settings
{
    public class ShaderDictionary : ThunderKitSetting
    {
        [Serializable]
        public class ShaderPair
        {
            public Shader original;
            public Shader stubbed;

            public ShaderPair(Shader original, Shader stubbed)
            {
                this.original = original;
                this.stubbed = stubbed;
            }
        }

        const string ShaderRootGUID = "e57526cd2e529264f8e9999843849112";

        [InitializeOnLoadMethod]
        static void SetupSettings()
        {
            GetOrCreateSettings<ShaderDictionary>();
        }

        public void UpdateLists()
        {
            var origs = shaderPairs.Where(p => p.original).Select(p => p.original);
            var stubbeds = shaderPairs.Where(p => p.stubbed).Select(p => p.stubbed);

            allShaders = origs.Union(stubbeds).ToList();
            validPairs = shaderPairs.Where(p => p.original && p.stubbed).ToList();
        }

        private SerializedObject shaderDictionarySO;

        public List<ShaderPair> shaderPairs = new List<ShaderPair>();
        [HideInInspector]
        public List<Shader> allShaders = new List<Shader>();
        [HideInInspector]
        public List<ShaderPair> validPairs = new List<ShaderPair>();

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            UpdateLists();
            if (shaderDictionarySO == null)
                shaderDictionarySO = new SerializedObject(this);

            if (shaderPairs.Count == 0)
                FillWithDefaultShaders();

            var attemptToFinish = new Button();
            attemptToFinish.text = $"Attempt to find missing keys";
            attemptToFinish.tooltip = $"When clicked, MSEU will attempt to find the missing keys based off the value of stubbed shader." +
                $"\nThis is done by looking at the fileName of the stubbed shader, and finding a YAML shader with the same fileName (A YAML shader in this case being a shader with the .asset extension instead of .shader)" +
                $"\nthis process is not guaranteed to work constantly and or find all the keys.";
            attemptToFinish.style.maxWidth = new StyleLength(new Length(250));
            attemptToFinish.clicked += AttemptToFinishDictionaryAutomatically;
            rootElement.Add(attemptToFinish);

            var shaderPair = CreateStandardField(nameof(shaderPairs));
            shaderPair.tooltip = $"The ShaderPairs that are used for the Dictionary system in MSEU's SwapShadersAndStageAssetBundles pipeline." +
                $"\n The original shader should be the YAML exported shader from AssetRipper" +
                $"\nthe stubbed shader can be either a default stubbed shader from MSEU, or a stubbed shader from the Dummy shader exporter of AssetRipper.";
            shaderPair.Q(null, "thunderkit-field-input").style.maxWidth = new StyleLength(new Length(100f, LengthUnit.Percent));
            rootElement.Add(shaderPair);

            rootElement.Bind(shaderDictionarySO);
        }
        private void FillWithDefaultShaders()
        {
            string rootPath = AssetDatabase.GUIDToAssetPath(ShaderRootGUID);
            string fullPath = Path.GetFullPath(rootPath);
            string pathWithoutFile = fullPath.Replace(Path.GetFileName(fullPath), "");
            IEnumerable<string> files = Directory.EnumerateFiles(pathWithoutFile, "*.shader", SearchOption.AllDirectories);
            shaderPairs = files.Select(ModifyPath)
                .Select(path => FileUtil.GetProjectRelativePath(path.Replace("\\", "/")))
                .Select(relativePath => AssetDatabase.LoadAssetAtPath<Shader>(relativePath))
                .Select(shader => new ShaderPair(null, shader)).ToList();

            shaderDictionarySO.ApplyModifiedProperties();
        }

        private string ModifyPath(string path)
        {
            if (path.Contains($"Packages\\MoonstormSharedEditorUtils"))
            {
                return path.Replace($"Packages\\MoonstormSharedEditorUtils", "Packages\\teammoonstorm-moonstormsharededitorutils");
            }
            return path;
        }

        private void AttemptToFinishDictionaryAutomatically()
        {
            Shader[] allYAMLShaders = RoR2EditorKit.Utilities.AssetDatabaseUtils.FindAssetsByType<Shader>()
                .Where(shader => AssetDatabase.GetAssetPath(shader).EndsWith(".asset")).ToArray();

            foreach (ShaderPair pair in shaderPairs)
            {
                if (pair.original || !pair.stubbed)
                    continue;

                string stubbedShaderFileName = Path.GetFileName(AssetDatabase.GetAssetPath(pair.stubbed));
                string origShaderFileName = stubbedShaderFileName.Replace(".shader", ".asset");

                Shader origShader = allYAMLShaders.FirstOrDefault(shader =>
                {
                    string yamlShaderFileName = Path.GetFileName(AssetDatabase.GetAssetPath(shader));
                    if (string.Compare(yamlShaderFileName, origShaderFileName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                    return false;
                });

                if (!origShader)
                    continue;

                pair.original = origShader;
            }

            shaderDictionarySO.ApplyModifiedProperties();
        }
    }
}
