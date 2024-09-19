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

[UnityEngine.AddComponentMenu("Wwise/Spatial Audio/AkRoom")]
[UnityEngine.RequireComponent(typeof(UnityEngine.Collider))]
[UnityEngine.DisallowMultipleComponent]
/// @brief An AkRoom is an enclosed environment that can only communicate to the outside/other rooms with AkRoomPortals
/// @details The AkRoom component uses its required Collider component to determine when AkRoomAwareObjects enter and exit the room using the OnTriggerEnter and OnTriggerExit callbacks.
public class AkRoom : AkTriggerHandler
{
	public static ulong INVALID_ROOM_ID = unchecked((ulong)(-1));

	public static ulong GetAkRoomID(AkRoom room)
	{
		return room == null ? INVALID_ROOM_ID : room.GetID();
	}

	public static int RoomCount { get; private set; }

	#region Fields

	[UnityEngine.Tooltip("Higher number has a higher priority")]
	/// In cases where a game object is in an area with two rooms, the higher priority room will be chosen for AK::SpatialAudio::SetGameObjectInRoom()
	/// The higher the priority number, the higher the priority of a room.
	public int priority = 0;

	/// The reverb auxiliary bus.
	public AK.Wwise.AuxBus reverbAuxBus = new AK.Wwise.AuxBus();

	[UnityEngine.Range(0, 1)]
	/// The reverb control value for the send to the reverb aux bus.
	public float reverbLevel = 1;

	[UnityEngine.Range(0, 1)]
	/// Loss value modeling transmission through walls.
	public float transmissionLoss = 1;

	/// Wwise Event to be posted on the room game object.
	public AK.Wwise.Event roomToneEvent = new AK.Wwise.Event();

	[UnityEngine.Range(0, 1)]
	[UnityEngine.Tooltip("Send level for sounds that are posted on the room game object; adds reverb to ambience and room tones. Valid range: (0.f-1.f). A value of 0 disables the aux send.")]
	/// Send level for sounds that are posted on the room game object; adds reverb to ambience and room tones. Valid range: (0.f-1.f). A value of 0 disables the aux send.
	public float roomToneAuxSend = 0;

	/// This is the list of AkRoomAwareObjects that have entered this AkRoom
	private System.Collections.Generic.List<AkRoomAwareObject> roomAwareObjectsEntered = new System.Collections.Generic.List<AkRoomAwareObject>();

	/// This is the list of AkRoomAwareObjects that have entered this AkRoom while it was inactive or disabled.
	private System.Collections.Generic.List<AkRoomAwareObject> roomAwareObjectsDetectedWhileDisabled = new System.Collections.Generic.List<AkRoomAwareObject>();

	private UnityEngine.Collider roomCollider = null;

	private int previousRoomState;
	private int previousTransformState;
	private int previousGeometryState;

	private bool bSentToWwise = false;
	private ulong geometryID = AkSurfaceReflector.INVALID_GEOMETRY_ID;
	private bool bGeometrySetByRoom = false;

	private bool _isAReverbZoneInWwise = false;
	private ulong _parentRoomID = AkRoom.INVALID_ROOM_ID;

	public bool IsAReverbZoneInWwise { get { return _isAReverbZoneInWwise; } }
	public ulong ParentRoomID { get { return _parentRoomID; } }

	#endregion

	private int GetRoomState()
	{
		int[] hashCodes = new[] {
			reverbAuxBus.IsValid() ? reverbAuxBus.GetHashCode() : 0,
			reverbLevel.GetHashCode(),
			transmissionLoss.GetHashCode(),
			roomToneEvent.IsValid() ? roomToneEvent.GetHashCode() : 0,
			roomToneAuxSend.GetHashCode(),
			transform.rotation.GetHashCode()
		};

		return AK.Wwise.BaseType.CombineHashCodes(hashCodes);
	}

