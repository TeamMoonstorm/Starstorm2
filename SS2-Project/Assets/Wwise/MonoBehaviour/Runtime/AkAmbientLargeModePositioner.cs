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

/// @brief Use this component to add a Large Mode position to any AkAmbient in a Scene.
/// \sa
/// - \ref unity_use_AkEvent_AkAmbient
/// - \ref AkAmbient
public class AkAmbientLargeModePositioner : UnityEngine.MonoBehaviour
{
	public UnityEngine.Vector3 Position
	{
		get
		{
			return transform.position;
		}
	}

	public UnityEngine.Vector3 Forward
	{
		get
		{
			return transform.forward;
		}
	}

	public UnityEngine.Vector3 Up
	{
		get
		{
			return transform.up;
		}
	}

#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		UnityEngine.Gizmos.color = UnityEngine.Color.green;
		UnityEngine.Gizmos.DrawSphere(transform.position, 0.1f);

		UnityEditor.Handles.Label(transform.position, name);
	}
#endif
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.