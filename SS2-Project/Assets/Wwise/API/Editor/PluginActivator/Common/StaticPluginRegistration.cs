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
using System.IO;

internal class StaticPluginRegistration
{
	private readonly System.Collections.Generic.HashSet<string> FactoriesHeaderFilenames =
		new System.Collections.Generic.HashSet<string>();

	private readonly UnityEditor.BuildTarget Target;

	public static void Setup(UnityEditor.BuildTarget target, AkPlatformPluginActivator platformPluginActivator)
	{
		if (!platformPluginActivator.RequiresStaticPluginRegistration)
			return;

		string deploymentTargetName = platformPluginActivator.WwisePlatformName;

		var staticPluginRegistration = new StaticPluginRegistration(target);
		var importers = AkPluginActivator.GetWwisePluginImporters(platformPluginActivator.PluginDirectoryName);
		foreach (var pluginImporter in importers)
		{
			var pluginInfo = platformPluginActivator.GetPluginImporterInformation(pluginImporter);
			var pluginPlatform = AkPluginActivator.GetPluginInfoPlatform(pluginImporter.assetPath);
			if (pluginPlatform != platformPluginActivator.PluginDirectoryName)
				continue;

			if (platformPluginActivator.Architectures != null && platformPluginActivator.Architectures.Count != 0)
			{
				if (!platformPluginActivator.Architectures.Contains(pluginInfo.PluginArch))
				{
					UnityEngine.Debug.Log("WwiseUnity: Architecture not found: " + pluginInfo.PluginArch);
					continue;
				}
			}

			if (pluginInfo.PluginConfig != "DSP")
				continue;

			if (!AkPlatformPluginList.IsPluginUsed(platformPluginActivator, pluginPlatform, Path.GetFileNameWithoutExtension(pluginImporter.assetPath)))
				continue;

			staticPluginRegistration.TryAddLibrary(platformPluginActivator, pluginImporter.assetPath);
		}

		System.Collections.Generic.HashSet<AkPluginInfo> plugins;
		AkPlatformPluginList.GetPluginsUsedForPlatform(deploymentTargetName, out plugins);
		var missingPlugins = staticPluginRegistration.GetMissingPlugins(plugins);
		if (missingPlugins.Count == 0)
		{
			if (plugins == null)
				UnityEngine.Debug.LogWarningFormat("WwiseUnity: The activated Wwise plug-ins may not be correct. Could not read PluginInfo.xml for platform: {0}", deploymentTargetName);
			
			staticPluginRegistration.TryWriteToFile(platformPluginActivator);
		}
		else
		{
			UnityEngine.Debug.LogErrorFormat(
				"WwiseUnity: These plugins used by the Wwise project are missing from the Unity project: {0}. Please check folder Assets/Wwise/API/Runtime/Plugin/{1}.",
				string.Join(", ", missingPlugins.ToArray()), deploymentTargetName);
		}
	}
	
	private StaticPluginRegistration(UnityEditor.BuildTarget target)
	{
		Target = target;
	}

	private void TryAddLibrary(AkPlatformPluginActivator config, string AssetPath)
	{
		if (AssetPath.EndsWith(".a"))
		{
			//Extract the lib name, generate the registration code.
			var begin = AssetPath.LastIndexOf('/') + 4;
			var end = AssetPath.LastIndexOf('.') - begin;
			var LibName = AssetPath.Substring(begin, end); //Remove the lib prefix and the .a extension

			if (!LibName.Contains("AkSoundEngine"))
			{
				string headerFilename = LibName + "Factory.h";

				string fullPath = Path.GetFullPath(AkUtilities.GetPathInPackage(Path.Combine(AkPluginActivatorConstants.WwisePluginFolder, config.DSPDirectoryPath, headerFilename)));

				if (File.Exists(fullPath))
				{
					FactoriesHeaderFilenames.Add(headerFilename);
				}
				else
				{
					UnityEngine.Debug.LogErrorFormat("WwiseUnity: Could not find '{0}', required for building plugin.", fullPath);
				}
			}
		}
		else if (AssetPath.EndsWith("Factory.h"))
		{
			FactoriesHeaderFilenames.Add(Path.GetFileName(AssetPath));
		}
	}

