#if UNITY_EDITOR
#if !UNITY_2019_1_OR_NEWER
#define AK_ENABLE_TIMELINE
#endif
#if AK_ENABLE_TIMELINE

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

[System.Obsolete(AkSoundEngine.Deprecation_2019_2_0)]
[UnityEditor.CustomEditor(typeof(AkEventPlayable))]
public class AkEventPlayableInspector : UnityEditor.Editor
{
	private AkEventPlayable m_AkEventPlayable;
	private UnityEditor.SerializedProperty akEvent;
	private UnityEditor.SerializedProperty emitterObjectRef;
	private UnityEditor.SerializedProperty retriggerEvent;
	private UnityEditor.SerializedProperty UseWwiseEventDuration;
	private UnityEditor.SerializedProperty StopEventAtClipEnd;
	private UnityEditor.SerializedProperty blendInCurve;
	private UnityEditor.SerializedProperty blendOutCurve;

	public void OnEnable()
	{
		m_AkEventPlayable = target as AkEventPlayable;
		if (m_AkEventPlayable == null)
			return;

		akEvent = serializedObject.FindProperty("akEvent");
		emitterObjectRef = serializedObject.FindProperty("emitterObjectRef");
		retriggerEvent = serializedObject.FindProperty("retriggerEvent");
		UseWwiseEventDuration = serializedObject.FindProperty("UseWwiseEventDuration");
		StopEventAtClipEnd = serializedObject.FindProperty("StopEventAtClipEnd");
		blendInCurve = serializedObject.FindProperty("blendInCurve");
		blendOutCurve = serializedObject.FindProperty("blendOutCurve");
	}

	public override void OnInspectorGUI()
	{
		UnityEditor.EditorGUILayout.HelpBox(AkSoundEngine.Deprecation_2019_2_0, UnityEditor.MessageType.Warning);

		serializedObject.Update();

		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			UnityEditor.EditorGUILayout.PropertyField(akEvent, new UnityEngine.GUIContent("Event: "));
			UnityEditor.EditorGUILayout.PropertyField(emitterObjectRef, new UnityEngine.GUIContent("Override Track Object: "));
			UnityEditor.EditorGUILayout.PropertyField(blendInCurve);
			UnityEditor.EditorGUILayout.PropertyField(blendOutCurve);
		}

		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			UnityEditor.EditorGUILayout.PropertyField(UseWwiseEventDuration, new UnityEngine.GUIContent("Use Wwise Event Duration: ", "The clip duration is set to the duration of the Wwise Event"));

			if (!UpdateClipInformation(m_AkEventPlayable.owningClip, m_AkEventPlayable.akEvent, serializedObject, UseWwiseEventDuration.boolValue))
			{
				UnityEditor.EditorGUILayout.HelpBox(string.Format("The duration of the Wwise event \"{0}\" has not been determined. Playback for this event may be inconsistent. " +
					"Ensure that the event is associated with a generated SoundBank!", m_AkEventPlayable.akEvent.Name), UnityEditor.MessageType.Warning);
			}

			if (!UseWwiseEventDuration.boolValue)
			{
				var StopEventAtClipEndValue = StopEventAtClipEnd.boolValue;
				var retriggerEventValue = retriggerEvent.boolValue;

				UnityEditor.EditorGUILayout.PropertyField(StopEventAtClipEnd, new UnityEngine.GUIContent("Stop Event At End of clip: "));
				UnityEditor.EditorGUILayout.PropertyField(retriggerEvent, new UnityEngine.GUIContent("Loop: ", "When checked, an event will loop until the end of the clip."));

				if (retriggerEvent.boolValue && !StopEventAtClipEnd.boolValue)
				{
					if (!retriggerEventValue)
						StopEventAtClipEnd.boolValue = true;
					else if (StopEventAtClipEndValue)
						retriggerEvent.boolValue = false;
				}
			}
		}

		serializedObject.ApplyModifiedProperties();
	}

	private static void UpdateProgressBar(int index, int count)
	{
		float progress = (float)index / count;
		UnityEditor.EditorUtility.DisplayProgressBar("Wwise Integration", "Fixing clip durations of AkEventPlayables...", progress);
	}

	[UnityEditor.InitializeOnLoadMethod]
	public static void SetupSoundbankSetting()
	{
		if (UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
		{
			return;
		}

		AkUtilities.EnableBoolSoundbankSettingInWproj("SoundBankGenerateEstimatedDuration", AkWwiseEditorSettings.WwiseProjectAbsolutePath);
		
		UnityEditor.EditorApplication.delayCall += UpdateAllClips;
		AkWwiseSoundbanksInfoXMLFileWatcher.Instance.XMLUpdated += UpdateAllClips;
	}

	private static void UpdateAllClips()
	{
		var guids = UnityEditor.AssetDatabase.FindAssets("t:AkEventPlayable", new[] { "Assets" });
		if (guids.Length < 1)
			return;

		var processedGuids = new System.Collections.Generic.HashSet<string>();

		for (var i = 0; i < guids.Length; i++)
		{
			UpdateProgressBar(i, guids.Length);

			var guid = guids[i];
			if (processedGuids.Contains(guid))
				continue;

			processedGuids.Add(guid);

			var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
			var objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
			var instanceIds = new System.Collections.Generic.List<int>();
			foreach (var obj in objects)
			{
				if (obj == null)
					continue;

				var id = obj.GetInstanceID();
				if (!instanceIds.Contains(id))
					instanceIds.Add(id);
			}

			for (; instanceIds.Count > 0; instanceIds.RemoveAt(0))
			{
				var id = instanceIds[0];
				objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
				foreach (var obj in objects)
				{
					if (obj && obj.GetInstanceID() == id)
					{
						var playable = obj as AkEventPlayable;
						if (playable)
						{
							var serializedObject = new UnityEditor.SerializedObject(playable);
							var setClipDuration = serializedObject.FindProperty("UseWwiseEventDuration").boolValue;
							UpdateClipInformation(playable.owningClip, playable.akEvent, serializedObject, setClipDuration);
							serializedObject.ApplyModifiedProperties();
						}

						break;
					}
				}
			}
		}

		UnityEditor.EditorUtility.ClearProgressBar();
	}

	/// <summary>
	/// The minimum clip duration. This value is set to 1/60 of a second which generally represents the time of 1 frame.
	/// </summary>
	private const double MinimumDurationInSeconds = 1.0 / 60;

	/// <summary>
	/// Updates the associated clip information and the event durations.
	/// </summary>
	/// <returns>Returns true if the Wwise event is found in the project data.</returns>
	private static bool UpdateClipInformation(UnityEngine.Timeline.TimelineClip clip, AK.Wwise.Event akEvent, 
		UnityEditor.SerializedObject serializedObject, bool setClipDuration)
	{
		var clipDuration = MinimumDurationInSeconds;
		var eventInfo = AkWwiseProjectInfo.GetData().GetEventInfo(akEvent.Id);
		if (eventInfo != null)
		{
			serializedObject.FindProperty("eventDurationMin").floatValue = eventInfo.minDuration;
			serializedObject.FindProperty("eventDurationMax").floatValue = eventInfo.maxDuration;

			if (eventInfo.maxDuration > clipDuration)
				clipDuration = eventInfo.maxDuration;
		}

		if (clip != null)
		{
			clip.displayName = akEvent.Name;
			if (setClipDuration)
				clip.duration = clipDuration;
		}

		return eventInfo != null && eventInfo.maxDuration > 0;
	}
}

#endif // AK_ENABLE_TIMELINE
#endif //#if UNITY_EDITOR