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

[UnityEngine.Timeline.TrackColor(0.855f, 0.8623f, 0.870f)]
[UnityEngine.Timeline.TrackClipType(typeof(AkTimelineEventPlayable))]
[UnityEngine.Timeline.TrackBindingType(typeof(UnityEngine.GameObject))]
/// @brief A track within timeline that holds \ref AkTimelineEventPlayable clips. 
/// @details AkTimelineEventTracks are bound to specific GameObjects, which are the default emitters for all of the associated \ref AkTimelineEventPlayable clips.
/// \sa
/// - \ref AkTimelineEventPlayable
/// - \ref AkTimelineEventPlayableBehavior
public class AkTimelineEventTrack : UnityEngine.Timeline.TrackAsset
{
	public override UnityEngine.Playables.Playable CreateTrackMixer(UnityEngine.Playables.PlayableGraph graph, UnityEngine.GameObject go, int inputCount)
	{
		var playable = UnityEngine.Playables.ScriptPlayable<AkTimelineEventPlayableBehavior>.Create(graph);
		UnityEngine.Playables.PlayableExtensions.SetInputCount(playable, inputCount);

		var clips = GetClips();
		foreach (var clip in clips)
		{
			var eventPlayable = clip.asset as AkTimelineEventPlayable;
			eventPlayable.owningClip = clip;
		}

		return playable;
	}
}
#endif // AK_ENABLE_TIMELINE
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