	private int GetTransformState()
	{
		var scale = transform.lossyScale;

		if (IsAssociatedGeometryFromCollider())
		{
			if (roomCollider == null)
			{
				roomCollider = GetComponent<UnityEngine.Collider>();
			}

			if (roomCollider.GetType() == typeof(UnityEngine.BoxCollider))
			{
				scale = new UnityEngine.Vector3(
					transform.lossyScale.x * ((UnityEngine.BoxCollider)roomCollider).size.x,
					transform.lossyScale.y * ((UnityEngine.BoxCollider)roomCollider).size.y,
					transform.lossyScale.z * ((UnityEngine.BoxCollider)roomCollider).size.z);
			}
			else if (roomCollider.GetType() == typeof(UnityEngine.CapsuleCollider))
			{
				scale = GetCubeScaleFromCapsule(
					transform.lossyScale,
					((UnityEngine.CapsuleCollider)roomCollider).radius,
					((UnityEngine.CapsuleCollider)roomCollider).height,
					((UnityEngine.CapsuleCollider)roomCollider).direction);
			}
			else if (roomCollider.GetType() == typeof(UnityEngine.SphereCollider))
			{
				scale = roomCollider.bounds.size;
			}
		}

		int[] hashCodes = new[] {
			transform.position.GetHashCode(),
			transform.rotation.GetHashCode(),
			scale.GetHashCode(),
		};

		return AK.Wwise.BaseType.CombineHashCodes(hashCodes);
	}

	private int GetGeometryState()
	{
		if (roomCollider == null)
		{
			roomCollider = GetComponent<UnityEngine.Collider>();
		}
		int colliderHash = roomCollider.GetHashCode();

		int meshHash = 0;
		if (roomCollider.GetType() == typeof(UnityEngine.MeshCollider))
		{
			meshHash = ((UnityEngine.MeshCollider)roomCollider).sharedMesh.GetHashCode();
		}

		int[] hashCodes = new[] {
			colliderHash,
			meshHash
		};
		return AK.Wwise.BaseType.CombineHashCodes(hashCodes);
	}

	public bool TryEnter(AkRoomAwareObject roomAwareObject)
	{
		if (roomAwareObject)
		{
			if (isActiveAndEnabled)
			{
				if(!roomAwareObjectsEntered.Contains(roomAwareObject))
				{
					roomAwareObjectsEntered.Add(roomAwareObject);
				}
				return true;
			}
			else
			{
				if (!roomAwareObjectsDetectedWhileDisabled.Contains(roomAwareObject))
				{
					roomAwareObjectsDetectedWhileDisabled.Add(roomAwareObject);
				}
				return false;
			}
		}
		return false;
	}

	public void Exit(AkRoomAwareObject roomAwareObject)
	{
		if (roomAwareObject)
		{
			roomAwareObjectsEntered.Remove(roomAwareObject);
			roomAwareObjectsDetectedWhileDisabled.Remove(roomAwareObject);
		}
	}

	/// Access the room's ID
	public ulong GetID()
	{
		return AkSoundEngine.GetAkGameObjectID(gameObject);
	}

	public bool IsAssociatedGeometryFromCollider()
	{
		return geometryID == GetID();
	}

	private void SetGeometryFromCollider()
	{
		if (roomCollider == null)
		{
			roomCollider = GetComponent<UnityEngine.Collider>();
		}

		if (roomCollider.GetType() == typeof(UnityEngine.MeshCollider))
		{
			var MeshGeometryData = new AkSurfaceReflector.GeometryData();
			AkSurfaceReflector.GetGeometryDataFromMesh(((UnityEngine.MeshCollider)roomCollider).sharedMesh, ref MeshGeometryData);
			for (int s = 0; s < MeshGeometryData.numSurfaces; s++)
			{
				MeshGeometryData.surfaces[s].transmissionLoss = 0;
			}

			geometryID = GetID();
			AkSoundEngine.SetGeometry(
				geometryID,
				MeshGeometryData.triangles,
				MeshGeometryData.numTriangles,
				MeshGeometryData.vertices,
				MeshGeometryData.numVertices,
				MeshGeometryData.surfaces,
				MeshGeometryData.numSurfaces,
				false,
				false);

			bGeometrySetByRoom = true;
		}
		else if ((roomCollider.GetType() == typeof(UnityEngine.BoxCollider) || (roomCollider.GetType() == typeof(UnityEngine.CapsuleCollider)) && AkInitializer.CubeGeometryData.numTriangles != 0))
		{
			// The capsule collider is approximated with a cube geometry
			geometryID = GetID();

			AkSoundEngine.SetGeometry(
				geometryID,
				AkInitializer.CubeGeometryData.triangles,
				AkInitializer.CubeGeometryData.numTriangles,
				AkInitializer.CubeGeometryData.vertices,
				AkInitializer.CubeGeometryData.numVertices,
				AkInitializer.CubeGeometryData.surfaces,
				AkInitializer.CubeGeometryData.numSurfaces,
				false,
				false);

			bGeometrySetByRoom = true;
		}
		else if (roomCollider.GetType() == typeof(UnityEngine.SphereCollider) && AkInitializer.SphereGeometryData.numTriangles != 0)
		{
			geometryID = GetID();

			AkSoundEngine.SetGeometry(
				geometryID,
				AkInitializer.SphereGeometryData.triangles,
				AkInitializer.SphereGeometryData.numTriangles,
				AkInitializer.SphereGeometryData.vertices,
				AkInitializer.SphereGeometryData.numVertices,
				AkInitializer.SphereGeometryData.surfaces,
				AkInitializer.SphereGeometryData.numSurfaces,
				false,
				false);

			bGeometrySetByRoom = true;
		}
		else
		{
			UnityEngine.Debug.LogWarning(name + " has an invalid collider for wet transmission. Wet Transmission will be disabled.");
			geometryID = AkSurfaceReflector.INVALID_GEOMETRY_ID;
		}
	}

