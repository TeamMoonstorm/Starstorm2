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

/// @brief This manager tracks AkRoomPortals and the rooms that they connect (front and back room).
/// @details At the end of the frame, the AkRoomPortals which rooms might have changed are updated and sent to Spatial Audio.
public class AkRoomManager
{
	private readonly System.Collections.Generic.List<AkRoomPortal> m_Portals =
		new System.Collections.Generic.List<AkRoomPortal>();

	private readonly System.Collections.Generic.List<AkRoomPortal> m_PortalsToUpdate =
		new System.Collections.Generic.List<AkRoomPortal>();

	private readonly System.Collections.Generic.List<AkSurfaceReflector> m_Reflectors =
		new System.Collections.Generic.List<AkSurfaceReflector>();

	private readonly System.Collections.Generic.List<AkSurfaceReflector> m_ReflectorsToUpdate =
		new System.Collections.Generic.List<AkSurfaceReflector>();

	private static AkRoomManager m_Instance;

	public static void Init()
	{
		if (m_Instance == null)
		{
			m_Instance = new AkRoomManager();
		}
	}

	public static void Terminate()
	{
		if (m_Instance != null)
		{
			m_Instance = null;
		}
	}

	public static void RegisterPortal(AkRoomPortal portal)
	{
		if (m_Instance != null)
		{
			if (!m_Instance.m_Portals.Contains(portal))
			{
				m_Instance.m_Portals.Add(portal);
			}
			if (!m_Instance.m_PortalsToUpdate.Contains(portal))
			{
				m_Instance.m_PortalsToUpdate.Add(portal);
			}
		}
	}

	public static void UnregisterPortal(AkRoomPortal portal)
	{
		if (m_Instance != null)
		{
			m_Instance.m_Portals.Remove(portal);
			m_Instance.m_PortalsToUpdate.Remove(portal);
		}
	}

	public static void RegisterReflector(AkSurfaceReflector reflector)
	{
		if (m_Instance != null)
		{
			if (!m_Instance.m_Reflectors.Contains(reflector))
			{
				m_Instance.m_Reflectors.Add(reflector);
			}
			if (!m_Instance.m_ReflectorsToUpdate.Contains(reflector))
			{
				m_Instance.m_ReflectorsToUpdate.Add(reflector);
			}
		}
	}

	public static void UnregisterReflector(AkSurfaceReflector reflector)
	{
		if (m_Instance != null)
		{
			m_Instance.m_Reflectors.Remove(reflector);
			m_Instance.m_ReflectorsToUpdate.Remove(reflector);
		}
	}

	public static void RegisterPortalUpdate(AkRoomPortal portal)
	{
		if (m_Instance != null)
		{
			if (m_Instance.m_Portals.Contains(portal) && !m_Instance.m_PortalsToUpdate.Contains(portal))
			{
				m_Instance.m_PortalsToUpdate.Add(portal);
			}
		}
	}

	public static void RegisterRoomUpdate(AkRoom room)
	{
		if (m_Instance != null)
		{
			for (var i = 0; i < m_Instance.m_Portals.Count; ++i)
			{
				var portal = m_Instance.m_Portals[i];
				if (!m_Instance.m_PortalsToUpdate.Contains(portal) &&
					(room == portal.frontRoom || room == portal.backRoom || portal.Overlaps(room)))
				{
					m_Instance.m_PortalsToUpdate.Add(portal);
				}
			}
		}
	}

	public static void Update()
	{
		if (m_Instance != null)
		{
			for (var i = 0; i < m_Instance.m_PortalsToUpdate.Count; ++i)
			{
				m_Instance.m_PortalsToUpdate[i].UpdateRoomPortal();
			}
			m_Instance.m_PortalsToUpdate.Clear();
			for (var i = 0; i < m_Instance.m_ReflectorsToUpdate.Count; ++i)
			{
				m_Instance.m_ReflectorsToUpdate[i].UpdateGeometry();
			}
			m_Instance.m_ReflectorsToUpdate.Clear();
		}
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.