using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Manifests.Datum;
using RoR2EditorKit.Core.ManifestDatums;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using ThunderKit.Core.Data;
using System.Threading.Tasks;
using System.Text;
using RoR2EditorKit.Utilities;

namespace RoR2EditorKit.Core.PipelineJobs
{
    [PipelineSupport(typeof(Pipeline)), RequiresManifestDatumType(typeof(SetObjectFlagsDatum))]
    public class SetObjectFlags : PipelineJob
    {
        public bool AffectsDependencies = false;
        public HideFlags flags = HideFlags.None;
        public override Task Execute(Pipeline pipeline)
        {
            AssetDatabase.SaveAssets();

            var manifests = pipeline.Manifests;

            var files = new List<SetObjectFlagsDatum>();
            for(int i = 0; i < manifests.Length; i++)
            {
                foreach(var filesDatum in manifests[i].Data.OfType<SetObjectFlagsDatum>())
                {
                    files.Add(filesDatum);
                }
            }

            var filesDatums = files.ToArray();
            var explicitAssets = files
                .SelectMany(f => f.objects)
                .ToArray();

            var explicitAssetPaths = new List<string>();

            PopulateWithExplicitAssets(explicitAssets, explicitAssetPaths);


            for(int defIndex = 0; defIndex < files.Count; defIndex++)
            {
                var fileDatum = filesDatums[defIndex];
                var assets = new List<string>();
                var firstAsset = fileDatum.objects.FirstOrDefault(x => x is SceneAsset);

                if (firstAsset != null) assets.Add(AssetDatabase.GetAssetPath(firstAsset));
                else
                {
                    PopulateWithExplicitAssets(fileDatum.objects, assets);

                    if(AffectsDependencies)
                    {
                        var dependencies = assets.SelectMany(assetPath => AssetDatabase.GetDependencies(assetPath))
                            .Where(dep => !explicitAssetPaths.Contains(dep))
                            .ToArray();
                        assets.AddRange(dependencies);
                    }
                }

                foreach (string assetPath in explicitAssetPaths.Where(p => !p.IsNullOrEmptyOrWhitespace()))
                {
                    Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    pipeline.Log(LogLevel.Information, $"Changing {asset}'s hideFlags ({asset.hideFlags}) to {flags}");
                    asset.hideFlags = flags;
                    new SerializedObject(asset).ApplyModifiedProperties();
                }
                AssetDatabase.SaveAssets();
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
