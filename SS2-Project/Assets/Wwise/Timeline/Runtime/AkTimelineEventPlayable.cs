using System;
using System.Collections.Generic;
#if !(UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
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

/// @brief Defines the behavior of a \ref AkTimelineEventPlayable within a \ref AkTimelineEventTrack.
/// \sa
/// - \ref AkTimelineEventTrack
/// - \ref AkTimelineEventPlayable
public class AkTimelineEventPlayableBehavior : UnityEngine.Playables.PlayableBehaviour
{
	private float currentDuration = -1f;
	private float currentDurationProportion = 1f;

	private bool eventIsPlaying
	{
		get
		{
			return playingId != 0;
		}
	}
	private float previousEventStartTime;
	private uint playingId = 0;

	private const uint CallbackFlags = (uint)(AkCallbackType.AK_EndOfEvent | AkCallbackType.AK_Duration);

	private void CallbackHandler(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
	{
		if (in_type == AkCallbackType.AK_EndOfEvent)
		{
			StopEvent();
		}
		else if (in_type == AkCallbackType.AK_Duration)
		{
			var estimatedDuration = (in_info as AkDurationCallbackInfo).fEstimatedDuration;
			currentDuration = estimatedDuration * currentDurationProportion / 1000f;
		}
	}

#if UNITY_EDITOR
	private static bool CanPostEvents
	{
		get { return UnityEditor.SessionState.GetBool("AkTimelineEventPlayableBehavior.CanPostEvents", true); }
		set { UnityEditor.SessionState.SetBool("AkTimelineEventPlayableBehavior.CanPostEvents", value); }
	}

	[UnityEditor.InitializeOnLoadMethod]
	private static void DetermineCanPostEvents()
	{
		if (UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
		{
			return;
		}

		UnityEditor.Compilation.CompilationPipeline.assemblyCompilationFinished += (string text, UnityEditor.Compilation.CompilerMessage[] messages) =>
		{
			if (!UnityEditor.EditorApplication.isPlaying)
			{
				CanPostEvents = true;
			}
		};

		UnityEditor.EditorApplication.playModeStateChanged += (UnityEditor.PlayModeStateChange playMode) =>
		{
			if (playMode == UnityEditor.PlayModeStateChange.ExitingEditMode)
			{
				CanPostEvents = true;
			}
			
			if (playMode == UnityEditor.PlayModeStateChange.EnteredEditMode)
			{
				CanPostEvents = true;
			}
		};
	}
#endif

	[System.Flags]
	private enum Actions
	{
		None = 0,
		Playback = 1 << 0,
		Retrigger = 1 << 1,
		DelayedStop = 1 << 2,
		Seek = 1 << 3,
		FadeIn = 1 << 4,
		FadeOut = 1 << 5
	}
	private Actions requiredActions;

	private const int scrubPlaybackLengthMs = 100;

	public AK.Wwise.Event akEvent;

	public float eventDurationMax;
	public float eventDurationMin;

	public float blendInDuration;
	public float blendOutDuration;
	public float easeInDuration;
	public float easeOutDuration;

	public AkCurveInterpolation blendInCurve;
	public AkCurveInterpolation blendOutCurve;

	public UnityEngine.GameObject eventObject;

	public bool retriggerEvent;
	private bool wasScrubbingAndRequiresRetrigger;
	public bool StopEventAtClipEnd;

	public bool PrintDebugInformation = false;

	private bool IsScrubbing(UnityEngine.Playables.Playable playable, UnityEngine.Playables.FrameData info)
	{
#if UNITY_EDITOR
		if (!UnityEngine.Application.isPlaying)
		{
			return info.evaluationType == UnityEngine.Playables.FrameData.EvaluationType.Evaluate;
		}
#endif
		var previousTime = UnityEngine.Playables.PlayableExtensions.GetPreviousTime(playable);
		var currentTime = UnityEngine.Playables.PlayableExtensions.GetTime(playable);
		var computedDelta = System.Math.Abs(currentTime - previousTime);

		// Unfortunately, we can't use info.seekOccurred, because it is always true.
		// When time is explicitely set using playable.time, deltaTime is zero, evaluationType is Evaluate, and 
		// either previous time or current time is non-zero
		// However, if time is added to playable.time (for example, playable.time += 1;), evaluationType remains
		// Playing.
		return (info.deltaTime == 0 && (previousTime >= 0 || currentTime >= 0)) || (computedDelta > info.deltaTime);
	}

	void PrintInfo(string FunctionName, UnityEngine.Playables.Playable playable, UnityEngine.Playables.FrameData info)
	{
		if (PrintDebugInformation)
		{
			var previousTime = UnityEngine.Playables.PlayableExtensions.GetPreviousTime(playable);
			var currentTime = UnityEngine.Playables.PlayableExtensions.GetTime(playable);
			var computedDelta = System.Math.Abs(currentTime - previousTime);

			UnityEngine.Debug.Log($"{FunctionName}: prevTime={previousTime}; curTime={currentTime}; computedDelta={computedDelta}; evalType={info.evaluationType}; deltaTime={info.deltaTime}; playState={info.effectivePlayState}; timeHeld={info.timeHeld}; speed={info.effectiveSpeed}; parentSpeed={info.effectiveParentSpeed}");
		}
	}

	public override void PrepareFrame(UnityEngine.Playables.Playable playable, UnityEngine.Playables.FrameData info)
	{
		base.PrepareFrame(playable, info);
		PrintInfo("PrepareFrame", playable, info);

		if (akEvent == null)
		{
			return;
		}

		var shouldPlay = ShouldPlay(playable);
		if (IsScrubbing(playable, info) && shouldPlay)
		{
			requiredActions |= Actions.Seek;

			if (playingId == 0)
			{
				requiredActions |= Actions.Playback;
#if UNITY_EDITOR
				if (!UnityEngine.Application.isPlaying)
				{
					// If we've explicitly set the playhead, only play a small snippet.
					requiredActions |= Actions.DelayedStop;
				}
#endif
				CheckForFadeInFadeOut(playable);
			}
		}
		else if (shouldPlay && playingId == 0 && (requiredActions & Actions.Playback) == 0)
		{
			// The clip is playing but the event hasn't been triggered. We need to start the event and jump to the correct time.
			requiredActions |= Actions.Retrigger;
			CheckForFadeInFadeOut(playable);
		}
		else
		{
			CheckForFadeOut(playable, UnityEngine.Playables.PlayableExtensions.GetTime(playable));
		}
	}

	private const float alph = 0.05f;

	public override void OnBehaviourPlay(UnityEngine.Playables.Playable playable, UnityEngine.Playables.FrameData info)
	{
		PrintInfo("OnBehaviourPlay", playable, info);
		base.OnBehaviourPlay(playable, info);

		if (akEvent == null)
		{
			return;
		}

		if (!ShouldPlay(playable))
		{
			return;
		}

		requiredActions |= Actions.Playback;

		if (IsScrubbing(playable, info))
		{
			wasScrubbingAndRequiresRetrigger = true;

#if UNITY_EDITOR
			if (!UnityEngine.Application.isPlaying)
			{
				// If we've explicitly set the playhead, only play a small snippet.
				requiredActions |= Actions.DelayedStop;
			}
#endif
		}
		else if (GetProportionalTime(playable) > alph)
		{
			// we need to jump to the correct position in the case where the event is played from some non-start position.
			requiredActions |= Actions.Seek;
		}

		CheckForFadeInFadeOut(playable);
	}

	public override void OnBehaviourPause(UnityEngine.Playables.Playable playable, UnityEngine.Playables.FrameData info)
	{
		PrintInfo("OnBehaviourPause", playable, info);
		wasScrubbingAndRequiresRetrigger = false;

		base.OnBehaviourPause(playable, info);
		if (eventObject != null && akEvent != null && StopEventAtClipEnd)
		{
			StopEvent();
		}
	}

	public override void ProcessFrame(UnityEngine.Playables.Playable playable, UnityEngine.Playables.FrameData info, object playerData)
	{
		PrintInfo("ProcessFrame", playable, info);
		base.ProcessFrame(playable, info, playerData);

		if (akEvent == null)
		{
			return;
		}

		var obj = playerData as UnityEngine.GameObject;
		if (obj != null)
		{
			eventObject = obj;
		}

		if (eventObject == null)
		{
			return;
		}

		if ((requiredActions & Actions.Playback) != 0)
		{
			PlayEvent();
		}
		else if ((requiredActions & Actions.Seek) != 0)
		{
			SeekToTime(playable);
		}

		if ((requiredActions & Actions.Retrigger) != 0)
		{
			RetriggerEvent(playable);
		}

		if ((requiredActions & Actions.DelayedStop) != 0)
		{
			StopEvent(scrubPlaybackLengthMs);
		}

		if ((requiredActions & Actions.FadeOut) != 0)
		{
			TriggerFadeOut(playable);
		}
		
		if ((requiredActions & Actions.FadeIn) != 0)
		{
			TriggerFadeIn(playable);
		}

		requiredActions = Actions.None;
	}

	/** Check the playable time against the Wwise event duration to see if playback should occur.
     */
	private bool ShouldPlay(UnityEngine.Playables.Playable playable)
	{
		var previousTime = UnityEngine.Playables.PlayableExtensions.GetPreviousTime(playable);
		var currentTime = UnityEngine.Playables.PlayableExtensions.GetTime(playable);
		
#if UNITY_EDITOR
		// In editor, do not automatically play the event if the cursor is already in the section.
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			if (previousTime == 0.0 && System.Math.Abs(currentTime - previousTime) > 1.0)
			{
				return false;
			}
		}
#endif

		if (previousTime > currentTime)
		{
			return true;
		}

		if (retriggerEvent)
		{
			return true;
		}

		// If max and min duration values from metadata are equal, we can assume a deterministic event.
		if (eventDurationMax.Equals(eventDurationMin) && !eventDurationMin.Equals(-1f))
		{
			return currentTime < eventDurationMax;
		}

		currentTime -= previousEventStartTime;

		var maxDuration = currentDuration.Equals(-1f) ? (float)UnityEngine.Playables.PlayableExtensions.GetDuration(playable) : currentDuration;
		return currentTime < maxDuration;
	}

	private void CheckForFadeInFadeOut(UnityEngine.Playables.Playable playable)
	{
		var currentClipTime = UnityEngine.Playables.PlayableExtensions.GetTime(playable);
		if (blendInDuration > currentClipTime || easeInDuration > currentClipTime)
		{
			requiredActions |= Actions.FadeIn;
		}

		CheckForFadeOut(playable, currentClipTime);
	}

	private void CheckForFadeOut(UnityEngine.Playables.Playable playable, double currentClipTime)
	{
		var timeLeft = UnityEngine.Playables.PlayableExtensions.GetDuration(playable) - currentClipTime;
		if (blendOutDuration >= timeLeft || easeOutDuration >= timeLeft)
		{
			requiredActions |= Actions.FadeOut;
		}
	}

	private void TriggerFadeIn(UnityEngine.Playables.Playable playable)
	{
		var currentClipTime = UnityEngine.Playables.PlayableExtensions.GetTime(playable);
		var fadeDuration = UnityEngine.Mathf.Max(easeInDuration, blendInDuration) - currentClipTime;
		if (fadeDuration > 0)
		{
			AkSoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Pause, playingId, 0, blendInCurve);
			AkSoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Resume, playingId, (int)(fadeDuration * 1000), blendInCurve);
		}
	}

	private void TriggerFadeOut(UnityEngine.Playables.Playable playable)
	{
		var fadeDuration = UnityEngine.Playables.PlayableExtensions.GetDuration(playable) - UnityEngine.Playables.PlayableExtensions.GetTime(playable);
		AkSoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Stop, playingId, (int)(fadeDuration * 1000), blendOutCurve);	
	}

	private void StopEvent(int transition = 0)
	{
		if (playingId == 0)
		{
			return;
		}

		AkSoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Stop, playingId);
		playingId = 0;
	}

	private bool PostEvent()
	{

#if UNITY_EDITOR
		if (!CanPostEvents)
		{
			playingId = AkSoundEngine.AK_INVALID_PLAYING_ID;
		}
#endif
		if(playingId == 0)
		{
			playingId = akEvent.Post(eventObject, CallbackFlags, CallbackHandler, null);
			return playingId != 0;
		}
		return false;
	}

	private void PlayEvent()
	{
		if (!PostEvent())
		{
			return;
		}

		currentDurationProportion = 1f;
		previousEventStartTime = 0f;
	}

	private void RetriggerEvent(UnityEngine.Playables.Playable playable)
	{
		wasScrubbingAndRequiresRetrigger = false;

		if (!PostEvent())
		{
			return;
		}

		currentDurationProportion = 1f - SeekToTime(playable);
		previousEventStartTime = (float)UnityEngine.Playables.PlayableExtensions.GetTime(playable);
	}

	private float GetProportionalTime(UnityEngine.Playables.Playable playable)
	{
		// If max and min duration values from metadata are equal, we can assume a deterministic event.
		if (eventDurationMax == eventDurationMin && eventDurationMin != -1f)
		{
			// If the timeline clip has length greater than the event duration, we want to loop.
			return (float)UnityEngine.Playables.PlayableExtensions.GetTime(playable) % eventDurationMax / eventDurationMax;
		}

		var currentTime = (float)UnityEngine.Playables.PlayableExtensions.GetTime(playable) - previousEventStartTime;
		if (currentTime < 0)
		{
			return -1f;
		}
		var maxDuration = currentDuration == -1f ? (float)UnityEngine.Playables.PlayableExtensions.GetDuration(playable) : currentDuration;
		// If the timeline clip has length greater than the event duration, we want to loop.
		return currentTime % maxDuration / maxDuration;
	}

	// Seek to the current time, taking looping into account.
	private float SeekToTime(UnityEngine.Playables.Playable playable)
	{
		var proportionalTime = GetProportionalTime(playable);
		if (proportionalTime >= 1f) // Avoids Wwise "seeking beyond end of event: audio will stop" error.
		{
			return 1f;
		}

		if (proportionalTime < 0f)
		{
			return 0f;
		}

#if UNITY_EDITOR
		if (!CanPostEvents)
		{
			return proportionalTime;
		}
#endif

		if (playingId != 0)
		{
			AkSoundEngine.SeekOnEvent(akEvent.Id, eventObject, proportionalTime, false, playingId);
		}

		return proportionalTime;
	}
}

