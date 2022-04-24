using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Manifests.Datums;
using UnityEngine;
using ThunderKit.Core.Paths;
using UnityEngine.Networking;
using UnityEditor;

namespace RoR2EditorKit.Core.PipelineJobs
{
    [PipelineSupport(typeof(Pipeline)), RequiresManifestDatumType(typeof(AssemblyDefinitions))]
    public class WeaveAssemblies : PipelineJob
    {
        [PathReferenceResolver, Tooltip("Location where built assemblies will be cached before being staged")]
        public string assemblyArtifactPath = "<AssemblyStaging>";
        public override Task Execute(Pipeline pipeline)
        {
            var resolvedArtifactPath = PathReference.ResolvePath(assemblyArtifactPath, pipeline, this);

            var definitionDatums = pipeline.Manifest.Data.OfType<AssemblyDefinitions>().ToArray();
            if (!definitionDatums.Any())
            {
                var scriptPath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
                pipeline.Log(LogLevel.Warning, $"No AssemblyDefinitions found, skipping.");
                return Task.CompletedTask;
            }

            for(int i = 0; i < definitionDatums.Length; i++)
            {
                var datum = definitionDatums[i];
                if (!datum) 
                    continue;

                var hasUnassignedDefinition = datum.definitions.Any(def => !(bool)def);
                if (hasUnassignedDefinition)
                    pipeline.Log(LogLevel.Warning, $"AssemblyDefinitions with unassigned definition at index {i}");
            }

            return Task.CompletedTask;
        }
    }
}