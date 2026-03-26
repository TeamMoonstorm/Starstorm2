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
using UnityEditor;

[UnityEditor.InitializeOnLoad]
public class AkLinuxPluginActivator : AkPlatformPluginActivator
{
	public override string WwisePlatformName => "Linux";
	public override string PluginDirectoryName => "Linux";

	static AkLinuxPluginActivator()
	{
		if (UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
		{
			return;
		}

#if !UNITY_2019_2_OR_NEWER
		AkPluginActivator.RegisterPlatformPluginActivator(BuildTarget.StandaloneLinuxUniversal, new AkLinuxPluginActivator());
#endif
		AkPluginActivator.RegisterPlatformPluginActivator(BuildTarget.StandaloneLinux64, new AkLinuxPluginActivator());
	}
	
	private const int ARCH_INDEX = 1;
	private const int CONFIG_INDEX = 2;
	public override AkPluginActivator.PluginImporterInformation GetPluginImporterInformation(PluginImporter pluginImporter)
	{
		var parts = GetPluginPathParts(pluginImporter.assetPath);
		return new AkPluginActivator.PluginImporterInformation
		{
			PluginConfig = parts[CONFIG_INDEX],
			PluginArch = parts[ARCH_INDEX]
		};
	}

	internal override bool ConfigurePlugin(PluginImporter pluginImporter, AkPluginActivator.PluginImporterInformation pluginImporterInformation)
	{
		if (pluginImporterInformation.PluginArch != "x86" && pluginImporterInformation.PluginArch != "x86_64")
		{
			UnityEngine.Debug.Log("WwiseUnity: Architecture not found: " + pluginImporterInformation.PluginArch);
			return false;
		}
		
		pluginImporter.SetPlatformData(BuildTarget.StandaloneLinux64, "CPU", pluginImporterInformation.IsX64 ? "x86_64" : "None");
		pluginImporter.SetPlatformData(BuildTarget.StandaloneWindows, "CPU", "None");
		pluginImporter.SetPlatformData(BuildTarget.StandaloneWindows64, "CPU", "None");
		pluginImporter.SetPlatformData(BuildTarget.StandaloneOSX, "CPU", "None");
		return true;
	}
}
#endif