	private void TryWriteToFile(AkPlatformPluginActivator config)
	{
		System.Text.StringBuilder CppText = new System.Text.StringBuilder(2000);

		string RelativePath = Path.Combine(config.DSPDirectoryPath, config.StaticPluginRegistrationName + ".cpp");
		CppText.AppendLine("#define " + config.StaticPluginDefine);

		// The purpose of this cpp file is to force the linker to recognize the usage of AK::PluginRegistration global objects
		// so that the static constructors for these objects are executed when the binary is being loaded in.
		// However, some platforms (e.g. WebGL) have a really aggressive LTO (link-time optimization) pass that will strip these
		// symbols even when they are defined as extern here.
		// To avoid any stripping, we call from C# a native function (AkVerifyPluginRegistration)
		// that looks at these symbols, forcing the linker to recognize that these symbols are required for proper program execution.

		CppText.AppendLine(@"namespace AK { class PluginRegistration; };");
		CppText.AppendLine(@"class AkUnityStaticPlugin;");
		CppText.AppendLine(@"AkUnityStaticPlugin * g_pAkUnityStaticPluginList = nullptr;");
		CppText.AppendLine(@"class AkUnityStaticPlugin {");
		CppText.AppendLine("\tpublic:");
		CppText.AppendLine("\tAkUnityStaticPlugin(AK::PluginRegistration* pReg) : m_pNext(g_pAkUnityStaticPluginList), m_pReg(pReg) { g_pAkUnityStaticPluginList = this; }");
		CppText.AppendLine("\tAkUnityStaticPlugin *m_pNext;");
		CppText.AppendLine("\tAK::PluginRegistration * m_pReg;");
		CppText.AppendLine(@"};");

		CppText.AppendLine(@"#define AK_STATIC_LINK_PLUGIN(_pluginName_) \");
		CppText.AppendLine(@"extern AK::PluginRegistration _pluginName_##Registration; \");
		CppText.AppendLine(@"AkUnityStaticPlugin _pluginName_##UnityStaticPlugin(&_pluginName_##Registration);");

		foreach (var filename in FactoriesHeaderFilenames)
		{
			CppText.AppendLine("#include \"" + filename + "\"");
		}
		CppText.AppendLine("extern \"C\" {");
		CppText.AppendLine("\t__attribute__ ((visibility(\"default\"))) bool AkVerifyPluginRegistration() {");
		CppText.AppendLine("\t\tbool bReg = true;");
		CppText.AppendLine("\t\tAkUnityStaticPlugin * pNext = g_pAkUnityStaticPluginList;");
		CppText.AppendLine("\t\twhile (pNext != nullptr) { bReg = bReg && pNext->m_pReg != nullptr; pNext = pNext->m_pNext; }");
		CppText.AppendLine("\t\treturn bReg;");
		CppText.AppendLine("\t}");
		CppText.AppendLine("}");

		try
		{
			var FullPath = Path.GetFullPath(AkUtilities.GetPathInPackage(Path.Combine(AkPluginActivatorConstants.WwisePluginFolder, RelativePath)));
			File.WriteAllText(FullPath, CppText.ToString());
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Could not write <" + RelativePath + ">. Exception: " + e.Message);
			return;
		}

		UnityEditor.AssetDatabase.Refresh();
	}

	private System.Collections.Generic.List<string> GetMissingPlugins(System.Collections.Generic.HashSet<AkPluginInfo> usedPlugins)
	{
		var pluginList = new System.Collections.Generic.List<string>();
		if (usedPlugins == null)
			return pluginList;

		foreach (var plugin in usedPlugins)
		{
			if (string.IsNullOrEmpty(plugin.StaticLibName))
			{
				continue;
			}

			string includeFilename = plugin.StaticLibName + "Factory.h";
			if (!FactoriesHeaderFilenames.Contains(includeFilename))
			{
				pluginList.Add(plugin.StaticLibName);
			}
		}

		return pluginList;
	}
}
#endif