	private void SetGeometryInstanceFromCollider()
	{
		if (!bGeometrySetByRoom)
		{
			SetGeometryFromCollider();
		}

		if (!bGeometrySetByRoom)
		{
			return;
		}

		if (roomCollider == null)
		{
			roomCollider = GetComponent<UnityEngine.Collider>();
		}

		if (roomCollider.GetType() == typeof(UnityEngine.MeshCollider))
		{
			geometryID = GetID();
			AkSurfaceReflector.SetGeometryInstance(geometryID, geometryID, INVALID_ROOM_ID, transform, false);
		}
		else if (roomCollider.GetType() == typeof(UnityEngine.BoxCollider) && AkInitializer.CubeGeometryData.numTriangles != 0)
		{
			geometryID = GetID();

			AkTransform geometryInstanceTransform = new AkTransform();
			geometryInstanceTransform.Set(roomCollider.bounds.center, transform.forward, transform.up);
			UnityEngine.Vector3 geometryInstanceScale = new UnityEngine.Vector3(
				transform.lossyScale.x * ((UnityEngine.BoxCollider)roomCollider).size.x,
				transform.lossyScale.y * ((UnityEngine.BoxCollider)roomCollider).size.y,
				transform.lossyScale.z * ((UnityEngine.BoxCollider)roomCollider).size.z);

			AkSoundEngine.SetGeometryInstance(geometryID, geometryInstanceTransform, geometryInstanceScale, geometryID, INVALID_ROOM_ID, false);
		}
		else if (roomCollider.GetType() == typeof(UnityEngine.CapsuleCollider) && AkInitializer.CubeGeometryData.numTriangles != 0)
		{
			geometryID = GetID();

			AkTransform geometryInstanceTransform = new AkTransform();
			geometryInstanceTransform.Set(roomCollider.bounds.center, transform.forward, transform.up);
			UnityEngine.Vector3 geometryInstanceScale = GetCubeScaleFromCapsule(
				transform.lossyScale,
				((UnityEngine.CapsuleCollider)roomCollider).radius,
				((UnityEngine.CapsuleCollider)roomCollider).height,
				((UnityEngine.CapsuleCollider)roomCollider).direction);

			AkSoundEngine.SetGeometryInstance(geometryID, geometryInstanceTransform, geometryInstanceScale, geometryID, INVALID_ROOM_ID, false);
		}
		else if (roomCollider.GetType() == typeof(UnityEngine.SphereCollider) && AkInitializer.SphereGeometryData.numTriangles != 0)
		{
			geometryID = GetID();

			AkTransform geometryInstanceTransform = new AkTransform();
			geometryInstanceTransform.Set(roomCollider.bounds.center, transform.forward, transform.up);
			UnityEngine.Vector3 geometryInstanceScale = roomCollider.bounds.size;

			AkSoundEngine.SetGeometryInstance(geometryID, geometryInstanceTransform, geometryInstanceScale, geometryID, INVALID_ROOM_ID, false);
		}
		else
		{
			UnityEngine.Debug.LogWarning(name + " has an invalid collider for wet transmission. Wet Transmission will be disabled.");
			geometryID = AkSurfaceReflector.INVALID_GEOMETRY_ID;
		}
	}

