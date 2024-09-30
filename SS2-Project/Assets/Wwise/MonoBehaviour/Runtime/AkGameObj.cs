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

[UnityEngine.AddComponentMenu("Wwise/AkGameObj")]
[UnityEngine.DisallowMultipleComponent]
[UnityEngine.ExecuteInEditMode] //ExecuteInEditMode necessary to maintain proper state of isStaticObject.
[UnityEngine.DefaultExecutionOrder(-25)]
///@brief This component represents a sound object in your scene and tracks its position and other game syncs such as Switches, RTPCs, and environment values. You can add this to any object that emits sound, and it is added to any object to which an AkAudioListener is attached. If it is not present, Wwise adds it automatically, with the default values, to any Unity Game Object that is passed to Wwise.
/// \sa
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=soundengine__gameobj.html" target="_blank">Integration Details - Game Objects</a> (SDK documentation)
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=soundengine__events.html" target="_blank">Integration Details - Events</a> (SDK documentation)
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=soundengine__listeners.html" target="_blank">Integrating Listeners</a> (SDK documentation)
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=soundengine__switch.html" target="_blank">Integration Details - Switches</a> (SDK documentation)
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=soundengine__states.html" target="_blank">Integration Details - States</a> (SDK documentation)
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=soundengine__environments.html" target="_blank">Integration Details - Environments and Game-defined Auxiliary Sends</a> (SDK documentation)
public class AkGameObj : UnityEngine.MonoBehaviour
{
	[UnityEngine.SerializeField] private AkGameObjListenerList m_listeners = new AkGameObjListenerList();

	/// Indicates whether the object is affected by environmental changes. Set to false if the object is not affected in order to save some unnecessary calls. The default value is true.
	public bool isEnvironmentAware = true;

	/// Maintains and persists the Static setting of the game object, which is available only in the editor.
	[UnityEngine.SerializeField] private bool isStaticObject = false;

	/// Cache the bounds to avoid calls to GetComponent()
	private UnityEngine.Collider m_Collider;

	private AkGameObjEnvironmentData m_envData;

	private AkGameObjPositionData m_posData;

	/// When not set to null, the position is offset relative to the Game Object position by the Position Offset
	public AkGameObjPositionOffsetData m_positionOffsetData;

	private float scalingFactor = -1f;

	public float ScalingFactor
	{
		get => scalingFactor;
		set
		{
			if (value < 0)
			{
				scalingFactor = 0;
			}
			else
			{
				scalingFactor = value;
			}
		}
	}

	public bool IsUsingDefaultListeners
	{
		get { return m_listeners.useDefaultListeners; }
	}

	public System.Collections.Generic.List<AkAudioListener> ListenerList
	{
		get { return m_listeners.ListenerList; }
	}

	private bool isRegistered = false;

	internal void AddListener(AkAudioListener listener)
	{
		m_listeners.Add(listener);
	}

	internal void RemoveListener(AkAudioListener listener)
	{
		m_listeners.Remove(listener);
	}

	public AKRESULT Register()
	{
		if (isRegistered)
		{
			return AKRESULT.AK_Success;
		}

		isRegistered = true;
		return AkSoundEngine.RegisterGameObj(gameObject, gameObject.name);
	}

	private void UnregisterGameObject()
	{
		if (AkSoundEngine.IsInitialized())
        {
			Unregister();
		}
    }

	public AKRESULT Unregister()
	{
		if (!isRegistered)
		{
			return AKRESULT.AK_Success;
		}

		isRegistered = false;
		m_posData = null;
		return AkSoundEngine.UnregisterGameObj(gameObject);
	}
	
	private void SetPosition()
	{
		var position = GetPosition();
		var forward = GetForward();
		var up = GetUpward();

		if (m_posData != null)
		{
			if (m_posData.position == position && m_posData.forward == forward && m_posData.up == up)
			{
				return;
			}

			m_posData.position = position;
			m_posData.forward = forward;
			m_posData.up = up;
		}

		AkSoundEngine.SetObjectPosition(gameObject, position, forward, up);
	}

	private void Awake()
	{
#if UNITY_EDITOR
		if (AkUtilities.IsMigrating)
		{
			return;
		}

		if (!UnityEditor.EditorApplication.isPlaying)
		{
			UnityEditor.EditorApplication.update += CheckStaticStatus;
		}
		AkSoundEngineInitialization.Instance.initializationDelegate += RegisterGameObject;
		AkSoundEngineInitialization.Instance.terminationDelegate += UnregisterGameObject;
#endif

		// If the object was marked as static, don't update its position to save cycles.
		if (!isStaticObject)
		{
			m_posData = new AkGameObjPositionData();
		}

		// Cache the bounds to avoid calls to GetComponent()
		m_Collider = GetComponent<UnityEngine.Collider>();
	}

