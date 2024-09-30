/*******************************************************************************
The content of this file includes portions of the proprietary AUDIOKINETIC Wwise
Technology released in source code form as part of the game integration package.
The content of this file may not be used without valid licenses to the
AUDIOKINETIC Wwise Technology.
Note that the use of the game engine is subject to the Unity(R) Terms of
Service at https://unity3d.com/legal/terms-of-service
 
License Usage
 
Licensees holding valid licenses to the AUDIOKINETIC Wwise Technology may use
this file in accordance with the end user license agreement provided with the
software or, alternatively, in accordance with the terms contained
in a written agreement between you and Audiokinetic Inc.
Copyright (c) 2024 Audiokinetic Inc.
*******************************************************************************/

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AkPluginActivator : UnityEditor.AssetPostprocessor
{
	private const string EditorConfiguration = AkPluginActivatorConstants.CONFIG_PROFILE;
	private static bool bIsAlreadyActivating = false;
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
	{
		if (UnityEditor.AssetDatabase.IsAssetImportWorkerProcess() || bIsAlreadyActivating)
		{
			return;
		}

		if (Array.IndexOf(Environment.GetCommandLineArgs(), "-verboseAkPluginActivator") != -1)
		{
            UnityEngine.Debug.Log("Enabling verbose logging!");
			IsVerboseLogging = true;
		}

		if (didDomainReload)
		{
			ActivatePluginsForEditor();	
		}
	}

	public static string GetCurrentConfig()
	{
		var CurrentConfig = AkWwiseProjectInfo.GetData().CurrentPluginConfig;
		if (string.IsNullOrEmpty(CurrentConfig))
		{
			CurrentConfig = AkPluginActivatorConstants.CONFIG_PROFILE;
		}
		
		return CurrentConfig;
	}

	public static Dictionary<BuildTarget, AkPlatformPluginActivator> BuildTargetToPlatformPluginActivator = new Dictionary<BuildTarget, AkPlatformPluginActivator>();

	public static void RegisterPlatformPluginActivator(BuildTarget target, AkPlatformPluginActivator platformPluginActivator)
	{
		LogVerbose("Adding platform " + target.ToString() + " to PluginActivator");
		BuildTargetToPlatformPluginActivator.Add(target, platformPluginActivator);
	}

	internal static string GetPluginInfoPlatform(string path)
	{
		var indexOfPluginFolder = path.IndexOf(AkPluginActivatorConstants.WwisePluginFolder, StringComparison.OrdinalIgnoreCase);
		if (indexOfPluginFolder == -1)
		{
			return null;
		}

		return path.Substring(indexOfPluginFolder + AkPluginActivatorConstants.WwisePluginFolder.Length + 1).Split('/')[0];
	}

	internal static List<PluginImporter> GetWwisePluginImporters(string platformFilter = "")
	{
		PluginImporter[] pluginImporters = PluginImporter.GetAllImporters();
		List<PluginImporter> wwisePlugins = new List<PluginImporter>();
		foreach (var pluginImporter in pluginImporters)
		{
			if (pluginImporter.assetPath.Contains("Wwise/API/"))
			{
				if (string.IsNullOrEmpty(platformFilter) || platformFilter == GetPluginInfoPlatform(pluginImporter.assetPath))
				{
					wwisePlugins.Add(pluginImporter);
				}
			}
		}
		return wwisePlugins;
	}

	public class PluginImporterInformation
	{
		public string PluginName;
		public string PluginArch;
		public string PluginSDKVersion;
		public string PluginConfig;

		public string EditorOS;
		public string EditorCPU;

		public bool IsX86 => PluginArch == "x86";
		public bool IsX64 => PluginArch == "x86_64";

		public bool IsSupportLibrary => AkPluginActivatorConstants.SupportLibraries.Contains(PluginName);
	}
	
	public static void ActivatePluginsForDeployment(BuildTarget target, bool Activate)
	{
		if (!BuildTargetToPlatformPluginActivator.TryGetValue(target, out var platformPluginActivator))
		{
			Debug.LogError("WwiseUnity: Build Target " + target + " not supported.");
			return;
		}

		if (!platformPluginActivator.IsBuildEnvironmentValid())
		{
			Debug.LogError("Build Environment for platform " + platformPluginActivator.WwisePlatformName + " is not valid. Current BuildTarget is " + EditorUserBuildSettings.activeBuildTarget);
			return;
		}

		bIsAlreadyActivating = true;

		if (Activate)
		{
			StaticPluginRegistration.Setup(target, platformPluginActivator);
		}

		var importers = GetWwisePluginImporters();
		var assetChanged = false;
		foreach (var pluginImporter in importers)
		{
			var pluginPlatform = GetPluginInfoPlatform(pluginImporter.assetPath);
			if (pluginPlatform != platformPluginActivator.PluginDirectoryName)
			{
				if (Activate)
				{
					platformPluginActivator.FilterOutPlatformIfNeeded(target, pluginImporter, pluginPlatform);
				}

				continue;
			}

			var pluginInfo = platformPluginActivator.GetPluginImporterInformation(pluginImporter);
			var bShouldActivatePlugin = platformPluginActivator.ConfigurePlugin(pluginImporter, pluginInfo);

			if (pluginImporter.GetCompatibleWithAnyPlatform())
			{
				LogVerbose("Plugin" + pluginImporter.assetPath + " was compatible with the \"any\" platform, deactivating.");
				pluginImporter.SetCompatibleWithAnyPlatform(false);
				assetChanged = true;
			}

			if (pluginInfo.PluginConfig == "DSP")
			{
				if (!pluginInfo.IsSupportLibrary && !AkPlatformPluginList.IsPluginUsed(platformPluginActivator, pluginPlatform, Path.GetFileNameWithoutExtension(pluginImporter.assetPath)))
				{
					LogVerbose("Plugin" + pluginImporter.assetPath + " is not used, skipping.");
					bShouldActivatePlugin = false;
				}
			}
			else if (pluginInfo.PluginConfig != GetCurrentConfig())
			{
				LogVerbose("Plugin" + pluginImporter.assetPath + " does not match current config (" + GetCurrentConfig() + "). Skipping.");
				bShouldActivatePlugin = false;
			}

			if (!string.IsNullOrEmpty(pluginInfo.PluginSDKVersion))
			{
				var sdkCompatible = platformPluginActivator.IsPluginSDKVersionCompatible(pluginInfo.PluginSDKVersion);
				LogVerbose("Plugin " + pluginImporter.assetPath + " is " + (sdkCompatible ? "" : "NOT ") + "compatible with current platform SDK");
				bShouldActivatePlugin &= sdkCompatible;
			}

			bool isCompatibleWithPlatform = bShouldActivatePlugin && Activate;
			LogVerbose("Will set plugin " + pluginImporter.assetPath + " as " + (isCompatibleWithPlatform ? "" : "NOT ") + "compatible with platform.");
			assetChanged |= pluginImporter.GetCompatibleWithPlatform(target) != isCompatibleWithPlatform;

			pluginImporter.SetCompatibleWithPlatform(target, isCompatibleWithPlatform);

			if (assetChanged)
			{
				LogVerbose("Changed plugin " + pluginImporter.assetPath + ", saving and reimporting.");
				pluginImporter.SaveAndReimport();
			}
		}

		bIsAlreadyActivating = false;
	}

	public static void ActivatePluginsForEditor()
	{
		var importers = GetWwisePluginImporters();
		var ChangedSomeAssets = false;

		bIsAlreadyActivating = true;
		foreach (var pluginImporter in importers)
		{
			var pluginPlatform = GetPluginInfoPlatform(pluginImporter.assetPath);
			if (string.IsNullOrEmpty(pluginPlatform) || (pluginPlatform != "Mac" && pluginPlatform != "Windows"))
			{
				continue;
			}

			BuildTarget pluginBuildTarget = pluginPlatform == "Mac" ? BuildTarget.StandaloneOSX : BuildTarget.StandaloneWindows64;
			
			if (!BuildTargetToPlatformPluginActivator.TryGetValue(pluginBuildTarget, out var platformPluginActivator))
			{
				Debug.Log("WwiseUnity: Build Target " + pluginBuildTarget + " not supported.");
				bIsAlreadyActivating = false;
				return;
			}

			var pluginInfo = platformPluginActivator.GetPluginImporterInformation(pluginImporter);
			
			var AssetChanged = false;
			if (pluginImporter.GetCompatibleWithAnyPlatform())
			{
				LogVerbose("ActivatePluginsForEditor: Plugin" + pluginImporter.assetPath + " was compatible with the \"any\" platform, deactivating.");
				pluginImporter.SetCompatibleWithAnyPlatform(false);
				AssetChanged = true;
			}

			var bActivate = false;
			if (!string.IsNullOrEmpty(pluginInfo.EditorOS))
			{
				if (pluginInfo.PluginConfig == "DSP")
				{
					if (!AkPlatformPluginList.ContainsPlatform(platformPluginActivator.WwisePlatformName))
					{
						continue;
					}

					bActivate = AkPlatformPluginList.IsPluginUsed(platformPluginActivator, pluginPlatform,
						Path.GetFileNameWithoutExtension(pluginImporter.assetPath));
				}
				else
				{
					bActivate = pluginInfo.PluginConfig == EditorConfiguration;
				}

				if (bActivate)
				{
					LogVerbose("ActivatePluginsForEditor: Activating " + pluginImporter.assetPath);
					pluginImporter.SetEditorData("CPU", pluginInfo.EditorCPU);
					pluginImporter.SetEditorData("OS", pluginInfo.EditorOS);
				}

				AssetChanged |= pluginImporter.GetCompatibleWithEditor() != bActivate;
				pluginImporter.SetCompatibleWithEditor(bActivate);
			}
			else
			{
				LogVerbose("ActivatePluginsForEditor: Could not determine EditorOS for " + pluginImporter.assetPath);
			}

			if (AssetChanged)
			{
				ChangedSomeAssets = true;
				LogVerbose("ActivatePluginsForEditor: Changed plugin " + pluginImporter.assetPath + ", saving and reimporting.");
				pluginImporter.SaveAndReimport();
			}
		}

		if (ChangedSomeAssets)
		{
			Debug.Log("WwiseUnity: Plugins successfully activated for " + EditorConfiguration + " in Editor.");
		}

		bIsAlreadyActivating = false;
	}

	public static void DeactivateAllPlugins()
	{
		var importers = GetWwisePluginImporters();
		foreach (var pluginImporter in importers)
		{
			if (pluginImporter.assetPath.IndexOf(AkPluginActivatorConstants.WwisePluginFolder, StringComparison.OrdinalIgnoreCase) == -1)
			{
				continue;
			}

			pluginImporter.SetCompatibleWithAnyPlatform(false);
			pluginImporter.SaveAndReimport();
		}
	}

	public static void Update(bool forceUpdate = false)
	{
		AkPlatformPluginList.Update(forceUpdate);
		AkPluginActivatorMenus.CheckMenuItems(GetCurrentConfig());
	}

	public static bool IsVerboseLogging = false;
	public static void LogVerbose(string msg)
	{
		if (IsVerboseLogging)
		{
			Debug.Log("wwiseunity: AkPluginActivator VERBOSE: " + msg);
		}
	}
}
#endif