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

[UnityEditor.CanEditMultipleObjects]
[UnityEditor.CustomEditor(typeof(AkRoomPortal))]
public class AkRoomPortalInspector : UnityEditor.Editor
{
	private UnityEditor.SerializedProperty initialState;
	private UnityEditor.SerializedProperty rooms;

	private readonly AkUnityEventHandlerInspector m_ClosePortalEventHandlerInspector = new AkUnityEventHandlerInspector();
	private readonly AkUnityEventHandlerInspector m_OpenPortalEventHandlerInspector = new AkUnityEventHandlerInspector();

	private AkRoomPortal m_roomPortal;

	[UnityEditor.MenuItem("GameObject/Wwise/Room Portal", false, 1)]
	public static void CreatePortal()
	{
		var portal = new UnityEngine.GameObject("RoomPortal");

		UnityEditor.Undo.AddComponent<AkRoomPortal>(portal);
		portal.GetComponent<UnityEngine.Collider>().isTrigger = true;

		UnityEditor.Selection.objects = new UnityEngine.Object[] { portal };
	}

	private void OnEnable()
	{
		initialState = serializedObject.FindProperty("initialState");
		rooms = serializedObject.FindProperty("rooms");

		m_OpenPortalEventHandlerInspector.Init(serializedObject, "triggerList", "Open On: ", false);
		m_ClosePortalEventHandlerInspector.Init(serializedObject, "closePortalTriggerList", "Close On: ", false);

		m_roomPortal = target as AkRoomPortal;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			UnityEditor.EditorGUILayout.PropertyField(initialState);
			m_OpenPortalEventHandlerInspector.OnGUI();
			m_ClosePortalEventHandlerInspector.OnGUI();
		}

		m_roomPortal.UpdateRooms();

		var labels = new[] { "Back Room", "Front Room" };
		var tooltips = new[] { "The highest priority, active and enabled AkRoom component overlapping the back surface of this AkRoomPortal.",
			"The highest priority, active and enabled AkRoom component overlapping the front surface of this AkRoomPortal." };

		var wasEnabled = UnityEngine.GUI.enabled;
		UnityEngine.GUI.enabled = false;

		for (var i = 0; i < AkRoomPortal.MAX_ROOMS_PER_PORTAL; i++)
			UnityEditor.EditorGUILayout.PropertyField(rooms.GetArrayElementAtIndex(i), new UnityEngine.GUIContent(labels[i], tooltips[i]), true);

		UnityEngine.GUI.enabled = wasEnabled;

		RoomCheck(m_roomPortal);

		serializedObject.ApplyModifiedProperties();
	}

	public static void RoomCheck(AkRoomPortal portal)
	{
		if (AkWwiseEditorSettings.Instance.ShowSpatialAudioWarningMsg)
		{
			if (!portal.IsValid)
			{
				UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

				UnityEditor.EditorGUILayout.HelpBox(
					"Portal placement is invalid. It will not be set in the Spatial Audio engine. The front and back Rooms of the Portal cannot be the same or have a ReverbZone-parent relationship.",
					UnityEditor.MessageType.Warning);
			}
		}
	}
}
#endif