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

public class AkMIDIPostArray
{
	private readonly int m_Count;
	private readonly int SIZE_OF = AkSoundEnginePINVOKE.CSharp_AkMIDIPost_GetSizeOf();
	private System.IntPtr m_Buffer = System.IntPtr.Zero;

	public AkMIDIPostArray(int size)
	{
		m_Count = size;
		m_Buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(m_Count * SIZE_OF);
	}

	public AkMIDIPost this[int index]
	{
		get
		{
			if (index >= m_Count)
				throw new System.IndexOutOfRangeException("Out of range access in AkMIDIPostArray");

			return new AkMIDIPost(GetObjectPtr(index), false);
		}

		set
		{
			if (index >= m_Count)
				throw new System.IndexOutOfRangeException("Out of range access in AkMIDIPostArray");

			AkSoundEnginePINVOKE.CSharp_AkMIDIPost_Clone(GetObjectPtr(index), AkMIDIPost.getCPtr(value));
		}
	}

	~AkMIDIPostArray()
	{
		System.Runtime.InteropServices.Marshal.FreeHGlobal(m_Buffer);
		m_Buffer = System.IntPtr.Zero;
	}

	public void PostOnEvent(uint in_eventID, UnityEngine.GameObject gameObject)
	{
		var gameObjectID = AkSoundEngine.GetAkGameObjectID(gameObject);
		AkSoundEngine.PreGameObjectAPICall(gameObject, gameObjectID);
		AkSoundEnginePINVOKE.CSharp_PostMIDIOnEvent__SWIG_3(in_eventID, gameObjectID, m_Buffer, (ushort) m_Count);
	}

	public void PostOnEvent(uint in_eventID, UnityEngine.GameObject gameObject, int count)
	{
		if (count >= m_Count)
			throw new System.IndexOutOfRangeException("Out of range access in AkMIDIPostArray");

		var gameObjectID = AkSoundEngine.GetAkGameObjectID(gameObject);
		AkSoundEngine.PreGameObjectAPICall(gameObject, gameObjectID);
		AkSoundEnginePINVOKE.CSharp_PostMIDIOnEvent__SWIG_3(in_eventID, gameObjectID, m_Buffer, (ushort) count);
	}

	public System.IntPtr GetBuffer()
	{
		return m_Buffer;
	}

	public int Count()
	{
		return m_Count;
	}

	private System.IntPtr GetObjectPtr(int index)
	{
		return (System.IntPtr) (m_Buffer.ToInt64() + SIZE_OF * index);
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.