	public void SetRoom()
	{
		if (!AkSoundEngine.IsInitialized())
		{
			return;
		}

		var roomParams = new AkRoomParams
		{
			Up = transform.up,
			Front = transform.forward,

			ReverbAuxBus = reverbAuxBus.Id,
			ReverbLevel = reverbLevel,
			TransmissionLoss = transmissionLoss,

			RoomGameObj_AuxSendLevelToSelf = roomToneAuxSend,
			RoomGameObj_KeepRegistered = roomToneEvent.IsValid(),
		};

		if (bSentToWwise == false)
		{
			RoomCount++;
		}

		if (geometryID == AkSurfaceReflector.INVALID_GEOMETRY_ID)
		{
			SetGeometryInstanceFromCollider();
		}

		AkSoundEngine.SetRoom(GetID(), roomParams, geometryID, name);
		bSentToWwise = true;
	}

	public void SetRoom(ulong id)
	{
		if (geometryID != id)
		{
			if (IsAssociatedGeometryFromCollider())
			{
				AkSoundEngine.RemoveGeometryInstance(GetID());
			}
			geometryID = id;
		}
		SetRoom();
	}

	public bool UsesGeometry(ulong id)
	{
		return geometryID == id;
	}

	private void Update()
	{
		int currentTransformState = GetTransformState();
		int currentGeometryState = GetGeometryState();
		int currentRoomState = GetRoomState();

		bool GeometryNeedsUpdate = false;
		bool GeometryInstanceNeedsUpdate = false;
		bool RoomNeedsUpdate = false;
		bool PortalsNeedUpdate = false;

		if (previousTransformState != currentTransformState)
		{
			if (IsAssociatedGeometryFromCollider())
			{
				GeometryInstanceNeedsUpdate = true;
			}
			PortalsNeedUpdate = true;
			previousTransformState = currentTransformState;
		}

		if (previousGeometryState != currentGeometryState)
		{
			if (IsAssociatedGeometryFromCollider())
			{
				GeometryNeedsUpdate = true;
			}
			PortalsNeedUpdate = true;
			previousGeometryState = currentGeometryState;
		}

		if (previousRoomState != currentRoomState)
		{
			RoomNeedsUpdate = true;
			previousRoomState = currentRoomState;
		}

		if (GeometryNeedsUpdate)
		{
			SetGeometryFromCollider();
			SetGeometryInstanceFromCollider();
		}

		if (GeometryInstanceNeedsUpdate)
		{
			SetGeometryInstanceFromCollider();
		}

		if (RoomNeedsUpdate)
		{
			SetRoom();
		}

		if (PortalsNeedUpdate)
		{
			AkRoomManager.RegisterRoomUpdate(this);
		}
	}

	private UnityEngine.Vector3 GetCubeScaleFromCapsule(UnityEngine.Vector3 capsuleScale, float capsuleRadius, float capsuleHeight, int capsuleDirection)
	{
		UnityEngine.Vector3 cubeScale = new UnityEngine.Vector3();

		capsuleScale.x = UnityEngine.Mathf.Abs(capsuleScale.x);
		capsuleScale.y = UnityEngine.Mathf.Abs(capsuleScale.y);
		capsuleScale.z = UnityEngine.Mathf.Abs(capsuleScale.z);

		switch (capsuleDirection)
		{
			case 0:
				cubeScale.y = UnityEngine.Mathf.Max(capsuleScale.y, capsuleScale.z) * (capsuleRadius * 2);
				cubeScale.z = cubeScale.y;
				cubeScale.x = UnityEngine.Mathf.Max(cubeScale.y, capsuleScale.x * capsuleHeight);
				break;
			case 2:
				cubeScale.x = UnityEngine.Mathf.Max(capsuleScale.x, capsuleScale.y) * (capsuleRadius * 2);
				cubeScale.y = cubeScale.x;
				cubeScale.z = UnityEngine.Mathf.Max(cubeScale.x, capsuleScale.z * capsuleHeight);
				break;
			case 1:
			default:
				cubeScale.x = UnityEngine.Mathf.Max(capsuleScale.x, capsuleScale.z) * (capsuleRadius * 2);
				cubeScale.y = UnityEngine.Mathf.Max(cubeScale.x, capsuleScale.y * capsuleHeight);
				cubeScale.z = cubeScale.x;
				break;
		}

		return cubeScale;
	}

