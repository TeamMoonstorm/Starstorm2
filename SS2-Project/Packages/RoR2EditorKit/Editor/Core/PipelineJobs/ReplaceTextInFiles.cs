using RoR2EditorKit.Core.ManifestDatums;
using System.Threading.Tasks;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine;
using ThunderKit.Core.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ThunderKit.Core.Data;
using RoR2EditorKit.Utilities;

namespace RoR2EditorKit.Core.PipelineJobs
{
    [PipelineSupport(typeof(Pipeline)), RequiresManifestDatumType(typeof(ReplaceTextInFiles))]
    public class ReplaceTextInFiles : PipelineJob
    {
        public string textToReplace;
        public string replacementText;
        public override Task Execute(Pipeline pipeline)
        {
            AssetDatabase.SaveAssets();

            var manifests = pipeline.Manifests;

            var files = new List<ReplaceTextInFilesDatum>();
            for(int i = 0; i < manifests.Length; i++)
            {
                foreach(var textInFilesDatum in manifests[i].Data.OfType<ReplaceTextInFilesDatum>())
                {
                    files.Add(textInFilesDatum);
                }
            }

            var textInFilesDatums = files.ToArray();
            var explicitAssets = files
                .SelectMany(f => f.Objects)
                .ToArray();

            var explicitAssetPaths = new List<string>();

            PopulateWithExplicitAssets(explicitAssets, explicitAssetPaths);

            for(int defIndex = 0; defIndex < files.Count; defIndex++)
            {
                var textFileDatum = textInFilesDatums[defIndex];
                var assets = new List<string>();
                var firstAsset = textFileDatum.Objects.FirstOrDefault(x => x is SceneAsset);

                if (firstAsset != null) assets.Add(AssetDatabase.GetAssetPath(firstAsset));
                else
                {
                    PopulateWithExplicitAssets(textFileDatum.Objects, assets);
                }

                foreach(string assetPath in explicitAssetPaths.Where(p => !p.IsNullOrEmptyOrWhitespace()))
                {
                    string[] text = File.ReadAllLines(assetPath);
                    List<string> newText = new List<string>();

                    for(int i = 0; i < text.Length; i++)
                    {
                        if(text[i].Contains(textToReplace))
                        {
                            pipeline.Log(LogLevel.Information, $"Replacing {textToReplace} (Line {i}) with {replacementText} {AssetDatabase.LoadAssetAtPath<Object>(assetPath)}");
                            newText.Add(text[i].Replace(textToReplace, replacementText));
                        }
                        else
                        {
                            newText.Add(text[i]);
                        }
                    }
                    File.WriteAllLines(assetPath, newText.ToArray());
                }
            }

            return Task.CompletedTask;
        }

        private static void PopulateWithExplicitAssets(IEnumerable<Object> inputAssets, List<string> outputAssets)
        {
            foreach (var asset in inputAssets)
            {
                var assetPath = AssetDatabase.GetAssetPath(asset);

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    var files = Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories);
                    var assets = files.Select(path => AssetDatabase.LoadAssetAtPath<Object>(path));
                    PopulateWithExplicitAssets(assets, outputAssets);
                }
                else if (asset is UnityPackage up)
                {
                    PopulateWithExplicitAssets(up.AssetFiles, outputAssets);
                }
                else
                {
                    outputAssets.Add(assetPath);
                }
            }
        }
    }
}