/// @brief A playable asset containing a Wwise event that can be placed within a \ref AkTimelineEventTrack in a timeline.
/// @details Use this class to play Wwise events from a timeline and synchronize them to the animation. Events will be emitted from the GameObject that is bound to the AkTimelineEventTrack.
/// \sa
/// - \ref AkTimelineEventTrack
/// - \ref AkTimelineEventPlayableBehavior
public class AkTimelineEventPlayable : UnityEngine.Playables.PlayableAsset, UnityEngine.Timeline.ITimelineClipAsset
{
	public AK.Wwise.Event akEvent = new AK.Wwise.Event();

	[UnityEngine.SerializeField]
	private AkCurveInterpolation blendInCurve = AkCurveInterpolation.AkCurveInterpolation_Linear;
	[UnityEngine.SerializeField]
	private AkCurveInterpolation blendOutCurve = AkCurveInterpolation.AkCurveInterpolation_Linear;

	public float eventDurationMax = -1f;
	public float eventDurationMin = -1f;

	[System.NonSerialized]
	public UnityEngine.Timeline.TimelineClip owningClip;

	[UnityEngine.SerializeField]
	private bool retriggerEvent = false;

	public bool UseWwiseEventDuration = true;
	public bool PrintDebugInformation = false;

	[UnityEngine.SerializeField]
	private bool StopEventAtClipEnd = true;

