#if UNITY_EDITOR
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

using System.Threading;

public class AkWwiseSoundbanksInfoXMLFileWatcher
{
	private static readonly AkWwiseSoundbanksInfoXMLFileWatcher instance = new AkWwiseSoundbanksInfoXMLFileWatcher();
	public static AkWwiseSoundbanksInfoXMLFileWatcher Instance { get { return instance; } }

	public event System.Action XMLUpdated;
	public System.Func<bool> PopulateXML;
	private string generatedSoundbanksPath;

	private const int SecondsBetweenChecks = 3;
	private static System.DateTime s_lastFileCheck = System.DateTime.Now.AddSeconds(-SecondsBetweenChecks);
	private static System.DateTime s_lastXmlFileCheck = System.DateTime.MinValue;

	private AkWwiseSoundbanksInfoXMLFileWatcher()
	{
		if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}

		StartWatcher();
	}

	public void StartWatcher()
	{
		generatedSoundbanksPath = AkBasePathGetter.GetPlatformBasePath();
		UnityEditor.EditorApplication.update += OnEditorUpdate;
	}

	private void OnEditorUpdate()
	{
		if (System.DateTime.Now.Subtract(s_lastFileCheck).Seconds > SecondsBetweenChecks &&
		    !UnityEditor.EditorApplication.isCompiling && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
		{
			HandleXmlChange();
			s_lastFileCheck = System.DateTime.Now;
		}
	}

	private void HandleXmlChange()
	{
		var logWarnings = AkBasePathGetter.LogWarnings;
		AkBasePathGetter.LogWarnings = false;
		generatedSoundbanksPath = AkBasePathGetter.GetPlatformBasePath();
		AkBasePathGetter.LogWarnings = logWarnings;

		var filename = System.IO.Path.Combine(generatedSoundbanksPath, "SoundbanksInfo.xml");
		var time = System.IO.File.GetLastWriteTime(filename);
		if (time > s_lastXmlFileCheck)
		{
			s_lastXmlFileCheck = time;
			var populate = PopulateXML;
			if (populate == null || !populate())
				return;

			var callback = XMLUpdated;
			if (callback != null)
			{
				callback();
			}
		}
	}
}
#endif