	public override void OnEnable()
	{
		roomCollider = GetComponent<UnityEngine.Collider>();

		AkSurfaceReflector surfaceReflectorComponent = gameObject.GetComponent<AkSurfaceReflector>();
		if (surfaceReflectorComponent != null && surfaceReflectorComponent.enabled)
		{
			geometryID = surfaceReflectorComponent.GetID();
		}
		else
		{
			// better call both, even if the geometry might have already been sent to wwise, in case something related to the geometry changed while the room was disabled.
			SetGeometryFromCollider();
			SetGeometryInstanceFromCollider();
		}

		SetRoom();
		AkRoomManager.RegisterRoomUpdate(this);

		// if objects entered the room while disabled, enter them now
		for (var i = 0; i < roomAwareObjectsDetectedWhileDisabled.Count; ++i)
		{
			AkRoomAwareManager.ObjectEnteredRoom(roomAwareObjectsDetectedWhileDisabled[i], this);
		}

		roomAwareObjectsDetectedWhileDisabled.Clear();
		base.OnEnable();

		// init update condition
		previousRoomState = GetRoomState();
		previousTransformState = GetTransformState();
		previousGeometryState = GetGeometryState();
	}

	private void OnDisable()
	{
		for (var i = 0; i < roomAwareObjectsEntered.Count; ++i)
		{
			roomAwareObjectsEntered[i].ExitedRoom(this);
			AkRoomAwareManager.RegisterRoomAwareObjectForUpdate(roomAwareObjectsEntered[i]);
			roomAwareObjectsDetectedWhileDisabled.Add(roomAwareObjectsEntered[i]);
		}
		roomAwareObjectsEntered.Clear();

		AkRoomManager.RegisterRoomUpdate(this);
		if (IsAssociatedGeometryFromCollider())
		{
			AkSoundEngine.RemoveGeometryInstance(GetID());
		}
		geometryID = AkSurfaceReflector.INVALID_GEOMETRY_ID;

		// stop sounds applied to the room game object
		if (roomToneEvent.IsValid())
		{
			AkSoundEngine.StopAll(GetID());
		}

		RoomCount--;
		AkSoundEngine.RemoveRoom(GetID());
		bSentToWwise = false;
	}

	protected override void OnDestroy()
	{
		if (bGeometrySetByRoom)
		{
			AkSoundEngine.RemoveGeometry(GetID());
			bGeometrySetByRoom = false;
		}

		base.OnDestroy();
	}

    private void OnTriggerEnter(UnityEngine.Collider in_other)
	{
		AkRoomAwareManager.ObjectEnteredRoom(in_other, this);
	}

	private void OnTriggerExit(UnityEngine.Collider in_other)
	{
		AkRoomAwareManager.ObjectExitedRoom(in_other, this);
	}

	public void PostRoomTone()
	{
		if (roomToneEvent.IsValid() && isActiveAndEnabled)
		{
			roomToneEvent.Post(GetID());
		}
	}

	public override void HandleEvent(UnityEngine.GameObject in_gameObject)
	{
		PostRoomTone();
	}