	UnityEngine.Timeline.ClipCaps UnityEngine.Timeline.ITimelineClipAsset.clipCaps
	{
		get { return UnityEngine.Timeline.ClipCaps.Looping | UnityEngine.Timeline.ClipCaps.Blending; }
	}

	public override UnityEngine.Playables.Playable CreatePlayable(UnityEngine.Playables.PlayableGraph graph, UnityEngine.GameObject owner)
	{
		var playable = UnityEngine.Playables.ScriptPlayable<AkTimelineEventPlayableBehavior>.Create(graph);
		if (akEvent == null)
		{
			return playable;
		}

		var b = playable.GetBehaviour();
		b.akEvent = akEvent;
		b.blendInCurve = blendInCurve;
		b.blendOutCurve = blendOutCurve;
		b.PrintDebugInformation = PrintDebugInformation;

		if (owningClip != null)
		{
			b.easeInDuration = (float)owningClip.easeInDuration;
			b.easeOutDuration = (float)owningClip.easeOutDuration;
			b.blendInDuration = (float)owningClip.blendInDuration;
			b.blendOutDuration = (float)owningClip.blendOutDuration;
		}
		else
		{
			b.easeInDuration = b.easeOutDuration = b.blendInDuration = b.blendOutDuration = 0;
		}

		b.retriggerEvent = retriggerEvent;
		b.StopEventAtClipEnd = StopEventAtClipEnd;
		b.eventObject = owner;
		b.eventDurationMin = eventDurationMin;
		b.eventDurationMax = eventDurationMax;
		if (eventDurationMin.Equals(eventDurationMax) && eventDurationMax.Equals(0f))
		{
			b.eventDurationMin = -1f;
			b.eventDurationMax = -1f;
			eventDurationMin = -1f;
			eventDurationMax = -1f;
		}
		return playable;
	}

#if UNITY_EDITOR
	[UnityEditor.CustomEditor(typeof(AkTimelineEventPlayable))]
	public class Editor : UnityEditor.Editor
	{
		private AkTimelineEventPlayable m_AkTimelineEventPlayable;
		private UnityEditor.SerializedProperty akEvent;
		private UnityEditor.SerializedProperty retriggerEvent;
		private UnityEditor.SerializedProperty UseWwiseEventDuration;
		private UnityEditor.SerializedProperty PrintDebugInformation;
		private UnityEditor.SerializedProperty StopEventAtClipEnd;
		private UnityEditor.SerializedProperty blendInCurve;
		private UnityEditor.SerializedProperty blendOutCurve;

