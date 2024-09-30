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

#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
using AK.Wwise.Unity.WwiseAddressables;
#endif

[UnityEngine.AddComponentMenu("Wwise/AkInitializer")]
[UnityEngine.ExecuteAlways]
[UnityEngine.DisallowMultipleComponent]
[UnityEngine.DefaultExecutionOrder(-100)]
/// @brief This script deals with initialization, and frame updates of the Wwise audio engine.  
/// It is marked as \c DontDestroyOnLoad so it stays active for the life of the game, 
/// not only one scene. Double-click the Initialization Settings entry, AkWwiseInitializationSettings, 
/// to review and edit Wwise initialization settings.
/// \sa
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=workingwithsdks__initialization.html" target="_blank">Initialize the Different Modules of the Sound Engine</a> (Note: This is described in the Wwise SDK documentation.)
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=namespace_a_k_1_1_sound_engine_a9a26da64092b97243844df77cbcdbf5f.html" target="_blank">AK::SoundEngine::Init()</a> (Note: This is described in the Wwise SDK documentation.)
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=namespace_a_k_1_1_sound_engine_a90f8c91937038615480db2b57ce2279e.html" target="_blank">AK::SoundEngine::Term()</a> (Note: This is described in the Wwise SDK documentation.)
/// - <a href="https://www.audiokinetic.com/en/library/edge/?source=Unity&id=enter_play_mode_behaviors.html" target="_blank">Enter Play Mode Behaviors</a>
/// - AkCallbackManager
public class AkInitializer : UnityEngine.MonoBehaviour
{
	private static AkInitializer ms_Instance;
#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
	public AkWwiseAddressablesInitializationSettings InitializationSettings;
#else
	public AkWwiseInitializationSettings InitializationSettings;
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern bool AkVerifyPluginRegistration();
#endif

	public static AkSurfaceReflector.GeometryData CubeGeometryData;
	public static AkSurfaceReflector.GeometryData SphereGeometryData;

