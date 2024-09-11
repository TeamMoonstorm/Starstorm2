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

public class AkSoundEngineController
{
	private static AkSoundEngineController ms_Instance;

	public static AkSoundEngineController Instance
	{
		get
		{
			if (ms_Instance == null)
			{
				ms_Instance = new AkSoundEngineController();
			}

			return ms_Instance;
		}
	}

	private AkSoundEngineController()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.pauseStateChanged += OnPauseStateChanged;
		UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		AkSoundEngineInitialization.Instance.terminationDelegate += OnDisableEditorListener;
		AkSoundEngineInitialization.Instance.initializationDelegate += OnEnableEditorListener;
#endif
	}

	~AkSoundEngineController()
	{
		if (ms_Instance == this)
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.pauseStateChanged -= OnPauseStateChanged;
			UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			AkSoundEngineInitialization.Instance.terminationDelegate -= OnDisableEditorListener;
			AkSoundEngineInitialization.Instance.initializationDelegate -= OnEnableEditorListener;
			DisableEditorLateUpdate();
#endif
			ms_Instance = null;
		}
	}

#if UNITY_EDITOR
	public void EnableEditorLateUpdate()
	{
		UnityEditor.EditorApplication.update += LateUpdate;
	}

	public void DisableEditorLateUpdate()
	{
		UnityEditor.EditorApplication.update -= LateUpdate;
	}
#endif

	public void LateUpdate()
	{
		//Execute callbacks that occurred in last frame (not the current update)
		AkRoomManager.Update();
		AkRoomAwareManager.UpdateRoomAwareObjects();
		AkCallbackManager.PostCallbacks();
#if !(AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES)
		AkBankManager.DoUnloadBanks();
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
		AkSoundEngine.PerformStreamMgrIO();
#endif
		AkSoundEngine.RenderAudio();
	}

#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
	private AkWwiseAddressablesInitializationSettings GetInitSettingsInstance()
	{
		return AkWwiseAddressablesInitializationSettings.Instance;
	}
#else
	private AkWwiseInitializationSettings GetInitSettingsInstance()
	{
		return AkWwiseInitializationSettings.Instance;
	}
#endif

	public void Init(AkInitializer akInitializer)
	{
		// Only initialize the room manager during play.
		bool initRoomManager = true;
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			initRoomManager = false;
		}
#endif
		if (initRoomManager)
		{
			AkRoomManager.Init();
		}

		if (akInitializer == null)
		{
			UnityEngine.Debug.LogError("WwiseUnity: AkInitializer must not be null. Sound engine will not be initialized.");
			return;
		}

		var isInitialized = AkSoundEngine.IsInitialized();

		AkLogger.Instance.Init();

		if (isInitialized)
		{
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
			{
				OnEnableEditorListener(akInitializer.gameObject);
			}
			if (AkSoundEngineInitialization.Instance.ResetSoundEngine(UnityEngine.Application.isPlaying || UnityEditor.BuildPipeline.isBuildingPlayer))
			{
				EnableEditorLateUpdate();
			}

			if (UnityEditor.EditorApplication.isPaused && UnityEngine.Application.isPlaying)
			{
				AkSoundEngine.Suspend(true);
			}
#else
			UnityEngine.Debug.LogError("WwiseUnity: Sound engine is already initialized.");
#endif
			return;
		}

#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer)
		{
			return;
		}
#endif
		if (!AkSoundEngineInitialization.Instance.InitializeSoundEngine())
		{
			return;
		}
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			OnEnableEditorListener(akInitializer.gameObject);
		}
		EnableEditorLateUpdate();