		public void OnEnable()
		{
			m_AkTimelineEventPlayable = target as AkTimelineEventPlayable;
			if (m_AkTimelineEventPlayable == null)
			{
				return;
			}

			akEvent = serializedObject.FindProperty("akEvent");
			retriggerEvent = serializedObject.FindProperty("retriggerEvent");
			UseWwiseEventDuration = serializedObject.FindProperty("UseWwiseEventDuration");
			PrintDebugInformation = serializedObject.FindProperty("PrintDebugInformation");
			StopEventAtClipEnd = serializedObject.FindProperty("StopEventAtClipEnd");
			blendInCurve = serializedObject.FindProperty("blendInCurve");
			blendOutCurve = serializedObject.FindProperty("blendOutCurve");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

			using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
			{
				UnityEditor.EditorGUILayout.PropertyField(akEvent, new UnityEngine.GUIContent("Event: "));
				UnityEditor.EditorGUILayout.PropertyField(blendInCurve);
				UnityEditor.EditorGUILayout.PropertyField(blendOutCurve);
			}

			using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
			{
				UnityEditor.EditorGUILayout.PropertyField(UseWwiseEventDuration, new UnityEngine.GUIContent("Use Wwise Event Duration: ", "The clip duration is set to the duration of the Wwise Event"));

				if (!UpdateClipInformation(m_AkTimelineEventPlayable.owningClip, m_AkTimelineEventPlayable.akEvent, serializedObject, UseWwiseEventDuration.boolValue))
				{
					UnityEditor.EditorGUILayout.HelpBox(string.Format("The duration of the Wwise event \"{0}\" has not been determined. Playback for this event may be inconsistent. " +
						"Ensure that the event is associated with a generated SoundBank!", m_AkTimelineEventPlayable.akEvent.Name), UnityEditor.MessageType.Warning);
				}

				if (!UseWwiseEventDuration.boolValue)
				{
					var StopEventAtClipEndValue = StopEventAtClipEnd.boolValue;
					var retriggerEventValue = retriggerEvent.boolValue;

					UnityEditor.EditorGUILayout.PropertyField(StopEventAtClipEnd, new UnityEngine.GUIContent("Stop Event At End Of Clip: "));
					UnityEditor.EditorGUILayout.PropertyField(retriggerEvent, new UnityEngine.GUIContent("Loop: ", "When checked, an event will loop until the end of the clip."));

					if (retriggerEvent.boolValue && !StopEventAtClipEnd.boolValue)
					{
						if (!retriggerEventValue)
						{
							StopEventAtClipEnd.boolValue = true;
						}
						else if (StopEventAtClipEndValue)
						{
							retriggerEvent.boolValue = false;
						}
					}
				}
			}
			using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
			{
				UnityEditor.EditorGUILayout.PropertyField(PrintDebugInformation);
			}

			serializedObject.ApplyModifiedProperties();
		}

