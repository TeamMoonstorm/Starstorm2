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

[UnityEngine.AddComponentMenu("Wwise/AkEnvironment")]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(UnityEngine.Collider))]
/// @brief Use this component to define a reverb zone. This needs to be added to a collider object to work properly. See \ref unity_use_AkEnvironment_AkEnvironmentPortal.
/// @details This component can be attached to any collider. You can specify a roll-off to fade-in/out of the reverb.  
/// The reverb parameters will be defined in the Wwise project, by the sound designer.  All AkGameObj that are 
/// "environment"-aware will receive a send value when entering the attached collider.
/// \sa
/// - \ref unity_use_AkEnvironment_AkEnvironmentPortal
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=integrating__elements__environments.html" target="_blank">Integrating Environments and Game-defined Auxiliary Sends</a> (Note: This is described in the Wwise SDK documentation.)
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=namespace_a_k_1_1_sound_engine_af960fca0239e219b9d08c2659fe9e5d4.html" target="_blank">AK::SoundEngine::SetGameObjectAuxSendValues</a> (Note: This is described in the Wwise SDK documentation.)
public class AkEnvironment : UnityEngine.MonoBehaviour
#if UNITY_EDITOR
	, AK.Wwise.IMigratable
#endif
{
	public const int MAX_NB_ENVIRONMENTS = 4;

	public static AkEnvironment_CompareByPriority s_compareByPriority = new AkEnvironment_CompareByPriority();

	public static AkEnvironment_CompareBySelectionAlgorithm s_compareBySelectionAlgorithm =
		new AkEnvironment_CompareBySelectionAlgorithm();

	//if excludeOthers, then only the environment with the excludeOthers flag set to true and with the highest priority will be active
	public bool excludeOthers = false;

	//if isDefault, then this environment will be bumped out if any other is present 
	public bool isDefault = false;

	public AK.Wwise.AuxBus data = new AK.Wwise.AuxBus();

	//Cache of the colliders for this environment, to avoid calls to GetComponent.
	public UnityEngine.Collider Collider { get; private set; }

	//smaller number has a higher priority
	public int priority = 0;

	public void Awake()
	{
#if UNITY_EDITOR
		var reference = AkWwiseTypes.DragAndDropObjectReference;
		if (reference)
		{
			UnityEngine.GUIUtility.hotControl = 0;
			data.ObjectReference = reference;
			AkWwiseTypes.DragAndDropObjectReference = null;
		}
#endif

		Collider = GetComponent<UnityEngine.Collider>();
	}

	/// @brief Sorts AkEnvironments based on their priorities.
	public class AkEnvironment_CompareByPriority : System.Collections.Generic.IComparer<AkEnvironment>
	{
		public virtual int Compare(AkEnvironment a, AkEnvironment b)
		{
			var result = a.priority.CompareTo(b.priority);
			return result == 0 && a != b ? 1 : result;
		}
	}

	/// @brief Sorts AkEnvironments based on the selection algorithm.
	/// The selection algorithm is as follows: 
	/// -# Environments have priorities.
	/// -# Environments have a "Default" flag. This flag effectively says that this environment will be bumped out if any other is present.
	/// -# Environments have an "Exclude Other" flag. This flag will tell that this env is not overlappable with others. So, only one (the highest priority) should be selected.
	public class AkEnvironment_CompareBySelectionAlgorithm : AkEnvironment_CompareByPriority
	{
		public override int Compare(AkEnvironment a, AkEnvironment b)
		{
			if (a.isDefault)
				return b.isDefault ? base.Compare(a, b) : 1;

			if (b.isDefault)
				return -1;

			if (a.excludeOthers)
				return b.excludeOthers ? base.Compare(a, b) : -1;

			return b.excludeOthers ? 1 : base.Compare(a, b);
		}
	}

	#region Obsolete
	[System.Obsolete(AkSoundEngine.Deprecation_2018_1_2)]
	public int m_auxBusID { get { return (int)(data == null ? AkSoundEngine.AK_INVALID_UNIQUE_ID : data.Id); } }

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

	[System.Obsolete(AkSoundEngine.Deprecation_2018_1_2)]
	public uint GetAuxBusID()
	{
		return data.Id;
	}

	[System.Obsolete(AkSoundEngine.Deprecation_2018_1_6)]
	public UnityEngine.Collider GetCollider()
	{
		return Collider;
	}
	#endregion

	#region WwiseMigration
#pragma warning disable 0414 // private field assigned but not used.
	[UnityEngine.HideInInspector]
	[UnityEngine.SerializeField]
	[UnityEngine.Serialization.FormerlySerializedAs("m_auxBusID")]
	private int auxBusIdInternal = (int)AkSoundEngine.AK_INVALID_UNIQUE_ID;
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

		return AK.Wwise.TypeMigration.ProcessSingleGuidType(obj.FindProperty("data.WwiseObjectReference"), WwiseObjectType.AuxBus, 
			obj.FindProperty("valueGuidInternal"), obj.FindProperty("auxBusIdInternal"));
	}
#endif

	#endregion
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.