    private void RegisterGameObject()
    {
		if (!AkSoundEngine.IsInitialized())
        {
			return;
        }

		//Register a Game Object in the sound engine, with its name.
		if (Register() == AKRESULT.AK_Success)
		{
			SetPosition();

			if (isEnvironmentAware)
			{
				m_envData = new AkGameObjEnvironmentData();

				if (m_Collider)
				{
					m_envData.AddAkEnvironment(m_Collider, m_Collider);
				}

				m_envData.UpdateAuxSend(gameObject, transform.position);
			}

			m_listeners.Init(this);
			if (scalingFactor < 0f)
			{
				var initializer = AkInitializer.GetAkInitializerGameObject();
				if (initializer)
				{
					scalingFactor = initializer.GetComponent<AkInitializer>().InitializationSettings.UserSettings.m_DefaultScalingFactor;
				}
				else
				{
					scalingFactor = 1f;
				}
			}
			AkSoundEngine.SetScalingFactor(gameObject, scalingFactor);
		}
	}

	private void CheckStaticStatus()
	{
#if UNITY_EDITOR
		if (AkUtilities.IsMigrating)
		{
			return;
		}

		try
		{
			if (gameObject != null && isStaticObject != gameObject.isStatic)
			{
				isStaticObject = gameObject.isStatic;
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}
		catch
		{
			UnityEditor.EditorApplication.update -= CheckStaticStatus;
		}
#endif
	}

	private void OnEnable()
	{
#if UNITY_EDITOR
		if (AkUtilities.IsMigrating)
		{
			return;
		}
#endif
		RegisterGameObject();
	}

#if UNITY_EDITOR
	private void OnDisable()
	{
		if (!AkSoundEngineInitialization.Instance.ShouldKeepSoundEngineEnabled())
		{
			Unregister();
		}
	}
#endif

	private void OnDestroy()
	{
#if UNITY_EDITOR
		if (AkUtilities.IsMigrating)
		{
			return;
		}

		if (!UnityEditor.EditorApplication.isPlaying)
		{
			UnityEditor.EditorApplication.update -= CheckStaticStatus;
		}

		AkSoundEngineInitialization.Instance.initializationDelegate -= RegisterGameObject;
		AkSoundEngineInitialization.Instance.terminationDelegate -= UnregisterGameObject;
#endif

		// We can't do the code in OnDestroy if the gameObj is unregistered, so do it now.
		var eventHandlers = gameObject.GetComponents<AkTriggerHandler>();
		foreach (var handler in eventHandlers)
		{
			if (handler.triggerList.Contains(AkTriggerHandler.DESTROY_TRIGGER_ID))
			{
				handler.DoDestroy();
			}
		}

		if (AkSoundEngine.IsInitialized())
		{
			Unregister();
		}
	}

	private void Update()
	{
#if UNITY_EDITOR
		if (AkUtilities.IsMigrating || !UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif

		if (!isStaticObject)
		{
			if (m_envData != null)
			{
				m_envData.UpdateAuxSend(gameObject, transform.position);
			}

			SetPosition();			
		}
	}

	/// Gets the position including the position offset, if applyPositionOffset is enabled. User can also override this method to calculate an arbitrary position.
	/// \return  The position.
	public virtual UnityEngine.Vector3 GetPosition()
	{
		if (m_positionOffsetData == null)
		{
			return transform.position;
		}

		var worldOffset = transform.rotation * m_positionOffsetData.positionOffset;
		return transform.position + worldOffset;
	}

	/// Gets the orientation forward vector. User can also override this method to calculate an arbitrary vector.
	/// \return  The forward vector of orientation.
	public virtual UnityEngine.Vector3 GetForward()
	{
		return transform.forward;
	}

	/// Gets the orientation upward vector. User can also override this method to calculate an arbitrary vector.
	/// \return  The upward vector of orientation.
	public virtual UnityEngine.Vector3 GetUpward()
	{
		return transform.up;
	}

	private void OnTriggerEnter(UnityEngine.Collider other)
	{
#if UNITY_EDITOR
		if (AkUtilities.IsMigrating || !UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif

		if (isEnvironmentAware && m_envData != null)
		{
			m_envData.AddAkEnvironment(other, m_Collider);
		}
	}

	private void OnTriggerExit(UnityEngine.Collider other)
	{
#if UNITY_EDITOR
		if (AkUtilities.IsMigrating || !UnityEditor.EditorApplication.isPlaying)
		{
			return;
		}
#endif

		if (isEnvironmentAware && m_envData != null)
		{
			m_envData.RemoveAkEnvironment(other, m_Collider);
		}
	}

#if UNITY_EDITOR
	public void OnDrawGizmosSelected()
	{
		if (AkUtilities.IsMigrating)
		{
			return;
		}

		var position = GetPosition();
		UnityEngine.Gizmos.DrawIcon(position, "WwiseAudioSpeaker.png", false);
	}
#endif

	#region WwiseMigration

#pragma warning disable 0414 // private field assigned but not used.

	[UnityEngine.HideInInspector]
	[UnityEngine.SerializeField]
	private AkGameObjPosOffsetData m_posOffsetData;

	// Wwise v2016.2 and below supported up to 8 listeners[0-7].
	private const int AK_NUM_LISTENERS = 8;

	[UnityEngine.HideInInspector]
	[UnityEngine.SerializeField]
	/// Listener 0 by default.
	private int listenerMask = 1;

#pragma warning restore 0414 // private field assigned but not used.

#if UNITY_EDITOR
	public void Migrate9()
	{
		UnityEngine.Debug.Log("WwiseUnity: AkGameObj.Migrate9 for " + gameObject.name);

		const int ALL_LISTENER_MASK = (1 << AK_NUM_LISTENERS) - 1;
		if ((listenerMask & ALL_LISTENER_MASK) == ALL_LISTENER_MASK)
			listenerMask = 1;
	}

	public void Migrate10()
	{
		UnityEngine.Debug.Log("WwiseUnity: AkGameObj.Migrate10 for " + gameObject.name);

		if (m_posOffsetData != null)
		{
			m_positionOffsetData = new AkGameObjPositionOffsetData(true);
			m_positionOffsetData.positionOffset = m_posOffsetData.positionOffset;
			m_posOffsetData = null;
		}
	}

	private class Migration14Data
	{
		private readonly System.Collections.Generic.List<AkAudioListener>[] listeners =
			new System.Collections.Generic.List<AkAudioListener>[AK_NUM_LISTENERS];

		public Migration14Data()
		{
			var fullSceneListenerMask = 0;

			// Get all AkAudioListeners in the scene.
			var listenerObjects = FindObjectsOfType<AkAudioListener>();
			foreach (var listener in listenerObjects)
			{
				// Add AkGameObj to AkAudioListeners
				if (listener.GetComponent<AkGameObj>() == null)
				{
					var akGameObj = listener.gameObject.AddComponent<AkGameObj>();
					if (akGameObj)
					{
						akGameObj.isEnvironmentAware = false;
						UnityEngine.Debug.Log("WwiseUnity: Added AkGameObj to <" + listener.gameObject.name + ">.");
					}
					else
						UnityEngine.Debug.LogError("WwiseUnity: Failed to add AkGameObj to <" + listener.gameObject.name + ">.");
				}

				var listenerId = listener.listenerId;
				if (listenerId >= 0 && listenerId < AK_NUM_LISTENERS)
				{
					if (listeners[listenerId] == null)
						listeners[listenerId] = new System.Collections.Generic.List<AkAudioListener>();

					listeners[listenerId].Add(listener);
					fullSceneListenerMask |= 1 << listenerId;
				}
				else
					UnityEngine.Debug.LogError("WwiseUnity: Invalid listenerId <" + listenerId + "> found during migration.");
			}

			if (fullSceneListenerMask == 0)
			{
				UnityEngine.Debug.LogWarning("WwiseUnity: Listeners were not added via components within this Scene.");
				listeners = null;
			}
			else
			{
				for (var ii = 0; ii < AK_NUM_LISTENERS; ++ii)
				{
					if (listeners[ii] != null && listeners[ii].Count > 1)
					{
						UnityEngine.Debug.LogWarning("WwiseUnity: Multiple listeners <" + listeners[ii].Count +
						                             "> with same listenerId <" + ii + "> found during migration.");
					}
				}

				if (fullSceneListenerMask == 1)
				{
					UnityEngine.Debug.Log("WwiseUnity: Default listeners will be used for this Scene.");
					listeners = null;
				}
			}
		}

		public void Migrate(AkGameObj akGameObj)
		{
			if (listeners != null)
			{
				for (var ii = 0; ii < AK_NUM_LISTENERS; ++ii)
				{
					var idMask = 1 << ii;
					if ((akGameObj.listenerMask & idMask) != 0 && listeners[ii] != null)
					{
						foreach (var listener in listeners[ii])
							akGameObj.m_listeners.AddToInitialListenerList(listener);
					}
				}
			}
		}
	}

	private static Migration14Data migration14data;

	public static void PreMigration14()
	{
		migration14data = new Migration14Data();
	}

	public void Migrate14()
	{
		UnityEngine.Debug.Log("WwiseUnity: AkGameObj.Migrate14 for " + gameObject.name);

		if (migration14data != null)
			migration14data.Migrate(this);
	}

	public static void PostMigration14()
	{
		migration14data = null;
	}

#endif

	#endregion
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.