		private static void UpdateProgressBar(int index, int count)
		{
			float progress = (float)index / count;
			UnityEditor.EditorUtility.DisplayProgressBar("Wwise Integration", "Fixing clip durations of AkTimelineEventPlayables...", progress);
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
			var guids = UnityEditor.AssetDatabase.FindAssets("t:AkTimelineEventPlayable", new[] { "Assets" });
			if (guids.Length < 1)
			{
				return;
			}

			var processedGuids = new System.Collections.Generic.HashSet<string>();

			for (var i = 0; i < guids.Length; i++)
			{
				UpdateProgressBar(i, guids.Length);

				var guid = guids[i];
				if (processedGuids.Contains(guid))
				{
					continue;
				}

				processedGuids.Add(guid);

				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				var objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
				var instanceIds = new System.Collections.Generic.List<int>();
				foreach (var obj in objects)
				{
					if (obj == null)
					{
						continue;
					}

					var id = obj.GetInstanceID();
					if (!instanceIds.Contains(id))
					{
						instanceIds.Add(id);
					}
				}

				for (; instanceIds.Count > 0; instanceIds.RemoveAt(0))
				{
					var id = instanceIds[0];
					objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
					foreach (var obj in objects)
					{
						if (obj && obj.GetInstanceID() == id)
						{
							var playable = obj as AkTimelineEventPlayable;
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
			var maxDuration = -1.0f;
			var minDuration = -1.0f;

			AkUtilities.GetEventDurations(akEvent.Id, ref maxDuration, ref minDuration);
			if (maxDuration != -1.0f)
			{
				serializedObject.FindProperty("eventDurationMin").floatValue = minDuration;
				serializedObject.FindProperty("eventDurationMax").floatValue = maxDuration;

				if (maxDuration > clipDuration)
				{
					clipDuration = maxDuration;
				}
			}

			if (clip != null)
			{
				clip.displayName = akEvent.Name;
				if (setClipDuration)
				{
					clip.duration = clipDuration;
				}
			}

			return maxDuration != -1.0f;
		}
	}

#endif //#if UNITY_EDITOR
}

#endif // AK_ENABLE_TIMELINE
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
