#if UNITY_EDITOR
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

[UnityEditor.CustomEditor(typeof(AkRoom))]
public class AkRoomInspector : UnityEditor.Editor
{
	private readonly AkUnityEventHandlerInspector m_PostEventHandlerInspector = new AkUnityEventHandlerInspector();

	private AkRoom m_AkRoom;
	private UnityEditor.SerializedProperty priority;
	private UnityEditor.SerializedProperty reverbAuxBus;
	private UnityEditor.SerializedProperty reverbLevel;
	private UnityEditor.SerializedProperty transmissionLoss;
	private UnityEditor.SerializedProperty roomToneEvent;
	private UnityEditor.SerializedProperty roomToneAuxSend;

	private void OnEnable()
	{
		m_PostEventHandlerInspector.Init(serializedObject, "triggerList", "Trigger On: ", false);

		m_AkRoom = target as AkRoom;

		reverbAuxBus = serializedObject.FindProperty("reverbAuxBus");
		reverbLevel = serializedObject.FindProperty("reverbLevel");
		transmissionLoss = serializedObject.FindProperty("transmissionLoss");
		priority = serializedObject.FindProperty("priority");
		roomToneEvent = serializedObject.FindProperty("roomToneEvent");
		roomToneAuxSend = serializedObject.FindProperty("roomToneAuxSend");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			UnityEditor.EditorGUILayout.PropertyField(reverbAuxBus);
			UnityEditor.EditorGUILayout.PropertyField(reverbLevel);
			UnityEditor.EditorGUILayout.PropertyField(transmissionLoss);

			UnityEditor.EditorGUILayout.PropertyField(priority);

			WetTransmissionCheck(m_AkRoom.gameObject);
		}

		UnityEditor.EditorGUILayout.LabelField("Room Tone", UnityEditor.EditorStyles.boldLabel);
		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			m_PostEventHandlerInspector.OnGUI();

			UnityEditor.EditorGUILayout.PropertyField(roomToneEvent);
			UnityEditor.EditorGUILayout.PropertyField(roomToneAuxSend);

			TriggerCheck(m_AkRoom);
		}

		AkRoomAwareObjectInspector.RigidbodyCheck(m_AkRoom.gameObject);

		serializedObject.ApplyModifiedProperties();
	}

	public static void WetTransmissionCheck(UnityEngine.GameObject gameObject)
	{
		if (AkWwiseEditorSettings.Instance.ShowSpatialAudioWarningMsg &&
			gameObject.GetComponent<AkSurfaceReflector>() == null &&
			gameObject.GetComponent<UnityEngine.Mesh>() == null)
		{
			// wet transmission supports box, sphere, capsule and mesh colliders
			bool bSupported = false;
			if (gameObject.GetComponent<UnityEngine.BoxCollider>() != null ||
				gameObject.GetComponent<UnityEngine.SphereCollider>() != null ||
				gameObject.GetComponent<UnityEngine.CapsuleCollider>() != null ||
				gameObject.GetComponent<UnityEngine.MeshCollider>() != null ||
				(gameObject.GetComponent<AkSurfaceReflector>() != null && gameObject.GetComponent<AkSurfaceReflector>().enabled))
			{
				bSupported = true;
			}

			if (bSupported == false)
			{
				UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

				using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
				{
					UnityEditor.EditorGUILayout.HelpBox(
						"Associating a geometry with this room for wet transmission is currently only supported with box, sphere, capsule and mesh colliders, or if the game object also has an enabled AkSurfaceReflector component.",
						UnityEditor.MessageType.Warning);
				}
			}
		}
	}

	public static void TriggerCheck(AkRoom room)
	{
		if (room.triggerList.Contains(AkTriggerHandler.DESTROY_TRIGGER_ID) ||
			room.triggerList.Contains(AkTriggerHandler.ON_DISABLE_TRIGGER_ID))
		{
			using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
			{
				UnityEditor.EditorGUILayout.HelpBox(
					"Room tones will only be posted on active and enabled gameobjects; it is not possible to post room tones on disable and on destroy.",
					UnityEditor.MessageType.Warning);
			}
		}
	}
}
#endif