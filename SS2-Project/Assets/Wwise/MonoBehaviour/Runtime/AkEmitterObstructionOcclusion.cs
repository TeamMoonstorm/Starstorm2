#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
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

[UnityEngine.AddComponentMenu("Wwise/AkEmitterObstructionOcclusion")]
[UnityEngine.RequireComponent(typeof(AkGameObj))]
/// @brief Completely obstructs/occludes the emitter of the current game object from its listeners if at least one object is between them.
public class AkEmitterObstructionOcclusion : AkObstructionOcclusion
{
	private AkGameObj m_gameObj;

	private void Awake()
	{
		InitIntervalsAndFadeRates();
		m_gameObj = GetComponent<AkGameObj>();
	}

	protected override void UpdateCurrentListenerList()
	{
		if (AkSpatialAudioListener.TheSpatialAudioListener != null && AkRoom.RoomCount > 0)
		{
			currentListenerList.Add(AkSpatialAudioListener.TheSpatialAudioListener);
			return;
		}

		if (m_gameObj.IsUsingDefaultListeners)
		{
			currentListenerList.AddRange(AkAudioListener.DefaultListeners.ListenerList);
		}
		currentListenerList.AddRange(m_gameObj.ListenerList);
	}

	protected override void SetObstructionOcclusion(
		System.Collections.Generic.KeyValuePair<AkAudioListener, ObstructionOcclusionValue> ObsOccPair)
	{
		if (AkSpatialAudioListener.TheSpatialAudioListener != null && AkRoom.RoomCount > 0)
		{
			AkSoundEngine.SetObjectObstructionAndOcclusion(gameObject, ObsOccPair.Key.gameObject, ObsOccPair.Value.currentValue, 0.0f);
		}
		else
		{
			AkSoundEngine.SetObjectObstructionAndOcclusion(gameObject, ObsOccPair.Key.gameObject, 0.0f, ObsOccPair.Value.currentValue);
		}
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.