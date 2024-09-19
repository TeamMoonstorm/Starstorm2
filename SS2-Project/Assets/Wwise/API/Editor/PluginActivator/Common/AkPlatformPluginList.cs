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
internal class AkPlatformPluginList
{
	private static readonly Dictionary<string, System.DateTime> s_LastParsed = new Dictionary<string, System.DateTime>();

	private static readonly Dictionary<string, HashSet<AkPluginInfo>> s_PerPlatformPlugins = new Dictionary<string, HashSet<AkPluginInfo>>();
	
	internal static void GetPluginsUsedForPlatform(string deploymentTargetName, out HashSet<AkPluginInfo> UsedPlugins)
	{
		s_PerPlatformPlugins.TryGetValue(deploymentTargetName, out UsedPlugins);
	}

	internal static bool ContainsPlatform(string platform)
	{
		return s_PerPlatformPlugins.ContainsKey(platform);
	}

	public static bool IsPluginUsed(AkPlatformPluginActivator in_config, string in_UnityPlatform, string in_PluginName)
	{
		var pluginDSPPlatform = in_config.WwisePlatformName;

		if (!s_PerPlatformPlugins.ContainsKey(pluginDSPPlatform))
		{
			return false; //XML not parsed, don't touch anything.
		}

		if (in_PluginName.Contains("AkSoundEngine"))
		{
			return true;
		}

		var pluginName = in_PluginName;
		if (in_PluginName.StartsWith("lib"))
		{
			pluginName = in_PluginName.Substring(3);
		}

		const string factory = "Factory";
		int indexOfFactory = in_PluginName.IndexOf(factory);
		// Ensure the plugin name ends with "Factory.h"
		if (indexOfFactory != -1 && indexOfFactory + factory.Length == in_PluginName.Length)
		{
			pluginName = in_PluginName.Substring(0, indexOfFactory);
		}

		System.Collections.Generic.HashSet<AkPluginInfo> plugins;
		if (s_PerPlatformPlugins.TryGetValue(pluginDSPPlatform, out plugins))
		{
			if (!in_config.RequiresStaticPluginRegistration)
			{
				foreach (var pluginInfo in plugins)
				{
					if (pluginInfo.DllName == pluginName)
					{
						return true;
					}
				}
			}

			//Exceptions
			if (!string.IsNullOrEmpty(in_config.StaticPluginRegistrationName) && pluginName.Contains(in_config.StaticPluginRegistrationName))
			{
				return true;
			}

			//WebGL, iOS, tvOS, visionOS, and Switch deal with the static libs directly, unlike all other platforms.
			//Luckily the DLL name is always a subset of the lib name.
			foreach (var pluginInfo in plugins)
			{
				if (pluginInfo.StaticLibName == pluginName)
				{
					return true;
				}
			}
		}

		return false;
	}