	/// <summary>
	/// Create Spatial Audio Geometry from Unity Box and Sphere Colliders.
	/// When an AkRoom component is placed on a GameObject without a SurfaceReflector component, the AkRoom component's geometry is based on its sibling collider component.
	/// Box, capsule, sphere, and mesh colliders can be converted to Spatial Audio Geometries and used as the geometry of Rooms.
	/// For Box and sphere colliders, the geometry data is saved here to be used later for each corresponding AkRoom component.
	/// </summary>
	private void CreateRoomGeometryData()
	{
		float[] transmissionLossValue = { 0 };

		// Cube Geometry
		UnityEngine.GameObject tempGameObject = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
		UnityEngine.Mesh mesh = tempGameObject.GetComponent<UnityEngine.MeshFilter>().sharedMesh;
		AkSurfaceReflector.GetGeometryDataFromMesh(mesh, ref CubeGeometryData, null, transmissionLossValue);
		UnityEngine.GameObject.DestroyImmediate(tempGameObject);

		// Sphere Geometry
		tempGameObject = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Sphere);
		mesh = tempGameObject.GetComponent<UnityEngine.MeshFilter>().sharedMesh;
		AkSurfaceReflector.GetGeometryDataFromMesh(mesh, ref SphereGeometryData, null, transmissionLossValue);
		UnityEngine.GameObject.DestroyImmediate(tempGameObject);
	}

	private void Awake()
	{
#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer)
		{
			return;
		}
#endif

		if (ms_Instance)
		{
			DestroyImmediate(this);
			return;
		}

		ms_Instance = this;

#if UNITY_EDITOR
		UnityEditor.EditorApplication.quitting += OnApplicationQuit;

		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
	#if !(AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES)
		AkWwiseSoundbanksInfoXMLFileWatcher.Instance.XMLUpdated += AkBankManager.ReloadAllBanks;
	#endif
#endif

		DontDestroyOnLoad(this);
	}

	private bool IsInstance()
	{
		if(ms_Instance == null)
		{
			ms_Instance = this;
			return true;
		}
		return ms_Instance == this;
	}

	public static UnityEngine.GameObject GetAkInitializerGameObject()
    {
		if(ms_Instance != null)
        {
			return ms_Instance.gameObject;
        }
		UnityEngine.Debug.LogWarning("AkInitializer is null.");
		return null;
	}

	private void OnEnable()
	{
#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer || UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
		{
			return;
		}
		if (!AkWwiseEditorSettings.Instance.LoadSoundEngineInEditMode && !UnityEngine.Application.isPlaying)
		{
			return;
		}
		if(!UnityEngine.Application.isPlaying && UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}
#endif

		InitializeInitializationSettings();
#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES && UNITY_EDITOR
		var bankHolder = UnityEngine.Object.FindObjectOfType<AK.Wwise.Unity.WwiseAddressables.InitBankHolder>();
		if (bankHolder == null)
		{
			bankHolder = UnityEditor.Undo.AddComponent<AK.Wwise.Unity.WwiseAddressables.InitBankHolder>(gameObject);
		}
#endif

if (IsInstance())
		{
#if UNITY_WEBGL && !UNITY_EDITOR
			bool bRegistered = AkVerifyPluginRegistration();
			if (!bRegistered)
				UnityEngine.Debug.Log("Wwise plug-in registration has failed. Some plug-ins may fail to initialize.");
#endif
			AkSoundEngineController.Instance.Init(this);
			CreateRoomGeometryData();
		}
	}

	public void InitializeInitializationSettings()
	{
#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
		InitializationSettings = AkWwiseAddressablesInitializationSettings.Instance;
#else
		InitializationSettings = AkWwiseInitializationSettings.Instance;
#endif
	}

	private void OnDisable()
	{
#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer)
		{
			return;
		}
#endif
		if (IsInstance())
		{
			AkSoundEngineController.Instance.OnDisable();
		}
	}

	private void OnDestroy()
	{
#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer)
		{
			return;
		}
#endif

		if (IsInstance())
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.quitting -= OnApplicationQuit;
#endif
			ms_Instance = null;
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (IsInstance())
		{
			AkSoundEngineController.Instance.OnApplicationPause(pauseStatus);
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (IsInstance())
		{
			AkSoundEngineController.Instance.OnApplicationFocus(focus);
		}
	}

	private void OnApplicationQuit()
	{
		if (IsInstance() && !AkSoundEngineInitialization.Instance.ShouldKeepSoundEngineEnabled())
		{
			AkSoundEngineController.Instance.Terminate();
		}
	}

	//Use LateUpdate instead of Update() to ensure all gameobjects positions, listener positions, environements, RTPC, etc are set before finishing the audio frame.
	private void LateUpdate()
	{
		if (IsInstance())
		{
			AkSoundEngineController.Instance.LateUpdate();
		}
	}

#region WwiseMigration
#if UNITY_EDITOR
#pragma warning disable 0414 // private field assigned but not used.

	// previously serialized data that will be consumed by migration
	[UnityEngine.HideInInspector][UnityEngine.SerializeField] private string basePath = string.Empty;
	[UnityEngine.HideInInspector][UnityEngine.SerializeField] private string language = string.Empty;
	[UnityEngine.HideInInspector][UnityEngine.SerializeField] private int defaultPoolSize = 0;
	[UnityEngine.HideInInspector][UnityEngine.SerializeField] private int lowerPoolSize = 0;
	[UnityEngine.HideInInspector][UnityEngine.SerializeField] private int streamingPoolSize = 0;
	[UnityEngine.HideInInspector][UnityEngine.SerializeField] private float memoryCutoffThreshold = 0f;
	[UnityEngine.HideInInspector][UnityEngine.SerializeField] private int monitorPoolSize = 0;
	[UnityEngine.HideInInspector][UnityEngine.SerializeField] private int monitorQueuePoolSize = 0;
	[UnityEngine.HideInInspector][UnityEngine.SerializeField] private int callbackManagerBufferSize = 0;
	[UnityEngine.HideInInspector][UnityEngine.SerializeField] private int spatialAudioPoolSize = 0;
	[UnityEngine.HideInInspector][UnityEngine.SerializeField] private uint maxSoundPropagationDepth = 0;
	[UnityEngine.HideInInspector][UnityEngine.SerializeField] private bool engineLogging = false;

#pragma warning restore 0414 // private field assigned but not used.

	private class Migration15Data
	{
		bool hasMigrated = false;

		public void Migrate(AkInitializer akInitializer)
		{
			if (hasMigrated)
				return;

			var initializationSettings = akInitializer.InitializationSettings;
			if (!initializationSettings)
			{
#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
				initializationSettings = AkWwiseAddressablesInitializationSettings.Instance;
#else
				initializationSettings = AkWwiseInitializationSettings.Instance;
#endif
				if (!initializationSettings)
					return;
			}

			if (initializationSettings.UserSettings == null)
			{
				initializationSettings.UserSettings = new AkCommonUserSettings();
			}

			if (initializationSettings.AdvancedSettings == null)
			{
				initializationSettings.AdvancedSettings = new AkCommonAdvancedSettings();
			}

			if (initializationSettings.CommsSettings == null)
			{
				initializationSettings.CommsSettings = new AkCommonCommSettings();
			}

			if (initializationSettings.UserSettings.m_SpatialAudioSettings == null)
			{
				initializationSettings.UserSettings.m_SpatialAudioSettings =
					new AkCommonUserSettings.SpatialAudioSettings();
			}

			initializationSettings.UserSettings.m_BasePath = akInitializer.basePath;
			initializationSettings.UserSettings.m_StartupLanguage = akInitializer.language;

			initializationSettings.AdvancedSettings.m_MonitorQueuePoolSize = (uint)akInitializer.monitorQueuePoolSize * 1024;

			initializationSettings.UserSettings.m_SpatialAudioSettings.m_MaxSoundPropagationDepth = akInitializer.maxSoundPropagationDepth;

			initializationSettings.CallbackManagerInitializationSettings.IsLoggingEnabled = akInitializer.engineLogging;

			UnityEditor.EditorUtility.SetDirty(initializationSettings);
			UnityEditor.AssetDatabase.SaveAssets();
			
			UnityEngine.Debug.Log("WwiseUnity: Converted from AkInitializer to AkWwiseInitializationSettings.");
			hasMigrated = true;
		}
	}

	private static Migration15Data migration15data;

	public static void PreMigration15()
	{
		migration15data = new Migration15Data();
	}

	public void Migrate15()
	{
		UnityEngine.Debug.Log("WwiseUnity: AkInitializer.Migrate15 for " + gameObject.name);

		if (migration15data != null)
		{
			migration15data.Migrate(this);
		}
	}

	public static void PostMigration15()
	{
		migration15data = null;
	}
#endif
#endregion
			}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.