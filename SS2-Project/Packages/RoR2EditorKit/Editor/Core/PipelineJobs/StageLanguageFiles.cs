using System.Threading.Tasks;
using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Attributes;
using RoR2EditorKit.Core.ManifestDatums;
using System.Linq;
using System;
using System.IO;
using ThunderKit.Core.Paths;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using SimpleJSON;
using RoR2EditorKit.Utilities;

namespace RoR2EditorKit.Core.PipelineJobs
{
    [PipelineSupport(typeof(Pipeline)), RequiresManifestDatumType(typeof(LanguageFolderTree)), ManifestProcessor]
    public class StageLanguageFiles : PipelineJob
    {
        public enum InvalidLangfile
        {
            InvalidExtension,
            FormatError,
            NodeError
        }

        [PathReferenceResolver]
        public string LanguageArtifactPath = "<LanguageStaging>";
        public override Task Execute(Pipeline pipeline)
        {
            AssetDatabase.SaveAssets();

            var mainManifest = pipeline.Manifest;
            var folderTrees = mainManifest.Data.OfType<LanguageFolderTree>().ToList();

            var hasValidTreeFolders = folderTrees.Any(ft => ft.languageFolders.Any(lf => !lf.languageName.IsNullOrEmptyOrWhitespace() && lf.languageFiles.Any()));
            if (!hasValidTreeFolders)
            {
                var scriptPath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
                pipeline.Log(LogLevel.Warning, $"No valid LanguageTreeFolder defined, skipping {MarkdownUtils.GenerateAssetLink(nameof(StageLanguageFiles), scriptPath)} PipelineJob");
                return Task.CompletedTask;
            }

            var languageArtifactPath = PathReference.ResolvePath(LanguageArtifactPath, pipeline, this);
            IOUtils.EnsureDirectory(languageArtifactPath);

            var allLanguageFiles = folderTrees
                .SelectMany(lft => lft.languageFolders)
                .SelectMany(lf => lf.languageFiles)
                .ToArray();

            var logBuilder = new List<string>();
            if (CheckForInvalidFiles(allLanguageFiles, out var invalidFiles))
            {

                foreach (var (asset, error) in invalidFiles)
                {
                    var scriptPath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
                    switch (error)
                    {
                        case InvalidLangfile.InvalidExtension:
                            logBuilder.Add($"* File {MarkdownUtils.GenerateAssetLink(asset)} has an invalid extension ({Path.GetExtension(AssetDatabase.GetAssetPath(asset))}), extension should be either **\".txt\"** or **\".json\"**, skipping {MarkdownUtils.GenerateAssetLink(nameof(StageLanguageFiles), scriptPath)} PipelineJob");
                            break;
                        case InvalidLangfile.FormatError:
                            logBuilder.Add($"* File {MarkdownUtils.GenerateAssetLink(asset)} has JSON related format errors, skipping {MarkdownUtils.GenerateAssetLink(nameof(StageLanguageFiles), scriptPath)} PipelineJob");
                            break;
                        case InvalidLangfile.NodeError:
                            logBuilder.Add($"* File {MarkdownUtils.GenerateAssetLink(asset)} has an invalid JSON Node, the name for the json dictionary needs to be **\"strings\"**, skipping {MarkdownUtils.GenerateAssetLink(nameof(StageLanguageFiles), scriptPath)} PipelineJob");
                            break;
                    }
                }
                pipeline.Log(LogLevel.Warning, $"Found a total of {invalidFiles.Length} invalid language files on manifest {MarkdownUtils.GenerateAssetLink(mainManifest)}", logBuilder.ToArray());
                logBuilder.Clear();
                return Task.CompletedTask;
            }

            foreach (var languageFolderTree in folderTrees)
            {
                string rootLanguageFolderPath = Path.Combine(languageArtifactPath, languageFolderTree.rootFolderName);
                IOUtils.EnsureDirectory(rootLanguageFolderPath);

                foreach (LanguageFolder languageFolder in languageFolderTree.languageFolders)
                {
                    string langFolder = Path.Combine(rootLanguageFolderPath, languageFolder.languageName);
                    IOUtils.EnsureDirectory(langFolder);
                    foreach (TextAsset asset in languageFolder.languageFiles)
                    {
                        string relativeAssetPath = AssetDatabase.GetAssetPath(asset);
                        string fullAssetPath = Path.GetFullPath(relativeAssetPath);
                        string fullAssetName = Path.GetFileName(fullAssetPath);
                        string destPath = Path.Combine(langFolder, fullAssetName);
                        FileUtil.ReplaceFile(relativeAssetPath, destPath);
                        logBuilder.Add($"* Moved {MarkdownUtils.GenerateAssetLink(asset)} from ***{relativeAssetPath}*** to ***{destPath}***");
                    }
                }
            }
            pipeline.Log(LogLevel.Information, $"Finished moving language files to {LanguageArtifactPath} ({languageArtifactPath})", logBuilder.ToArray());
            logBuilder.Clear();

            foreach (var languageFolderTree in folderTrees)
            {
                var languageNames = languageFolderTree.languageFolders.Select(lf => lf.languageName).ToArray();
                foreach(var outputPath in languageFolderTree.StagingPaths.Select(path => path.Resolve(pipeline, this)))
                {
                    foreach (string dirPath in Directory.GetDirectories(languageArtifactPath, "*", SearchOption.AllDirectories))
                        IOUtils.EnsureDirectory(dirPath.Replace(languageArtifactPath, outputPath));

                    foreach(string filePath in Directory.GetFiles(languageArtifactPath, "*", SearchOption.AllDirectories))
                    {
                        bool found = false;
                        foreach(var languageName in languageNames)
                        {
                            if(filePath.IndexOf(languageName, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            continue;

                        string destFileName = filePath.Replace(languageArtifactPath, outputPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
                        FileUtil.ReplaceFile(filePath, destFileName);
                        logBuilder.Add($"* Moved {Path.GetFileName(filePath)} from ***{filePath}*** to ***{destFileName}***");
                    }
                }
            }
            pipeline.Log(LogLevel.Information, $"Finished moving language files to manifest's Staging Paths.", logBuilder.ToArray());
            return Task.CompletedTask;
        }

        private bool CheckForInvalidFiles(TextAsset[] assets, out (TextAsset, InvalidLangfile)[] invalidLangTuple)
        {
            List<(TextAsset, InvalidLangfile)> invalidLangTupleList = new List<(TextAsset, InvalidLangfile)>();
            foreach (TextAsset asset in assets)
            {
                string assetPath = Path.GetFullPath(AssetDatabase.GetAssetPath(asset));
                string fileExtension = Path.GetExtension(Path.GetFileName(assetPath));
                if (!MatchesExtension(fileExtension, ".txt") && !MatchesExtension(fileExtension, ".json"))
                {
                    invalidLangTupleList.Add((asset, InvalidLangfile.InvalidExtension));
                    continue;
                }

                using (Stream stream = File.Open(assetPath, FileMode.Open, FileAccess.Read))
                using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
                {
                    JSONNode node = JSON.Parse(streamReader.ReadToEnd());

                    if (node == null)
                    {
                        invalidLangTupleList.Add((asset, InvalidLangfile.FormatError));
                        continue;
                    }

                    JSONNode node2 = node["strings"];
                    if(node2 == null)
                    {
                        invalidLangTupleList.Add((asset, InvalidLangfile.NodeError));
                        continue;
                    }
                }
            }

            invalidLangTuple = invalidLangTupleList.ToArray();
            return invalidLangTupleList.Count > 0;

            bool MatchesExtension(string fileExtension, string extension)
            {
                return string.Compare(fileExtension, extension, StringComparison.OrdinalIgnoreCase) == 0;
            }
        }
    }
}