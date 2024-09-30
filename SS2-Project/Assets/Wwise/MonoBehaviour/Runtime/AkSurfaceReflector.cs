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

[UnityEngine.AddComponentMenu("Wwise/Spatial Audio/AkSurfaceReflector")]
[UnityEngine.ExecuteInEditMode]
///@brief This component converts the provided mesh into Spatial Audio Geometry.
///@details This component takes a mesh as a parameter. The triangles of the mesh are sent to Spatial Audio by calling SpatialAudio::AddGeometrySet(). The triangles reflect sounds that have an associated early reflections bus. If diffraction is enabled on this component, spatial audio also finds edges on the provided mesh, which diffract sounds that are diffraction enabled.
public class AkSurfaceReflector : UnityEngine.MonoBehaviour
#if UNITY_EDITOR
	, AK.Wwise.IMigratable
#endif
{
	public static ulong INVALID_GEOMETRY_ID = unchecked((ulong)-1.0f);

	[UnityEngine.Tooltip("The mesh to send to Spatial Audio as a Geometry Set. If this GameObject has a MeshFilter component, you can leave this parameter to None to use the same mesh for Spatial Audio. Otherwise, this parameter lets you import a different mesh for Spatial Audio purposes. We recommend using a simplified mesh.")]
	/// The mesh to send to Spatial Audio as a Geometry Set. We recommend using a simplified mesh.
	public UnityEngine.Mesh Mesh;

	[UnityEngine.Tooltip("The acoustic texture per submesh. The acoustic texture represents the surface of the geometry. An acoustic texture is a set of absorption levels that will filter the sound reflected from the geometry.")]
	/// The acoustic texture per submesh. The acoustic texture represents the surface of the geometry. An acoustic texture is a set of absorption levels that will filter the sound reflected from the geometry.
	public AK.Wwise.AcousticTexture[] AcousticTextures = new AK.Wwise.AcousticTexture[1];

	[UnityEngine.Tooltip("The transmission loss value per submesh. The transmission loss value is a control value used to adjust sound parameters. Typically, a value of 1.0 represents total sound loss, and a value of 0.0 indicates that sound can be transmitted through the geometry without any loss. Default value : 1.0.")]
	[UnityEngine.Range(0, 1)]
	/// The transmission loss value per submesh. The transmission loss value is a control value used to adjust sound parameters. Typically, a value of 1.0 represents total sound loss, and a value of 0.0 indicates that sound can be transmitted through the geometry without any loss. Default value : 1.0.
	public float[] TransmissionLossValues = new[] { 1.0f };

	[UnityEngine.Tooltip("Enable or disable geometric diffraction for this mesh.")]
	/// Switch to enable or disable geometric diffraction for this mesh.
	public bool EnableDiffraction = true;

	[UnityEngine.Tooltip("Enable or disable geometric diffraction on boundary edges for this mesh. Boundary edges are edges that are connected to only one triangle.")]
	/// Switch to enable or disable geometric diffraction on boundary edges for this mesh.  Boundary edges are edges that are connected to only one triangle.
	public bool EnableDiffractionOnBoundaryEdges = false;

	[UnityEngine.Tooltip("(Deprecated) Associate this AkSurfaceReflector component with a Room. This property is deprecated and will be removed in a future version. We recommend not using it by leaving it set to None. Associating an AkSurfaceReflector with a particular Room limits the scope in which the geometry is accessible. Doing so reduces the search space for ray casting performed by reflection and diffraction calculations. When set to None, this geometry has a global scope. Note if one or more geometry sets are associated with a room, that room can no longer access geometry that is in the global scope.")]
	/// (Deprecated) Associate this AkSurfaceReflector component with a Room.
	/// This property is deprecated and will be removed in a future version. We recommend not using it by leaving it set to None.
	/// Associating an AkSurfaceReflector with a particular Room limits the scope in which the geometry is accessible. Doing so reduces the search space for ray casting performed by reflection and diffraction calculations.
	/// When set to None, this geometry has a global scope.
	/// Note if one or more geometry sets are associated with a room, that room can no longer access geometry that is in the global scope.
	public AkRoom AssociatedRoom = null;

	private int PreviousTransformState;
	private int PreviousGeometryState;
	private int PreviousAssociatedRoomState;

	private bool isGeometrySetInWwise = false;

	private int GetTransformState()
	{
		int[] hashCodes = new[] {
			transform.position.GetHashCode(),
			transform.lossyScale.GetHashCode(),
			transform.rotation.GetHashCode()
		};

		return AK.Wwise.BaseType.CombineHashCodes(hashCodes);
	}

	private int GetGeometryState()
	{
		int[] hashCodes = new int[3 + AcousticTextures.Length + TransmissionLossValues.Length];

		hashCodes[0] = Mesh.GetHashCode();
		hashCodes[1] = EnableDiffraction.GetHashCode();
		hashCodes[2] = EnableDiffractionOnBoundaryEdges.GetHashCode();

		int idx = 3;

		foreach (var AcousticTexture in AcousticTextures)
		{
			hashCodes[idx] = AcousticTexture.ObjectReference != null? AcousticTexture.GetHashCode() : 0;
			idx++;
		}

		foreach (var TransmissionLossValue in TransmissionLossValues)
		{
			hashCodes[idx] = TransmissionLossValue.GetHashCode();
			idx++;
		}

		return AK.Wwise.BaseType.CombineHashCodes(hashCodes);
	}

	private int GetAssociatedRoomState()
	{
		int[] hashCodes = new[] {
			AssociatedRoom != null ? AssociatedRoom.GetHashCode() : 0
		};

		return AK.Wwise.BaseType.CombineHashCodes(hashCodes);
	}

	/// <summary>
	/// The Spatial Audio Geometry Data. Can be used when calling AkSoundEngine.SetGeometry()
	/// </summary>
	public struct GeometryData
	{
		public UnityEngine.Vector3[] vertices;
		public AkTriangleArray triangles;
		public AkAcousticSurfaceArray surfaces;
		public uint numVertices;
		public uint numTriangles;
		public uint numSurfaces;
	}

	public ulong GetID()
	{
		return (ulong)GetInstanceID();
	}

	/// <summary>
	/// Convert the mesh into a geometry consisting of vertices, triangles, surfaces, acoustic textures and transmission loss values.
	/// Send it to Wwise with the rest of the AkGeometryParams to add or update a geometry in Spatial Audio.
	/// It is necessary to create at least one geometry instance for each geometry set that is to be used for diffraction and reflection simulation. See SetGeometryInstance().
	/// </summary>
	/// <param name="mesh">The mesh representing the geometry to be sent to Spatial Audio.</param>
	/// <param name="geometryID">A unique ID representing the geometry.</param>
	/// <param name="enableDiffraction">Enable the edges of this geometry to become diffraction edges.</param>
	/// <param name="enableDiffractionOnBoundaryEdges">Enable the boundary edges of this geometry to become diffraction edges. Boundary edges are edges that are connected to only one triangle.</param>
	/// <param name="acousticTextures">The acoustic texture of each surface of the geometry. Acoustic textures describe the filtering when sound reflects on the surface.</param>
	/// <param name="transmissionLossValues">The transmission loss value of each surface of the geometry. Transmission loss is the filtering when the sound goes through the surface.</param>
	/// <param name="name">A name for the geometry.</param>
	/// <returns>Returns true if the Geometry was sent to Wwise.</returns>
	public static bool SetGeometryFromMesh(
		UnityEngine.Mesh mesh,
		ulong geometryID,
		bool enableDiffraction,
		bool enableDiffractionOnBoundaryEdges,
		AK.Wwise.AcousticTexture[] acousticTextures = null,
		float[] transmissionLossValues = null,
		string name = "")
	{
		if (!AkSoundEngine.IsInitialized())
		{
			return false;
		}

#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying) return false;
#endif

		var geometryData = new GeometryData();
		GetGeometryDataFromMesh(mesh, ref geometryData, acousticTextures, transmissionLossValues, name);

		if (geometryData.numTriangles > 0)
		{
			var result = AkSoundEngine.SetGeometry(
				geometryID,
				geometryData.triangles,
				geometryData.numTriangles,
				geometryData.vertices,
				geometryData.numVertices,
				geometryData.surfaces,
				geometryData.numSurfaces,
				enableDiffraction,
				enableDiffractionOnBoundaryEdges);

			return result == AKRESULT.AK_Success;
		}
		else
		{
			UnityEngine.Debug.LogFormat("SetGeometry({0}): No valid triangle was found. Geometry was not set", mesh.name);
			return false;
		}
	}

	/// <summary>
	/// Create Spatial Audio Geometry Data from a Unity mesh. The Geometry Data can be used later to call AkSoundEngine.SetGeometry()
	/// </summary>
	/// <param name="mesh">The mesh representing the geometry to be sent to Spatial Audio.</param>
	/// <param name="geometryData">(Output)The Spatial Audio Geometry Data created from the mesh.</param>
	/// <param name="acousticTextures">The acoustic texture of each surface of the geometry. Acoustic textures describe the filtering when sound reflects on the surface.</param>
	/// <param name="transmissionLossValues">The transmission loss value of each surface of the geometry. Transmission loss is the filtering when the sound goes through the surface.</param>
	/// <param name="name">A name for the geometry.</param>
	public static void GetGeometryDataFromMesh(
		UnityEngine.Mesh mesh,
		ref GeometryData geometryData,
		AK.Wwise.AcousticTexture[] acousticTextures = null,
		float[] transmissionLossValues = null,
		string name = "")
	{
		var vertices = mesh.vertices;

		// Remove duplicate vertices
		var vertRemap = new int[vertices.Length];
		var uniqueVerts = new System.Collections.Generic.List<UnityEngine.Vector3>();
		var vertDict = new System.Collections.Generic.Dictionary<UnityEngine.Vector3, int>();

		for (var v = 0; v < vertices.Length; ++v)
		{
			int vertIdx = 0;
			if (!vertDict.TryGetValue(vertices[v], out vertIdx))
			{
				vertIdx = uniqueVerts.Count;
				uniqueVerts.Add(vertices[v]);
				vertDict.Add(vertices[v], vertIdx);
			}
			vertRemap[v] = vertIdx;
		}

		int vertexCount = uniqueVerts.Count;
		geometryData.vertices = new UnityEngine.Vector3[vertexCount];

		for (var v = 0; v < vertexCount; ++v)
		{
			geometryData.vertices[v].x = uniqueVerts[v].x;
			geometryData.vertices[v].y = uniqueVerts[v].y;
			geometryData.vertices[v].z = uniqueVerts[v].z;
		}

		int surfaceCount = mesh.subMeshCount;

		var numTriangles = mesh.triangles.Length / 3;
		if ((mesh.triangles.Length % 3) != 0)
		{
			UnityEngine.Debug.LogFormat("SetGeometryFromMesh({0}): Wrong number of triangles", mesh.name);
		}

		geometryData.surfaces = new AkAcousticSurfaceArray(surfaceCount);
		geometryData.triangles = new AkTriangleArray(numTriangles);

		int triangleArrayIdx = 0;

		for (var s = 0; s < surfaceCount; ++s)
		{
			var surface = geometryData.surfaces[s];
			var triangles = mesh.GetTriangles(s);
			var triangleCount = triangles.Length / 3;
			if ((triangles.Length % 3) != 0)
			{
				UnityEngine.Debug.LogFormat("SetGeometryFromMesh({0}): Wrong number of triangles in submesh {1}", mesh.name, s);
			}

			AK.Wwise.AcousticTexture acousticTexture = null;
			float occlusionValue = 1.0f;

			if (acousticTextures != null && s < acousticTextures.Length)
			{
				acousticTexture = acousticTextures[s];
			}

			if (transmissionLossValues != null && s < transmissionLossValues.Length)
			{
				occlusionValue = transmissionLossValues[s];
			}

			surface.textureID = acousticTexture == null ? AK.Wwise.AcousticTexture.InvalidId : acousticTexture.Id;
			surface.transmissionLoss = occlusionValue;
			surface.strName = name + "_" + mesh.name + "_" + s;

			for (var i = 0; i < triangleCount; ++i)
			{
				var triangle = geometryData.triangles[triangleArrayIdx];

				triangle.point0 = (ushort)vertRemap[triangles[3 * i + 0]];
				triangle.point1 = (ushort)vertRemap[triangles[3 * i + 1]];
				triangle.point2 = (ushort)vertRemap[triangles[3 * i + 2]];
				triangle.surface = (ushort)s;

				if (triangle.point0 != triangle.point1 && triangle.point0 != triangle.point2 && triangle.point1 != triangle.point2)
				{
					++triangleArrayIdx;
				}
				else
				{
					UnityEngine.Debug.LogFormat("SetGeometryFromMesh({0}): Skipped degenerate triangle({1}, {2}, {3}) in submesh {4}", mesh.name, 3 * i + 0, 3 * i + 1, 3 * i + 2, s);
				}
			}
		}

		geometryData.numVertices = (uint)geometryData.vertices.Length;
		geometryData.numTriangles = (uint)triangleArrayIdx;
		geometryData.numSurfaces = (uint)geometryData.surfaces.Count();
	}

	/// <summary>
	/// Add or update an instance of the geometry by sending the transform of this component to Wwise.
	/// A geometry instance is a unique instance of a geometry set with a specified transform (position, rotation and scale).
	/// It is necessary to create at least one geometry instance for each geometry set that is to be used for diffraction and reflection simulation.
	/// </summary>
	/// <param name="geometryInstanceID">A unique ID to for the geometry instance. It must be unique amongst all geometry instances, including geometry instances referencing different geometries.</param>
	/// <param name="geometryID">The ID of the geometry referenced by this instance.</param>
	/// <param name="associatedRoomID">The ID of the room this geometry is encompassed in, if any.</param>
	/// <param name="transform">The transform to be applied to the geometry to convert it in world positions.</param>
	/// <param name="useForReflectionAndDiffraction">When enabled, the geometry instance triangles are used to compute reflection and diffraction. Set to false when using a geometry instance only to describe a room, and not for reflection and diffraction calculation.</param>
	public static void SetGeometryInstance(
		ulong geometryInstanceID,
		ulong geometryID,
		ulong associatedRoomID,
		UnityEngine.Transform transform,
		bool useForReflectionAndDiffraction)
	{
		if (!AkSoundEngine.IsInitialized())
		{
			return;
		}

#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying) return;
#endif

		AkTransform geometryTransform = new AkTransform();
		geometryTransform.Set(transform.position, transform.forward, transform.up);
		AkSoundEngine.SetGeometryInstance(geometryInstanceID, geometryTransform, transform.lossyScale, geometryID, associatedRoomID, useForReflectionAndDiffraction);
	}

	public void SetAssociatedRoom(AkRoom room)
	{
		if (AssociatedRoom != room)
		{
			AssociatedRoom = room;
			UpdateAssociatedRoom();
		}
	}

	public void UpdateAssociatedRoom()
	{
		UpdateGeometry();
		if (AssociatedRoom != null)
		{
			AkRoomManager.RegisterReflector(this);
		}
		else
		{
			AkRoomManager.UnregisterReflector(this);
		}
	}

	/// <summary>
	/// Call AkSurfaceReflector::SetGeometryFromMesh() with this component's mesh.
	/// </summary>
	public void SetGeometry()
	{
		if (!AkSoundEngine.IsInitialized())
		{
			return;
		}

		if (Mesh == null)
		{
			UnityEngine.Debug.LogFormat("SetGeometry({0}): No mesh found!", gameObject.name);
			return;
		}

		if (SetGeometryFromMesh(
			Mesh,
			GetID(),
			EnableDiffraction,
			EnableDiffractionOnBoundaryEdges,
			AcousticTextures,
			TransmissionLossValues,
			name))
		{
			isGeometrySetInWwise = true;
		}
	}

	/// <summary>
	/// Call AkSurfaceReflector::SetGeometryInstance() with this component's tranform.
	/// </summary>
	public void SetGeometryInstance()
	{
		SetGeometryInstance(
			GetID(),
			GetID(),
			AkRoom.GetAkRoomID(AssociatedRoom && AssociatedRoom.enabled ? AssociatedRoom : null), 
			transform,
			true);
	}

	/// <summary>
	/// Update this component's geometry instance
	/// </summary>
	public void UpdateGeometry()
	{
		SetGeometryInstance();
	}

	/// <summary>
	/// Remove this component's geometry and corresponding instance from Spatial Audio.
	/// </summary>
	public void RemoveGeometry()
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying) return;
#endif
		AkSoundEngine.RemoveGeometry(GetID());
	}

	/// <summary>
	/// Remove this component's geometry instance from Spatial Audio.
	/// </summary>
	public void RemoveGeometryInstance()
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying) return;
#endif
		AkSoundEngine.RemoveGeometryInstance(GetID());
	}

	private void Awake()
	{
#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer || AkUtilities.IsMigrating)
		{
			return;
		}

		var reference = AkWwiseTypes.DragAndDropObjectReference;
		if (reference)
		{
			UnityEngine.GUIUtility.hotControl = 0;

			if (AcousticTextures == null || AcousticTextures.Length < 1)
			{
				AcousticTextures = new AK.Wwise.AcousticTexture[1];
			}

			if (AcousticTextures[0] == null)
			{
				AcousticTextures[0] = new AK.Wwise.AcousticTexture();
			}

			AcousticTextures[0].ObjectReference = reference;
			AkWwiseTypes.DragAndDropObjectReference = null;
		}

		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif

		if (Mesh == null)
		{
			var meshFilter = GetComponent<UnityEngine.MeshFilter>();
			if (meshFilter != null)
			{
				Mesh = meshFilter.sharedMesh;
			}
		}
	}

	private void OnEnable()
	{
#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer || AkUtilities.IsMigrating || !UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif
		// need to call geometry, even if it might have already been sent to wwise, in case something changed while the component was disabled.
		SetGeometry();

		// init update conditions
		PreviousTransformState = GetTransformState();
		PreviousGeometryState = GetGeometryState();
		PreviousAssociatedRoomState = GetAssociatedRoomState();

		// Only SetGeometryInstance directly if there is no associated room because the room manager will set the geometry instance of registered reflectors.
		if (AssociatedRoom != null)
		{
			AkRoomManager.RegisterReflector(this);
		}
		else
		{
			SetGeometryInstance();
		}

		AkRoom roomComponent = gameObject.GetComponent<AkRoom>();
		if (roomComponent != null && roomComponent.isActiveAndEnabled && !roomComponent.UsesGeometry(GetID()))
		{
			roomComponent.SetRoom(GetID());
		}
	}

	private void OnDisable()
	{
#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer || AkUtilities.IsMigrating || !UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif

		RemoveGeometryInstance();
		AkRoomManager.UnregisterReflector(this);

		AkRoom roomComponent = gameObject.GetComponent<AkRoom>();
		if (roomComponent != null && roomComponent.isActiveAndEnabled && roomComponent.UsesGeometry(GetID()))
        {
			roomComponent.SetRoom(INVALID_GEOMETRY_ID);
		}
	}

	private void OnDestroy()
	{
#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer || AkUtilities.IsMigrating || !UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif
		if (isGeometrySetInWwise)
		{
			RemoveGeometry();
		}
	}

	private void Update()
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying) return;
#endif

		int CurrentGeometryState = GetGeometryState();
		int CurrentTransformState = GetTransformState();
		int CurrentAssociatedRoomState = GetAssociatedRoomState();

		if (PreviousGeometryState != CurrentGeometryState)
		{
			SetGeometry();
			PreviousGeometryState = CurrentGeometryState;
		}

		if (PreviousTransformState != CurrentTransformState)
		{
			UpdateGeometry();
			PreviousTransformState = CurrentTransformState;
		}

		if (PreviousAssociatedRoomState != CurrentAssociatedRoomState)
		{
			SetAssociatedRoom(AssociatedRoom);
			PreviousAssociatedRoomState = CurrentAssociatedRoomState;
		}
	}

	#region Obsolete
	[System.Obsolete(AkSoundEngine.Deprecation_2019_2_0)]
	public static ulong GetAkGeometrySetID(UnityEngine.MeshFilter meshFilter)
	{
		return (ulong)meshFilter.GetInstanceID();
	}

	[System.Obsolete(AkSoundEngine.Deprecation_2019_2_0)]
	public static void AddGeometrySet(
		AK.Wwise.AcousticTexture acousticTexture,
		UnityEngine.MeshFilter meshFilter,
		ulong roomID, bool enableDiffraction,
		bool enableDiffractionOnBoundaryEdges,
		bool enableTriangles)
	{
		if (!AkSoundEngine.IsInitialized())
		{
			return;
		}

		if (meshFilter == null)
		{
			UnityEngine.Debug.LogFormat("AddGeometrySet: No mesh found!");
			return;
		}

		var AcousticTextures = new[] { acousticTexture };

		var OcclusionValues = new[] { 1.0f };

		SetGeometryFromMesh(
			meshFilter.sharedMesh,
			GetAkGeometrySetID(meshFilter),
			enableDiffraction,
			enableDiffractionOnBoundaryEdges,
			AcousticTextures,
			OcclusionValues,
			meshFilter.name);

		SetGeometryInstance(GetAkGeometrySetID(meshFilter), GetAkGeometrySetID(meshFilter), roomID, meshFilter.transform, enableTriangles);
	}

	// for migration purpose, have a single acoustic texture parameter as a setter
	[System.Obsolete(AkSoundEngine.Deprecation_2019_2_0)]
	public AK.Wwise.AcousticTexture AcousticTexture
	{
		get
		{
			return (AcousticTextures == null || AcousticTextures.Length < 1) ? null : AcousticTextures[0];
		}
		set
		{
			var numAcousticTextures = (Mesh == null) ? 1 : Mesh.subMeshCount;
			if (AcousticTextures == null || AcousticTextures.Length < numAcousticTextures)
			{
				AcousticTextures = new AK.Wwise.AcousticTexture[numAcousticTextures];
			}

			for (int i = 0; i < numAcousticTextures; ++i)
			{
				AcousticTextures[i] = new AK.Wwise.AcousticTexture { WwiseObjectReference = value != null ? value.WwiseObjectReference : null };
			}
		}
	}

	[System.Obsolete(AkSoundEngine.Deprecation_2019_2_0)]
	public static void RemoveGeometrySet(UnityEngine.MeshFilter meshFilter)
	{
		if (meshFilter != null)
		{
			AkSoundEngine.RemoveGeometry(GetAkGeometrySetID(meshFilter));
		}
	}

	[System.Obsolete(AkSoundEngine.Deprecation_2021_1_0)]
	public float[] OcclusionValues
	{
		get
		{
			return TransmissionLossValues;
		}
		set
		{
			TransmissionLossValues = value;
		}
	}

	[System.Obsolete(AkSoundEngine.Deprecation_2022_1_0)]
	public static void SetGeometryFromMesh(
		UnityEngine.Mesh mesh,
		UnityEngine.Transform transform,
		ulong geometryID,
		ulong associatedRoomID,
		bool enableDiffraction,
		bool enableDiffractionOnBoundaryEdges,
		bool enableTriangles,
		AK.Wwise.AcousticTexture[] acousticTextures = null,
		float[] transmissionLossValues = null,
		string name = "")
	{
		SetGeometryFromMesh(
		mesh,
		geometryID,
		enableDiffraction,
		enableDiffractionOnBoundaryEdges,
		acousticTextures,
		transmissionLossValues,
		"");

		SetGeometryInstance(geometryID, geometryID, associatedRoomID, transform, enableTriangles);
	}

	[System.Obsolete(AkSoundEngine.Deprecation_2023_1_0)]
	public static void SetGeometryFromMesh(
		UnityEngine.Mesh mesh,
		ulong geometryID,
		bool enableDiffraction,
		bool enableDiffractionOnBoundaryEdges,
		bool enableTriangles,
		AK.Wwise.AcousticTexture[] acousticTextures = null,
		float[] transmissionLossValues = null,
		string name = "")
    {
		SetGeometryFromMesh(mesh,
		geometryID,
		enableDiffraction,
		enableDiffractionOnBoundaryEdges,
		acousticTextures,
		transmissionLossValues,
		"");
	}

	[System.Obsolete(AkSoundEngine.Deprecation_2023_1_0)]
	public static void SetGeometryInstance(
		ulong geometryInstanceID,
		ulong geometryID,
		ulong associatedRoomID,
		UnityEngine.Transform transform)
    {
		SetGeometryInstance(
			geometryInstanceID,
			geometryID,
			associatedRoomID,
			transform,
			true);

	}
	#endregion

	#region WwiseMigration
