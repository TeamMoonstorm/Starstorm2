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
public class AkPluginActivatorMenus
{
	private const string MENU_PATH = "Assets/Wwise/Activate Plugins/";

	[UnityEditor.MenuItem(MENU_PATH + AkPluginActivatorConstants.CONFIG_DEBUG)]
	public static void ActivateDebug()
	{
		ActivateConfig(AkPluginActivatorConstants.CONFIG_DEBUG);
	}

	[UnityEditor.MenuItem(MENU_PATH + AkPluginActivatorConstants.CONFIG_PROFILE)]
	public static void ActivateProfile()
	{
		ActivateConfig(AkPluginActivatorConstants.CONFIG_PROFILE);
	}

	[UnityEditor.MenuItem(MENU_PATH + AkPluginActivatorConstants.CONFIG_RELEASE)]
	public static void ActivateRelease()
	{
		ActivateConfig(AkPluginActivatorConstants.CONFIG_RELEASE);
	}

	public static void CheckMenuItems(string config)
	{
		UnityEditor.Menu.SetChecked(MENU_PATH + AkPluginActivatorConstants.CONFIG_DEBUG, config == AkPluginActivatorConstants.CONFIG_DEBUG);
		UnityEditor.Menu.SetChecked(MENU_PATH + AkPluginActivatorConstants.CONFIG_PROFILE, config == AkPluginActivatorConstants.CONFIG_PROFILE);
		UnityEditor.Menu.SetChecked(MENU_PATH + AkPluginActivatorConstants.CONFIG_RELEASE, config == AkPluginActivatorConstants.CONFIG_RELEASE);
	}
	private static void SetCurrentConfig(string config)
	{
		var data = AkWwiseProjectInfo.GetData();
		data.CurrentPluginConfig = config;
		UnityEditor.EditorUtility.SetDirty(data);
	}

	private static void ActivateConfig(string config)
	{
		SetCurrentConfig(config);
		AkPluginActivatorMenus.CheckMenuItems(config);
	}

}
#endif