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

public class AkChannelEmitterArray : System.IDisposable
{
	public System.IntPtr m_Buffer;
	private System.IntPtr m_Current;
	private uint m_MaxCount;

	public AkChannelEmitterArray(uint in_Count)
	{
		// Three vectors of 3 floats, plus a mask
		m_Buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal((int) in_Count * (sizeof(float) * 6 + sizeof(double) * 3 + sizeof(uint)));
		m_Current = m_Buffer;
		m_MaxCount = in_Count;
		Count = 0;
	}

	public uint Count { get; private set; }

	public void Dispose()
	{
		if (m_Buffer != System.IntPtr.Zero)
		{
			System.Runtime.InteropServices.Marshal.FreeHGlobal(m_Buffer);
			m_Buffer = System.IntPtr.Zero;
			m_MaxCount = 0;
		}
	}

	~AkChannelEmitterArray()
	{
		Dispose();
	}

	public void Reset()
	{
		m_Current = m_Buffer;
		Count = 0;
	}

	public void Add(AkVector64 in_Pos, UnityEngine.Vector3 in_Forward, UnityEngine.Vector3 in_Top,
		uint in_ChannelMask)
	{
		if (Count >= m_MaxCount)
			throw new System.IndexOutOfRangeException("Out of range access in AkChannelEmitterArray");

		//Marshal doesn't do floats.  So copy the bytes themselves.  Grrr.
		System.Runtime.InteropServices.Marshal.WriteInt32(m_Current,
			System.BitConverter.ToInt32(System.BitConverter.GetBytes(in_Forward.x), 0));
		m_Current = (System.IntPtr) (m_Current.ToInt64() + sizeof(float));
		System.Runtime.InteropServices.Marshal.WriteInt32(m_Current,
			System.BitConverter.ToInt32(System.BitConverter.GetBytes(in_Forward.y), 0));
		m_Current = (System.IntPtr) (m_Current.ToInt64() + sizeof(float));
		System.Runtime.InteropServices.Marshal.WriteInt32(m_Current,
			System.BitConverter.ToInt32(System.BitConverter.GetBytes(in_Forward.z), 0));
		m_Current = (System.IntPtr) (m_Current.ToInt64() + sizeof(float));
		System.Runtime.InteropServices.Marshal.WriteInt32(m_Current,
			System.BitConverter.ToInt32(System.BitConverter.GetBytes(in_Top.x), 0));
		m_Current = (System.IntPtr) (m_Current.ToInt64() + sizeof(float));
		System.Runtime.InteropServices.Marshal.WriteInt32(m_Current,
			System.BitConverter.ToInt32(System.BitConverter.GetBytes(in_Top.y), 0));
		m_Current = (System.IntPtr) (m_Current.ToInt64() + sizeof(float));
		System.Runtime.InteropServices.Marshal.WriteInt32(m_Current,
			System.BitConverter.ToInt32(System.BitConverter.GetBytes(in_Top.z), 0));
		m_Current = (System.IntPtr) (m_Current.ToInt64() + sizeof(float));
		
		System.Runtime.InteropServices.Marshal.WriteInt64(m_Current,
			System.BitConverter.ToInt64(System.BitConverter.GetBytes(in_Pos.X), 0));
		m_Current = (System.IntPtr) (m_Current.ToInt64() + sizeof(double));
		System.Runtime.InteropServices.Marshal.WriteInt64(m_Current,
			System.BitConverter.ToInt64(System.BitConverter.GetBytes(in_Pos.Y), 0));
		m_Current = (System.IntPtr) (m_Current.ToInt64() + sizeof(double));
		System.Runtime.InteropServices.Marshal.WriteInt64(m_Current,
			System.BitConverter.ToInt64(System.BitConverter.GetBytes(in_Pos.Z), 0));
		m_Current = (System.IntPtr) (m_Current.ToInt64() + sizeof(double));

		System.Runtime.InteropServices.Marshal.WriteInt32(m_Current, (int) in_ChannelMask);
		m_Current = (System.IntPtr) (m_Current.ToInt64() + sizeof(uint));

		Count++;
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.