#pragma warning disable 0414 // private field assigned but not used.
	[UnityEngine.HideInInspector]
	[UnityEngine.SerializeField]
	[UnityEngine.Serialization.FormerlySerializedAs("AcousticTexture")]
	private AK.Wwise.AcousticTexture AcousticTextureInternal = new AK.Wwise.AcousticTexture();
#pragma warning restore 0414 // private field assigned but not used.

#if UNITY_EDITOR
	bool AK.Wwise.IMigratable.Migrate(UnityEditor.SerializedObject obj)
	{
		if (!AkUtilities.IsMigrationRequired(AkUtilities.MigrationStep.NewScriptableObjectFolder_v2019_2_0))
		{
			return false;
		}

		var hasChanged = false;

		var numAcousticTextures = 1;
		var meshProperty = obj.FindProperty("Mesh");
		if (meshProperty != null)
		{
			var meshFilter = GetComponent<UnityEngine.MeshFilter>();
			if (meshFilter)
			{
				var sharedMesh = meshFilter.sharedMesh;
				if (sharedMesh)
				{
					hasChanged = true;
					meshProperty.objectReferenceValue = sharedMesh;
					numAcousticTextures = sharedMesh.subMeshCount;
				}
			}
		}

		var oldwwiseObjRefProperty = obj.FindProperty("AcousticTextureInternal.WwiseObjectReference");
		if (oldwwiseObjRefProperty != null)
		{
			var objectReferenceValue = oldwwiseObjRefProperty.objectReferenceValue;
			if (objectReferenceValue != null)
			{
				hasChanged = true;
				var acousticTextures = obj.FindProperty("AcousticTextures");
				acousticTextures.arraySize = numAcousticTextures;
				for (int i = 0; i < numAcousticTextures; ++i)
				{
					acousticTextures.GetArrayElementAtIndex(i).FindPropertyRelative("WwiseObjectReference").objectReferenceValue = objectReferenceValue;
				}
			}
		}

		return hasChanged;
	}
#endif
	#endregion
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.