	public static void Update(bool forceUpdate = false)
	{
		//Gather all GeneratedSoundBanks folder from the project
		var allPaths = AkUtilities.GetAllBankPaths();
		var bNeedRefresh = false;
		var projectDir = AkBasePathGetter.GetWwiseProjectDirectory();
		var baseSoundBankPath = AkBasePathGetter.GetFullSoundBankPathEditor();

		AkWwiseInitializationSettings.UpdatePlatforms();

		//make a copy of the platform map and handle "special" custom platforms
		var platformMap = new Dictionary<string, List<string>>();
		foreach (var key in AkUtilities.PlatformMapping.Keys)
		{
			platformMap.Add(key, new List<string>(AkUtilities.PlatformMapping[key]));
			foreach (var customPF in AkUtilities.PlatformMapping[key])
			{
				if (customPF != key && (AkWwiseInitializationSettings.PlatformSettings.IsDistinctPlatform(customPF)))
				{
					platformMap.Add(customPF, new List<string> { customPF });
					platformMap[key].Remove(customPF);
				}
			}
			if (platformMap[key].Count==0)
			{
				platformMap.Remove(key);
			}
		}


		//Go through all BasePlatforms 
		foreach (var pairPF in platformMap)
		{
			//Go through all custom platforms related to that base platform and check if any of the bank files were updated.
			var bParse = forceUpdate;
			var fullPaths = new System.Collections.Generic.List<string>();
			foreach (var customPF in pairPF.Value)
			{
				string bankPath;
				if (!allPaths.TryGetValue(customPF, out bankPath))
					continue;

				var pluginFile = "";
				try
				{
					pluginFile = System.IO.Path.Combine(projectDir, System.IO.Path.Combine(bankPath, "PluginInfo.xml"));
					pluginFile = pluginFile.Replace('/', System.IO.Path.DirectorySeparatorChar);
					if (!System.IO.File.Exists(pluginFile))
					{
						//Try in StreamingAssets too.
						pluginFile = System.IO.Path.Combine(System.IO.Path.Combine(baseSoundBankPath, customPF), "PluginInfo.xml");
						if (!System.IO.File.Exists(pluginFile))
							continue;
					}

					fullPaths.Add(pluginFile);

					var t = System.IO.File.GetLastWriteTime(pluginFile);
					var lastTime = System.DateTime.MinValue;
					bool bParsedBefore = s_LastParsed.TryGetValue(customPF, out lastTime);
					if (!bParsedBefore || lastTime < t)
					{
						bParse = true;
						s_LastParsed[customPF] = t;
					}
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError("WwiseUnity: " + pluginFile + " could not be parsed. " + ex.Message);
				}
			}

			if (bParse)
			{
				var platform = pairPF.Key;

				var newDlls = ParsePluginsXML(platform, fullPaths);
				System.Collections.Generic.HashSet<AkPluginInfo> oldDlls = null;

				s_PerPlatformPlugins.TryGetValue(platform, out oldDlls);
				s_PerPlatformPlugins[platform] = newDlls;

				//Check if there was any change.
				if (!bNeedRefresh && oldDlls != null)
				{
					if (oldDlls.Count == newDlls.Count)
					{
						oldDlls.IntersectWith(newDlls);
					}

					bNeedRefresh |= oldDlls.Count != newDlls.Count;
				}
				else
				{
					bNeedRefresh |= newDlls.Count > 0;
				}
			}
		}

		if (bNeedRefresh)
		{
			AkPluginActivator.ActivatePluginsForEditor();
		}
	}

	private static HashSet<AkPluginInfo> ParsePluginsXML(string platform, List<string> in_pluginFiles)
	{
		var newPlugins = new System.Collections.Generic.HashSet<AkPluginInfo>();

		foreach (var pluginFile in in_pluginFiles)
		{
			if (!System.IO.File.Exists(pluginFile))
			{
				continue;
			}

			try
			{
				var doc = new System.Xml.XmlDocument();
				doc.Load(pluginFile);
				var Navigator = doc.CreateNavigator();
				var pluginInfoNode = Navigator.SelectSingleNode("//PluginInfo");
				var boolMotion = pluginInfoNode.GetAttribute("Motion", "");

				var it = Navigator.Select("//PluginLib");

				if (boolMotion == "true")
				{
					AkPluginInfo motionPluginInfo = new AkPluginInfo();
					motionPluginInfo.DllName = "AkMotion";
					newPlugins.Add(motionPluginInfo);
				}

				foreach (System.Xml.XPath.XPathNavigator node in it)
				{
					var rawPluginID = uint.Parse(node.GetAttribute("LibId", ""));
					if (rawPluginID == 0)
					{
						continue;
					}

					AkPluginActivatorConstants.PluginID pluginID = (AkPluginActivatorConstants.PluginID)rawPluginID;

					if (AkPluginActivatorConstants.alwaysSkipPluginsIDs.Contains(pluginID))
					{
						continue;
					}

					var dll = string.Empty;

					if (platform == "Switch" || platform == "Web")
					{
						if (pluginID == AkPluginActivatorConstants.PluginID.AkMeter)
						{
							dll = "AkMeter";
						}
					}
					else if (AkPluginActivatorConstants.builtInPluginIDs.Contains(pluginID))
					{
						continue;
					}

					if (string.IsNullOrEmpty(dll))
					{
						dll = node.GetAttribute("DLL", "");
					}

					var staticLibName = node.GetAttribute("StaticLib", "");

					AkPluginInfo newPluginInfo = new AkPluginInfo();
					newPluginInfo.PluginID = rawPluginID;
					newPluginInfo.DllName = dll;
					newPluginInfo.StaticLibName = staticLibName;

					if (string.IsNullOrEmpty(newPluginInfo.StaticLibName) && !AkPluginActivatorConstants.PluginIDToStaticLibName.TryGetValue(pluginID, out newPluginInfo.StaticLibName))
					{
						newPluginInfo.StaticLibName = dll;
					}

					newPlugins.Add(newPluginInfo);
				}
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogError("WwiseUnity: " + pluginFile + " could not be parsed. " + ex.Message);
			}
		}

		return newPlugins;
	}
}
#endif