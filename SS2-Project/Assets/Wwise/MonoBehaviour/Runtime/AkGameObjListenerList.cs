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

[System.Serializable]
public class AkGameObjListenerList : AkAudioListener.BaseListenerList
{
	[System.NonSerialized] private AkGameObj akGameObj;

	[UnityEngine.SerializeField]
	public System.Collections.Generic.List<AkAudioListener> initialListenerList =
		new System.Collections.Generic.List<AkAudioListener>();

	[UnityEngine.SerializeField] public bool useDefaultListeners = true;

	public void SetUseDefaultListeners(bool useDefault)
	{
		if (useDefaultListeners != useDefault)
		{
			useDefaultListeners = useDefault;

			if (useDefault)
			{
				AkSoundEngine.ResetListenersToDefault(akGameObj.gameObject);
				for (var i = 0; i < ListenerList.Count; ++i)
					AkSoundEngine.AddListener(akGameObj.gameObject, ListenerList[i].gameObject);
			}
			else
			{
				var Ids = GetListenerIds();
				AkSoundEngine.SetListeners(akGameObj.gameObject, Ids, Ids == null ? 0 : (uint) Ids.Length);
			}
		}
	}

	public void Init(AkGameObj akGameObj)
	{
		this.akGameObj = akGameObj;

		if (!useDefaultListeners)
			AkSoundEngine.SetListeners(akGameObj.gameObject, null, 0);

		for (var ii = 0; ii < initialListenerList.Count; ++ii)
			initialListenerList[ii].StartListeningToEmitter(akGameObj);
	}

	public override bool Add(AkAudioListener listener)
	{
		var ret = base.Add(listener);
		if (ret && AkSoundEngine.IsInitialized())
			AkSoundEngine.AddListener(akGameObj.gameObject, listener.gameObject);
		return ret;
	}

	public override bool Remove(AkAudioListener listener)
	{
		var ret = base.Remove(listener);
		if (ret && AkSoundEngine.IsInitialized())
			AkSoundEngine.RemoveListener(akGameObj.gameObject, listener.gameObject);
		return ret;
	}

#if UNITY_EDITOR
	public void AddToInitialListenerList(AkAudioListener listener)
	{
		if (!initialListenerList.Contains(listener))
			initialListenerList.Add(listener);
	}

	public void RemoveFromInitialListenerList(AkAudioListener listener)
	{
		initialListenerList.Remove(listener);
	}
#endif
}

#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.