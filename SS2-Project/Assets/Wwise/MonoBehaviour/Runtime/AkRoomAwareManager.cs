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

/// @brief This manager tracks the AkRoomAwareObjects and the AkRooms in which they enter and exit.
/// @details At the end of the frame, the AkRoomAwareObject is set in the highest priority AkRoom in Spatial Audio.
public static class AkRoomAwareManager
{
	private static readonly System.Collections.Generic.HashSet<AkRoomAwareObject> m_RoomAwareObjects =
		new System.Collections.Generic.HashSet<AkRoomAwareObject>();

	private static readonly System.Collections.Generic.HashSet<AkRoomAwareObject> m_RoomAwareObjectToUpdate =
		new System.Collections.Generic.HashSet<AkRoomAwareObject>();

	public static void RegisterRoomAwareObject(AkRoomAwareObject roomAwareObject)
	{
		m_RoomAwareObjects.Add(roomAwareObject);
		RegisterRoomAwareObjectForUpdate(roomAwareObject);
	}

	public static void UnregisterRoomAwareObject(AkRoomAwareObject roomAwareObject)
	{
		m_RoomAwareObjects.Remove(roomAwareObject);
		m_RoomAwareObjectToUpdate.Remove(roomAwareObject);
	}

	public static void RegisterRoomAwareObjectForUpdate(AkRoomAwareObject roomAwareObject)
	{
		m_RoomAwareObjectToUpdate.Add(roomAwareObject);
	}

	public static void ObjectEnteredRoom(UnityEngine.Collider collider, AkRoom room)
	{
		if (!collider)
		{
			return;
		}

		ObjectEnteredRoom(AkRoomAwareObject.GetAkRoomAwareObjectFromCollider(collider), room);
	}

	public static void ObjectEnteredRoom(AkRoomAwareObject roomAwareObject, AkRoom room)
	{
		if (!roomAwareObject || !room)
		{
			return;
		}

		var enteredRoom = room.TryEnter(roomAwareObject);
		if (enteredRoom)
		{
			roomAwareObject.EnteredRoom(room);
			RegisterRoomAwareObjectForUpdate(roomAwareObject);
		}
	}

	public static void ObjectExitedRoom(UnityEngine.Collider collider, AkRoom room)
	{
		if (!collider)
		{
			return;
		}

		ObjectExitedRoom(AkRoomAwareObject.GetAkRoomAwareObjectFromCollider(collider), room);
	}

	public static void ObjectExitedRoom(AkRoomAwareObject roomAwareObject, AkRoom room)
	{
		if (!roomAwareObject || !room)
		{
			return;
		}

		room.Exit(roomAwareObject);
		roomAwareObject.ExitedRoom(room);
		RegisterRoomAwareObjectForUpdate(roomAwareObject);
	}

	public static void UpdateRoomAwareObjects()
	{
		foreach (var roomAwareObject in m_RoomAwareObjectToUpdate)
		{
			if (m_RoomAwareObjects.Contains(roomAwareObject))
			{
				roomAwareObject.SetGameObjectInHighestPriorityActiveAndEnabledRoom();
			}
		}
		m_RoomAwareObjectToUpdate.Clear();
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.