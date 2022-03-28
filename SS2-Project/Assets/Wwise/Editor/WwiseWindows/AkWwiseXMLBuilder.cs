﻿#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

[UnityEditor.InitializeOnLoad]
public class AkWwiseXMLBuilder
{
	private static readonly System.DateTime s_LastParsed = System.DateTime.MinValue;

	static AkWwiseXMLBuilder()
	{
		AkWwiseXMLWatcher.Instance.PopulateXML = Populate;
		UnityEditor.EditorApplication.playModeStateChanged += PlayModeChanged;
	}

	private static void PlayModeChanged(UnityEditor.PlayModeStateChange mode)
	{
		if (mode == UnityEditor.PlayModeStateChange.EnteredEditMode)
		{
			AkWwiseProjectInfo.Populate();
			AkWwiseXMLWatcher.Instance.StartWatcher();
		}
	}

	public static bool Populate()
	{
		if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || UnityEditor.EditorApplication.isCompiling)
		{
			return false;
		}

		try
		{
			// Try getting the SoundbanksInfo.xml file for Windows or Mac first, then try to find any other available platform.
			var logWarnings = AkBasePathGetter.LogWarnings;
			AkBasePathGetter.LogWarnings = false;
			var FullSoundbankPath = AkBasePathGetter.GetPlatformBasePath();
			AkBasePathGetter.LogWarnings = logWarnings;

			var filename = System.IO.Path.Combine(FullSoundbankPath, "SoundbanksInfo.xml");
			if (!System.IO.File.Exists(filename))
			{
				FullSoundbankPath = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, AkWwiseEditorSettings.Instance.SoundbankPath);

				if (!System.IO.Directory.Exists(FullSoundbankPath))
				{
					UnityEngine.Debug.Log("WwiseUnity: Could not open SoundbanksInfo.xml, generated SoundBanks path does not exist: " + FullSoundbankPath);
					return false;
				}

				var foundFiles = System.IO.Directory.GetFiles(FullSoundbankPath, "SoundbanksInfo.xml", System.IO.SearchOption.AllDirectories);
				if (foundFiles.Length == 0)
				{
					UnityEngine.Debug.Log("WwiseUnity: Could not find SoundbanksInfo.xml in directory: " + FullSoundbankPath);
					return false;
				}
				filename = foundFiles[0];
			}

			var time = System.IO.File.GetLastWriteTime(filename);
			if (time <= s_LastParsed)
			{
				UnityEngine.Debug.Log("WwiseUnity: Skipping parsing of SoundbanksInfo.xml because it has not changed.");
				return false;
			}

			var doc = new System.Xml.XmlDocument();
			doc.Load(filename);

			var bChanged = false;
			var soundBanks = doc.GetElementsByTagName("SoundBanks");
			for (var i = 0; i < soundBanks.Count; i++)
			{
				var soundBank = soundBanks[i].SelectNodes("SoundBank");
				for (var j = 0; j < soundBank.Count; j++)
				{
					bChanged = SerialiseSoundBank(soundBank[j]) || bChanged;
				}
			}

			return bChanged;
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.Log("WwiseUnity: Exception occured while parsing SoundbanksInfo.xml: " + e.ToString());
			return false;
		}
	}

	private static bool SerialiseSoundBank(System.Xml.XmlNode node)
	{
		var bChanged = false;
		var includedEvents = node.SelectNodes("IncludedEvents");
		for (var i = 0; i < includedEvents.Count; i++)
		{
			var events = includedEvents[i].SelectNodes("Event");
			for (var j = 0; j < events.Count; j++)
			{
				bChanged = SerialiseEventData(events[j]) || bChanged;
			}
		}

		return bChanged;
	}

	private static float GetFloatFromString(string s)
	{
		if (string.Compare(s, "Infinite") == 0)
		{
			return UnityEngine.Mathf.Infinity;
		}
		else
		{
			System.Globalization.CultureInfo CultInfo = System.Globalization.CultureInfo.CurrentCulture.Clone() as System.Globalization.CultureInfo;
			CultInfo.NumberFormat.NumberDecimalSeparator = ".";
			CultInfo.NumberFormat.CurrencyDecimalSeparator = ".";
			float Result;
			if(float.TryParse(s, System.Globalization.NumberStyles.Float, CultInfo, out Result))
			{
				return Result;
			}
			else
			{
				UnityEngine.Debug.Log("WwiseUnity: Could not parse float number " + s);
				return 0.0f;
			}
		}
	}

	private static bool SerialiseEventData(System.Xml.XmlNode node)
	{
		var maxAttenuationAttribute = node.Attributes["MaxAttenuation"];
		var durationMinAttribute = node.Attributes["DurationMin"];
		var durationMaxAttribute = node.Attributes["DurationMax"];
		var name = node.Attributes["Name"].InnerText;
		if (maxAttenuationAttribute == null && durationMinAttribute == null && durationMaxAttribute == null)
		{
			return false;
		}

		var bChanged = false;
		foreach (var wwu in AkWwiseProjectInfo.GetData().EventWwu)
		{
			var eventData = wwu.Find(name);
			if (eventData == null)
				continue;

			if (maxAttenuationAttribute != null)
			{
				var maxAttenuation = GetFloatFromString(maxAttenuationAttribute.InnerText);
				if (eventData.maxAttenuation != maxAttenuation)
				{
					eventData.maxAttenuation = maxAttenuation;
					bChanged = true;
				}
			}

			if (durationMinAttribute != null)
			{
				var minDuration = GetFloatFromString(durationMinAttribute.InnerText);
				if (eventData.minDuration != minDuration)
				{
					eventData.minDuration = minDuration;
					bChanged = true;
				}
			}

			if (durationMaxAttribute != null)
			{
				var maxDuration = GetFloatFromString(durationMaxAttribute.InnerText);
				if (eventData.maxDuration != maxDuration)
				{
					eventData.maxDuration = maxDuration;
					bChanged = true;
				}
			}
		}
		return bChanged;
	}
}
#endif