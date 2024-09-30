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

﻿#if (UNITY_STANDALONE_OSX && !UNITY_EDITOR) || UNITY_EDITOR_OSX
public partial class AkCommonUserSettings
{
	partial void SetSampleRate(AkPlatformInitSettings settings)
	{
		settings.uSampleRate = m_SampleRate;
	}
}
#endif

public class AkMacSettings : AkWwiseInitializationSettings.PlatformSettings
{
#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoadMethod]
	private static void AutomaticPlatformRegistration()
	{
		if (UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
		{
			return;
		}

		RegisterPlatformSettingsClass<AkMacSettings>("Mac");
	}
#endif // UNITY_EDITOR

	[System.Serializable]
	public class PlatformAdvancedSettings : AkCommonAdvancedSettings
	{
		[UnityEngine.Tooltip("Number of Apple Spatial Audio point sources to allocate for 3D audio use (each point source is a system audio object).")]
		public uint NumSpatialAudioPointSources = 128;
		
		[UnityEngine.Tooltip("Print debug information related to audio device initialization in the system log.")]
		public bool VerboseSystemOutput = false;
		
		public override void CopyTo(AkPlatformInitSettings settings)
		{
#if (UNITY_STANDALONE_OSX && !UNITY_EDITOR) || UNITY_EDITOR_OSX
			settings.uNumSpatialAudioPointSources = NumSpatialAudioPointSources;
			settings.bVerboseSystemOutput = VerboseSystemOutput;
#endif
		}
	}
	
	protected override AkCommonUserSettings GetUserSettings()
	{
		return UserSettings;
	}

	protected override AkCommonAdvancedSettings GetAdvancedSettings()
	{
		return AdvancedSettings;
	}

	protected override AkCommonCommSettings GetCommsSettings()
	{
		return CommsSettings;
	}

	[UnityEngine.HideInInspector]
	public AkCommonUserSettings UserSettings;
	[UnityEngine.HideInInspector]
	public PlatformAdvancedSettings AdvancedSettings;
	[UnityEngine.HideInInspector]
	public AkCommonCommSettings CommsSettings;

}
