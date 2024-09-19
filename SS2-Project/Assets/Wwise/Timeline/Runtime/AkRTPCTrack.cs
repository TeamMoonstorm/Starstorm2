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

#if !(UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
#if !UNITY_2019_1_OR_NEWER
#define AK_ENABLE_TIMELINE
#endif
#if AK_ENABLE_TIMELINE

[UnityEngine.Timeline.TrackColor(0.32f, 0.13f, 0.13f)]
// Specifies the type of Playable Asset this track manages
[UnityEngine.Timeline.TrackClipType(typeof(AkRTPCPlayable))]
// Use if the track requires a binding to a scene object or asset
[UnityEngine.Timeline.TrackBindingType(typeof(UnityEngine.GameObject))]
[System.Obsolete(AkSoundEngine.Deprecation_2019_2_0)]
#if UNITY_2019_1_OR_NEWER
[UnityEngine.Timeline.HideInMenu]
#endif
public class AkRTPCTrack : UnityEngine.Timeline.TrackAsset
{
	public AK.Wwise.RTPC Parameter;

	// override the type of mixer playable used by this track
	public override UnityEngine.Playables.Playable CreateTrackMixer(UnityEngine.Playables.PlayableGraph graph,
		UnityEngine.GameObject go, int inputCount)
	{
		var playable = UnityEngine.Playables.ScriptPlayable<AkRTPCPlayableBehaviour>.Create(graph, inputCount);
		setPlayableProperties();
		return playable;
	}

	public void setPlayableProperties()
	{
		var clips = GetClips();
		foreach (var clip in clips)
		{
			var clipPlayable = (AkRTPCPlayable) clip.asset;
			clipPlayable.Parameter = Parameter;
			clipPlayable.OwningClip = clip;
		}
	}

	public void OnValidate()
	{
		var clips = GetClips();
		foreach (var clip in clips)
		{
			var clipPlayable = (AkRTPCPlayable) clip.asset;
			clipPlayable.Parameter = Parameter;
		}
	}
}

#endif // AK_ENABLE_TIMELINE
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.