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

public class AkEventCallbackData : UnityEngine.ScriptableObject
{
	////AkSoundEngine.PostEvent callback flags. See the AkCallbackType enumeration for a list of all callbacks
	public System.Collections.Generic.List<int> callbackFlags = new System.Collections.Generic.List<int>();

	////Names of the callback functions.
	public System.Collections.Generic.List<string> callbackFunc = new System.Collections.Generic.List<string>();

	////GameObject that will receive the callback
	public System.Collections.Generic.List<UnityEngine.GameObject> callbackGameObj =
		new System.Collections.Generic.List<UnityEngine.GameObject>();

	////The sum of the flags of all game objects. This is the flag that will be passed to AkSoundEngine.PostEvent
	public int uFlags = 0;
}

#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.