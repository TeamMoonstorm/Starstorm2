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

[UnityEngine.AddComponentMenu("Wwise/AkWwiseTrigger")]
[UnityEngine.ExecuteInEditMode]
/// @brief This will call \c AkSoundEngine.PostTrigger() whenever the selected Unity event is triggered. For example this component could be set on a Unity collider to trigger when an object enters it.
/// \sa 
/// - <a href="https://www.audiokinetic.com/en/library/edge/?source=Help&id=working_with_triggers_overview" target="_blank">Working with Triggers > Overview</a> (Note: This is described in the Wwise SDK documentation.)
public class AkWwiseTrigger : AkDragDropTriggerHandler
#if UNITY_EDITOR
        , AK.Wwise.IMigratable
#endif
    {
	public AK.Wwise.Trigger data = new AK.Wwise.Trigger();
	protected override AK.Wwise.BaseType WwiseType { get { return data; } }

	protected override void Awake()
	{
		base.Awake();
#if UNITY_EDITOR
		var reference = AkWwiseTypes.DragAndDropObjectReference;
		if (reference)
		{
			UnityEngine.GUIUtility.hotControl = 0;
			data.ObjectReference = reference;
			AkWwiseTypes.DragAndDropObjectReference = null;
		}
#endif
	}
	
	protected override void Start()
	{
#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer || AkUtilities.IsMigrating || !UnityEditor.EditorApplication.isPlaying)
			return;
#endif
		base.Start();
	}
	public override void HandleEvent(UnityEngine.GameObject in_gameObject)
	{
		var gameObj = useOtherObject && in_gameObject != null ? in_gameObject : gameObject;
		data.Post(gameObj);
	}

	#region WwiseMigration
#if UNITY_EDITOR
	public virtual bool Migrate(UnityEditor.SerializedObject obj)
	{
		//Didn't exist before, so no migration step as of yet
		return true;
	}
#endif
	#endregion
    }
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.