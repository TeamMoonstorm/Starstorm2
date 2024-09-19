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

[UnityEngine.AddComponentMenu("Wwise/AkBank")]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.DefaultExecutionOrder(-75)]
/// @brief Loads and unloads a SoundBank at a specified moment. Vorbis sounds can be decompressed at a specified moment using the decode compressed data option. In that case, the SoundBank will be prepared.
public class AkBank : AkTriggerHandler
#if UNITY_EDITOR
	, AK.Wwise.IMigratable
#endif
{
	public AK.Wwise.Bank data = new AK.Wwise.Bank();

	/// Decode this SoundBank upon load
	public bool decodeBank = false;

	public bool overrideLoadSetting = false;

	/// Check this to load the SoundBank in the background. Be careful, if Events are triggered and the SoundBank hasn't finished loading, you'll have "Event not found" errors.
	public bool loadAsynchronous = false;

	/// Save the decoded SoundBank to disk for faster loads in the future
	public bool saveDecodedBank = false;

	/// Reserved.
	public System.Collections.Generic.List<int> unloadTriggerList =
		new System.Collections.Generic.List<int> { DESTROY_TRIGGER_ID };

	protected override void Awake()
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
			data.ObjectReference = reference;
			AkWwiseTypes.DragAndDropObjectReference = null;
		}
		AkSoundEngineInitialization.Instance.initializationDelegate += HandleEvent;
#endif

		base.Awake();

		RegisterTriggers(unloadTriggerList, UnloadBank);
	}


#if UNITY_EDITOR
	public override void OnEnable()
	{
		if (UnityEditor.BuildPipeline.isBuildingPlayer)
        {
			return;
        }
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			HandleEvent();
		}
		base.OnEnable();
	}
#endif
	protected override void Start()
	{
#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer || AkUtilities.IsMigrating)
		{
			return;
		}
#endif

		base.Start();

		//Call the UnloadBank function if registered to the Start Trigger
		if (unloadTriggerList.Contains(START_TRIGGER_ID))
		{
			UnloadBank(null);
		}
	}

	/// Loads the SoundBank
	public override void HandleEvent(UnityEngine.GameObject in_gameObject)
	{
		bool asyncResult = loadAsynchronous;
		if(!overrideLoadSetting)
		{
			asyncResult = AkWwiseInitializationSettings.ActivePlatformSettings.LoadBanksAsynchronously;
		}
		if (asyncResult)
		{
			data.LoadAsync();
		}
		else
		{
			data.Load(decodeBank, saveDecodedBank);
		}
	}

	private void HandleEvent()
	{
		HandleEvent(gameObject);
	}

	/// Unloads a SoundBank
	public void UnloadBank(UnityEngine.GameObject in_gameObject)
	{
		data.Unload();
	}

	protected override void OnDestroy()
	{
#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer || AkUtilities.IsMigrating)
		{
			return;
		}
		AkSoundEngineInitialization.Instance.initializationDelegate -= HandleEvent;
#endif

		base.OnDestroy();

		UnregisterTriggers(unloadTriggerList, UnloadBank);
	}

	#region Obsolete
	[System.Obsolete(AkSoundEngine.Deprecation_2018_1_6)]
	public string bankName { get { return data == null ? string.Empty : data.Name; } }

	[System.Obsolete(AkSoundEngine.Deprecation_2018_1_6)]
	public byte[] valueGuid
	{
		get
		{
			if (data == null)
				return null;

			var objRef = data.ObjectReference;
			return !objRef ? null : objRef.Guid.ToByteArray();
		}
	}
	#endregion

	#region WwiseMigration
#pragma warning disable 0414 // private field assigned but not used.
	[UnityEngine.HideInInspector]
	[UnityEngine.SerializeField]
	[UnityEngine.Serialization.FormerlySerializedAs("bankName")]
	private string bankNameInternal;
	[UnityEngine.HideInInspector]
	[UnityEngine.SerializeField]
	[UnityEngine.Serialization.FormerlySerializedAs("valueGuid")]
	private byte[] valueGuidInternal;
#pragma warning restore 0414 // private field assigned but not used.

#if UNITY_EDITOR
	bool AK.Wwise.IMigratable.Migrate(UnityEditor.SerializedObject obj)
	{
		if (!AkUtilities.IsMigrationRequired(AkUtilities.MigrationStep.WwiseTypes_v2018_1_6))
			return false;

		return AK.Wwise.TypeMigration.ProcessSingleGuidType(obj.FindProperty("data.WwiseObjectReference"), WwiseObjectType.Soundbank, 
			obj.FindProperty("valueGuidInternal"), obj.FindProperty("bankNameInternal"));
	}
#endif
	#endregion
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.