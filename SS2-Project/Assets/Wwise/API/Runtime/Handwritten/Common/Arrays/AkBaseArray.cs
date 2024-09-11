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

public abstract class AkBaseArray<T> : System.IDisposable
{
	public AkBaseArray(int capacity)
	{
		m_Buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(capacity * StructureSize);
		if (m_Buffer != System.IntPtr.Zero)
		{
			Capacity = capacity;

			for (var index = 0; index < capacity; ++index)
				DefaultConstructAtIntPtr(GetObjectPtr(index));
		}
	}

	public void Dispose()
	{
		if (m_Buffer != System.IntPtr.Zero)
		{
			for (var index = 0; index < Capacity; ++index)
				ReleaseAllocatedMemoryFromReferenceAtIntPtr(GetObjectPtr(index));

			System.Runtime.InteropServices.Marshal.FreeHGlobal(m_Buffer);
			m_Buffer = System.IntPtr.Zero;
			Capacity = 0;
		}
	}

	~AkBaseArray() { Dispose(); }

	public int Capacity { get; private set; }

	public virtual int Count() { return Capacity; }

	protected abstract int StructureSize { get; }

	/// <summary>
	/// This method is called for each element of the array when the array is constructed. It should be used to clear the memory associated with an element so that it will be seen as if it had been default constructed.
	/// </summary>
	/// <param name="address">The address of the element</param>
	protected virtual void DefaultConstructAtIntPtr(System.IntPtr address) { }

	/// <summary>
	/// This method is called for each element of the array when the array is disposed. It should be used to delete memory allocated by elements.
	/// </summary>
	/// <param name="address">The address of the element</param>
	protected virtual void ReleaseAllocatedMemoryFromReferenceAtIntPtr(System.IntPtr address) { }

	protected abstract T CreateNewReferenceFromIntPtr(System.IntPtr address);

	protected abstract void CloneIntoReferenceFromIntPtr(System.IntPtr address, T other);

	public T this[int index]
	{
		get { return CreateNewReferenceFromIntPtr(GetObjectPtr(index)); }
		set { CloneIntoReferenceFromIntPtr(GetObjectPtr(index), value); }
	}

	public System.IntPtr GetBuffer() { return m_Buffer; }

	private System.IntPtr m_Buffer;

	protected System.IntPtr GetObjectPtr(int index)
	{
		if (index >= Capacity)
			throw new System.IndexOutOfRangeException("Out of range access in " + GetType().Name);

		return (System.IntPtr)(m_Buffer.ToInt64() + StructureSize * index);
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.