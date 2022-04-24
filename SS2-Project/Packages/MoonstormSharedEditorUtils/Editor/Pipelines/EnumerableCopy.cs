using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.Pipelines
{
    [PipelineSupport(typeof(Pipeline))]
    public class EnumerableCopy : FlowPipelineJob
    {
        [Serializable]
        public struct PathReferencePair
        {
            [PathReferenceResolver]
            public string pathSource;
            [PathReferenceResolver]
            public string pathDestination;
        }
        public bool recursive;

        [Tooltip("While enabled, will error when the source is not found (default: true)")]
        public bool SourceRequired = true;

        [Tooltip("While enabled, copy will create destination directory if it doesn't already exist")]
        public bool EstablishDestination = true;

        public PathReferencePair[] pathReferencePairs = Array.Empty<PathReferencePair>();

        protected override Task ExecuteInternal(Pipeline pipeline)
        {
            for (int i = 0; i < pathReferencePairs.Length; i++)
            {
                PathReferencePair currentPair = pathReferencePairs[i];
                var copyStatement = $"``` {currentPair.pathSource} ``` to ``` {currentPair.pathDestination} ```\r\n";
                var source = string.Empty;
                try
                {
                    source = currentPair.pathSource.Resolve(pipeline, this);
                }
                catch (Exception e)
                {
                    if (SourceRequired)
                        throw new InvalidOperationException($"{copyStatement} Failed to resolve source when source is required", e);
                }
                if (SourceRequired && string.IsNullOrEmpty(source)) throw new ArgumentException($"{copyStatement} Required {nameof(currentPair.pathSource)} is empty");
                if (!SourceRequired && string.IsNullOrEmpty(source))
                {
                    pipeline.Log(LogLevel.Information, $"{copyStatement} Source not specified and is not required, copy skipped");
                    continue;
                }

                var destination = currentPair.pathDestination.Resolve(pipeline, this);

                bool sourceIsFile = false;

                try
                {
                    sourceIsFile = !File.GetAttributes(source).HasFlag(FileAttributes.Directory);
                }
                catch (Exception e)
                {
                    if (SourceRequired)
                        throw new InvalidOperationException($"{copyStatement} Failed to check {nameof(currentPair.pathSource)} is required", e);
                }

                if (recursive)
                {
                    if (!Directory.Exists(source) && !SourceRequired)
                    {
                        pipeline.Log(LogLevel.Information, $"{copyStatement} Source not found and is not required, copy skipped");
                        continue;
                    }
                    else if (!Directory.Exists(source) && SourceRequired)
                    {
                        throw new ArgumentException($"{copyStatement} Source not found and is required");
                    }
                    else if (sourceIsFile)
                        throw new ArgumentException($"{copyStatement} Expected Directory for recursive copy, Recieved file path: {source}");
                }

                if (EstablishDestination)
                    Directory.CreateDirectory(sourceIsFile ? Path.GetDirectoryName(destination) : destination);

                if (recursive)
                {
                    FileUtil.ReplaceDirectory(source, destination);
                    int j = 1;
                    var copiedFiles = Directory.EnumerateFiles(destination, "*", SearchOption.AllDirectories)
                        .Prepend("Copied Files")
                        .Aggregate((a, b) => $"{a}\r\n\r\n {j++}. {b}");
                    pipeline.Log(LogLevel.Information, $"{copyStatement}\r\n\r\nCopied ``` {source} ````to ``` {destination} ````", copiedFiles);
                }
                else
                {
                    FileUtil.ReplaceDirectory(source, destination);
                    pipeline.Log(LogLevel.Information, $"{copyStatement}\r\n\r\n ``` {source} ``` to ``` {destination} ```");
                }
            }

            return Task.CompletedTask;
        }
    }
}