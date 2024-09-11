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

[UnityEngine.AddComponentMenu("Wwise/Spatial Audio/AkRoomPortalObstruction")]
[UnityEngine.RequireComponent(typeof(AkRoomPortal))]
/// @brief Completely obstructs the spatial audio portal of the current game object from the spatial audio listener if at least one object is between them.
/// @details If no spatial audio listener has been registered, there will be no obstruction.
public class AkRoomPortalObstruction : AkObstructionOcclusion
{
	private AkRoomPortal m_portal;

	private void Awake()
	{
		InitIntervalsAndFadeRates();
		m_portal = GetComponent<AkRoomPortal>();
	}

	protected override void UpdateCurrentListenerList()
	{
		currentListenerList.Add(AkSpatialAudioListener.TheSpatialAudioListener);
	}

	protected override void SetObstructionOcclusion(
		System.Collections.Generic.KeyValuePair<AkAudioListener, ObstructionOcclusionValue> ObsOccPair)
	{
		if (m_portal.IsValid && m_portal.isSetInWwise())
		{
			AkSoundEngine.SetPortalObstructionAndOcclusion(m_portal.GetID(), ObsOccPair.Value.currentValue, 0.0f);
		}
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.