#endif
	}

	public void OnDisable()
	{
#if UNITY_EDITOR
		if(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
		{
			OnDisableEditorListener();
		}
		if(!AkSoundEngineInitialization.Instance.ShouldKeepSoundEngineEnabled())
		{
			Terminate();
		}
#endif
	}

	public void Terminate()
	{
		AkSoundEngineInitialization.Instance.TerminateSoundEngine();
		AkRoomManager.Terminate();
	}

	// In the Editor, the sound needs to keep playing when switching windows (remote debugging in Wwise, for example).
	// On iOS, application interruptions are handled in the sound engine already.
#if UNITY_EDITOR || UNITY_IOS
	public void OnApplicationPause(bool pauseStatus)
	{
	}

	public void OnApplicationFocus(bool focus)
	{
	}
#elif UNITY_WEBGL
	// On WebGL, allow background audio when browser is un-focused in development builds to make the Wwise Profiler usable.
	public void OnApplicationPause(bool pauseStatus) 
	{
		if (!UnityEngine.Debug.isDebugBuild)
		{
			ActivateAudio(!pauseStatus);
		}
	}
	public void OnApplicationFocus(bool focus)
	{
		if (!UnityEngine.Debug.isDebugBuild)
		{
			ActivateAudio(focus, AkWwiseInitializationSettings.Instance.RenderDuringFocusLoss);
		}
	}
#else
	public void OnApplicationPause(bool pauseStatus) 
	{
		ActivateAudio(!pauseStatus);
	}

	public void OnApplicationFocus(bool focus)
	{
#if !UNITY_ANDROID
		ActivateAudio(focus,AkWwiseInitializationSettings.Instance.RenderDuringFocusLoss);
#endif
	}
#endif

#if UNITY_EDITOR
	// Enable/Disable the audio when pressing play/pause in the editor.
	private void OnPauseStateChanged(UnityEditor.PauseState pauseState)
	{
		if (UnityEngine.Application.isPlaying)
		{
			ActivateAudio(pauseState != UnityEditor.PauseState.Paused);
		}
	}

	private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
    {
		if(state.HasFlag(UnityEditor.PlayModeStateChange.ExitingEditMode) || state.HasFlag(UnityEditor.PlayModeStateChange.ExitingPlayMode))
		{
			AkSoundEngine.StopAll();
		}
    }
#endif

#if UNITY_EDITOR || !UNITY_IOS
	private void ActivateAudio(bool activate, bool renderAnyway = false)
	{
		if (AkSoundEngine.IsInitialized() && AkWwiseInitializationSettings.Instance.SuspendAudioDuringFocusLoss)
		{
			if (activate)
			{
				AkSoundEngine.WakeupFromSuspend();
			}
			else
			{
				AkSoundEngine.Suspend(renderAnyway);
			}

			AkSoundEngine.RenderAudio();
		}
	}
#endif

#if UNITY_EDITOR
#region Editor Listener
	private UnityEngine.GameObject editorListenerGameObject;

	private bool IsPlayingOrIsNotInitialized
	{
		get { return UnityEngine.Application.isPlaying || !AkSoundEngine.IsInitialized(); }
	}

	public bool EditorListenerIsInitialized()
	{
		return editorListenerGameObject != null;
	}

	private void OnEnableEditorListener()
	{
		OnEnableEditorListener(AkInitializer.GetAkInitializerGameObject());
	}

	private void OnEnableEditorListener(UnityEngine.GameObject gameObject)
	{
		if (editorListenerGameObject != null || IsPlayingOrIsNotInitialized)
		{
			return;
		}

		if(gameObject == null)
		{
			return;
		}

		editorListenerGameObject = gameObject;
		AkSoundEngine.RegisterGameObj(editorListenerGameObject, editorListenerGameObject.name);

		// Do not create AkGameObj component when adding this listener
		var id = AkSoundEngine.GetAkGameObjectID(editorListenerGameObject);
		AkSoundEngine.AddDefaultListener(id);
		UnityEditor.EditorApplication.update += UpdateEditorListenerPosition;
	}

	private void OnDisableEditorListener()
	{
		if (IsPlayingOrIsNotInitialized || editorListenerGameObject == null)
		{
			return;
		}

		UnityEditor.EditorApplication.update -= UpdateEditorListenerPosition;

		var id = AkSoundEngine.GetAkGameObjectID(editorListenerGameObject);
		AkSoundEngine.RemoveDefaultListener(id);

		AkSoundEngine.UnregisterGameObj(editorListenerGameObject);
		editorListenerGameObject = null;
		editorListenerForward = UnityEngine.Vector3.zero;
		editorListenerPosition = UnityEngine.Vector3.zero;
		editorListenerUp = UnityEngine.Vector3.zero;
	}

	private UnityEngine.Vector3 editorListenerPosition = UnityEngine.Vector3.zero;
	private UnityEngine.Vector3 editorListenerForward = UnityEngine.Vector3.zero;
	private UnityEngine.Vector3 editorListenerUp = UnityEngine.Vector3.zero;

	private void UpdateEditorListenerPosition()
	{
		if (IsPlayingOrIsNotInitialized || editorListenerGameObject == null)
		{
			return;
		}

		if (UnityEditor.SceneView.lastActiveSceneView == null)
		{
			return;
		}

		var sceneViewCamera = UnityEditor.SceneView.lastActiveSceneView.camera;
		if (sceneViewCamera == null)
		{
			return;
		}

		var sceneViewTransform = sceneViewCamera.transform;
		if (sceneViewTransform == null)
		{
			return;
		}

		if (editorListenerPosition == sceneViewTransform.position &&
			editorListenerForward == sceneViewTransform.forward &&
			editorListenerUp == sceneViewTransform.up)
		{
			return;
		}

		AkSoundEngine.SetObjectPosition(editorListenerGameObject, sceneViewTransform);

		editorListenerPosition = sceneViewTransform.position;
		editorListenerForward = sceneViewTransform.forward;
		editorListenerUp = sceneViewTransform.up;
	}
#endregion
#endif // UNITY_EDITOR
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.