	/// <summary>
	/// Establish a parent-child relationship between this Room and a parent Room. Sound propagate between a Reverb Zone and its parent as if they were the same Room, without the need for a connecting Portal.
	/// Examples of Reverb Zones include a covered area with no walls, a forested area within an outdoor space, or any situation where multiple reverb effects are desired within a common space.
	/// Reverb Zones have many advantages compared to standard Game-Defined Auxiliary Sends. They are part of the wet path, and form reverb chains with other Rooms; they are spatialized according to their 3D extent; they are also subject to other acoustic phenomena simulated in Wwise Spatial Audio, such as diffraction and transmission.
	/// If a Room is already assigned to a parent Room, it is first be removed from the original parent (exactly as if RemoveReverbZone were called) before it is assigned to the new parent Room.
	/// The automatically created 'Outdoors' Room is commonly used as a parent Room for Reverb Zones, since they often model open spaces.
	/// Calls AK::SpatialAudio::SetReverbZone() with the Parent Room and transition Region Width parameters.
	/// </summary>
	/// <param name="parentRoom">The AkRoom component to set as the Reverb Zone's parent. A parent Room can have multiple Reverb Zones, but a Reverb Zone can only have a single Parent. A Room cannot be its own parent. Set to null to attach the Reverb Zone to the automatically created 'Outdoors' room.</param>
	/// <param name="transitionRegionWidth">The width of the transition region between the Reverb Zone and its parent. The transition region is centered around the Reverb Zone geometry. It only applies where triangle transmission loss is set to 0.</param>
	public void SetReverbZone(AkRoom parentRoom, float transitionRegionWidth)
	{
		_parentRoomID = AkRoom.INVALID_ROOM_ID;
		if (parentRoom != null)
		{
			_parentRoomID = parentRoom.GetID();
		}

		if (transitionRegionWidth < 0.0f)
		{
			UnityEngine.Debug.LogWarning("SetReverbZone: Transition region width is a negative number. It has been clamped to 0.");
			transitionRegionWidth = 0.0f;
		}

		AkSoundEngine.SetReverbZone(GetID(), ParentRoomID, transitionRegionWidth);
		_isAReverbZoneInWwise = true;
	}

	/// <summary>
	/// Remove this Room, a Reverb Zone, from its parent. Sound can no longer propagate between this Room and its parent, unless they are explicitly connected with a Portal.
	/// Calls AK::SpatialAudio::RemoveReverbZone() with this Room's ID."/>.
	/// </summary>
	public void RemoveReverbZone()
	{
		AkSoundEngine.RemoveReverbZone(GetID());
		_isAReverbZoneInWwise = false;
	}

	public class PriorityList
	{
		private static readonly CompareByPriority s_compareByPriority = new CompareByPriority();

		/// Contains all active rooms sorted by priority.
		private System.Collections.Generic.List<AkRoom> rooms = new System.Collections.Generic.List<AkRoom>();

		public ulong GetHighestPriorityActiveAndEnabledRoomID()
		{
			var room = GetHighestPriorityActiveAndEnabledRoom();
			return GetAkRoomID(room);
		}
		public AkRoom GetHighestPriorityActiveAndEnabledRoom()
		{
			for (int i = 0; i < rooms.Count; i++)
			{
				if (rooms[i].isActiveAndEnabled)
				{
					return rooms[i];
				}
			}

			return null;
		}

		public int Count { get { return rooms.Count; } }

		public void Clear()
		{
			rooms.Clear();
		}

		public void Add(AkRoom room)
		{
			var index = BinarySearch(room);
			if (index < 0)
			{
				rooms.Insert(~index, room);
			}
		}

		public void Remove(AkRoom room)
		{
			rooms.Remove(room);
		}

		public bool Contains(AkRoom room)
		{
			return room && rooms.Contains(room);
		}

		public int BinarySearch(AkRoom room)
		{
			return room ? rooms.BinarySearch(room, s_compareByPriority) : -1;
		}

		public AkRoom this[int index]
		{
			get { return rooms[index]; }
		}

		private class CompareByPriority : System.Collections.Generic.IComparer<AkRoom>
		{
			public virtual int Compare(AkRoom a, AkRoom b)
			{
				var result = a.priority.CompareTo(b.priority);
				if (result == 0 && a != b)
				{
					return 1;
				}

				return -result; // inverted to have highest priority first
			}
		}
	}

#region Obsolete
	[System.Obsolete(AkSoundEngine.Deprecation_2021_1_0)]
	public float wallOcclusion
	{
		get
		{
			return transmissionLoss;
		}
		set
		{
			transmissionLoss = value;
		}
	}

	[System.Obsolete(AkSoundEngine.Deprecation_2023_1_0)]
	public ulong GetGeometryID()
	{
		return geometryID;
	}

	[System.Obsolete(AkSoundEngine.Deprecation_2023_1_0)]
	public void SetGeometryID(ulong id)
	{
		geometryID = id;
	}
#endregion
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.